// Helper para geocodificar direcciones de texto
function geocodeAddress(address) {
    return new Promise((resolve, reject) => {
        new google.maps.Geocoder().geocode({ address }, (results, status) => {
            if (status === "OK" && results[0]) {
                resolve(results[0].geometry.location);
            } else {
                reject(status);
            }
        });
    });
}

async function showRouteModal(origin, destination, distance, truckType, stopsJson) {
    // 1) Rellenar texto
    document.getElementById("modalOrigin").innerText = origin;
    document.getElementById("modalDestination").innerText = destination;

    // 2) Parsear JSON de paradas
    let stops = [];
    try {
        stops = JSON.parse(stopsJson);
        if (!Array.isArray(stops)) stops = [];
    } catch (e) {
        // stopsJson inválido
        stops = [];
    }

    // 3) Resolver coordenadas de cada parada
    const resolvedStops = await Promise.all(stops.map(async s => {
        // Si ya tiene lat/lng válidos, los usamos
        if ((s.Latitude || 0) !== 0 || (s.Longitude || 0) !== 0) {
            return new google.maps.LatLng(s.Latitude, s.Longitude);
        }
        // Si no, geocodificamos la dirección
        try {
            return await geocodeAddress(s.Address);
        } catch {
            // No se pudo geocodificar parada
            return null;
        }
    }));
    const waypoints = resolvedStops
        .filter(loc => loc)
        .map(loc => ({ location: loc, stopover: true }));

    // 4) Inicializar mapa
    const map = new google.maps.Map(
        document.getElementById("modalMapContainer"), {
        center: { lat: 14.0723, lng: -87.1921 },
        zoom: 7,
        mapTypeControl: true,
        streetViewControl: false,
        fullscreenControl: true
    }
    );

    // 5) Configurar DirectionsRenderer (sin marcadores automáticos)
    const ds = new google.maps.DirectionsService();
    const dr = new google.maps.DirectionsRenderer({
        map: map,
        suppressMarkers: true,
        polylineOptions: {
            strokeColor: "#1E88E5",
            strokeOpacity: 0.8,
            strokeWeight: 5
        }
    });

    // 6) Calcular la ruta con waypoints
    ds.route({
        origin: origin,
        destination: destination,
        waypoints: waypoints,
        travelMode: google.maps.TravelMode.DRIVING
    }, (response, status) => {
        if (status !== "OK") {
            // Error al calcular ruta
            return;
        }
        dr.setDirections(response);

        // 7) Ajustar viewport para incluir toda la ruta
        const bounds = new google.maps.LatLngBounds();
        response.routes[0].legs.forEach(leg => {
            bounds.extend(leg.start_location);
            bounds.extend(leg.end_location);
        });
        map.fitBounds(bounds);

        // 8) Añadir marcadores personalizados
        // Origen
        new google.maps.Marker({
            position: response.routes[0].legs[0].start_location,
            map: map,
            title: "Origen"
        });
        // Paradas numeradas
        waypoints.forEach((wp, i) => {
            new google.maps.Marker({
                position: wp.location,
                map: map,
                label: {
                    text: String(i + 1),
                    color: "#fff",
                    fontWeight: "bold"
                },
                title: `Parada ${i + 1}`
            });
        });
        // Destino
        const lastLeg = response.routes[0].legs.slice(-1)[0];
        new google.maps.Marker({
            position: lastLeg.end_location,
            map: map,
            title: "Destino"
        });

        // 9) Mostrar detalles de distancia y duración
        const totalDistanceMeters = response.routes[0].legs
            .reduce((sum, leg) => sum + leg.distance.value, 0);
        const totalDurationSec = response.routes[0].legs
            .reduce((sum, leg) => sum + leg.duration.value, 0);

        document.getElementById("routeDistance").innerText =
            (totalDistanceMeters / 1000).toFixed(2) + " km";
        document.getElementById("routeDuration").innerText =
            Math.ceil(totalDurationSec / 60) + " min";

        // 10) Estimar combustible según tipo de camión
        const distKm = parseFloat(distance) || (totalDistanceMeters / 1000);
        let kmPerLiter;
        switch (truckType.toLowerCase()) {
            case "small": kmPerLiter = 12; break;
            case "medium": kmPerLiter = 8; break;
            case "large": kmPerLiter = 5; break;
            default: kmPerLiter = 10; break;
        }
        document.getElementById("fuelEstimate").innerText =
            (distKm / kmPerLiter).toFixed(2) + " L";
    });

    // 11) Mostrar el modal Bootstrap
    $('#routeModal').modal('show');
}


function showDetailsModal(dotNetRef) {
    window._quotRef = dotNetRef;      //  ←  la dejamos global
    $('#detailsModal').modal('show');
}

function showDeleteModal(quotationId, dotNetRef) {
    document.getElementById("deleteQuotationId").innerText = quotationId;

    const deleteBtn = document.getElementById("confirmDeleteBtn");
    deleteBtn.onclick = () => {
        // Llama al método de instancia
        dotNetRef.invokeMethodAsync('DeleteQuotation', quotationId)
            .then(() => $('#deleteModal').modal('hide'));
    };

    $('#deleteModal').modal('show');
}


// Función para mostrar notificaciones toast
function showToast(type, message) {
    if (typeof Toastify === 'undefined') {
        alert(message);
        return;
    }

    var bgColor;
    var icon;

    switch (type) {
        case "success":
            bgColor = "#28a745";
            icon = "<i class='fas fa-check-circle mr-2'></i>";
            break;
        case "error":
            bgColor = "#dc3545";
            icon = "<i class='fas fa-exclamation-circle mr-2'></i>";
            break;
        case "warning":
            bgColor = "#ffc107";
            icon = "<i class='fas fa-exclamation-triangle mr-2'></i>";
            break;
        default:
            bgColor = "#17a2b8";
            icon = "<i class='fas fa-info-circle mr-2'></i>";
    }

    Toastify({
        text: icon + message,
        duration: 3000,
        close: true,
        gravity: "top",
        position: "right",
        backgroundColor: bgColor,
        escapeMarkup: false
    }).showToast();
}

// freightAdminHelpers.js  (ES‑module)
//export function exportTableToExcel(tableID, fileName) {
//    const table = document.getElementById(tableID);
//    if (!table) { alert("Tabla no encontrada"); return; }

//    // opción raw / display recomendada
//    const wb = XLSX.utils.table_to_book(table, { sheet: "Cotizaciones", raw: true, display: true });
//    XLSX.writeFile(wb, `${fileName}.xlsx`, { compression: true });
//}


function printRouteDetails() {
    var origin = document.getElementById("modalOrigin").innerText;
    var destination = document.getElementById("modalDestination").innerText;
    var distance = document.getElementById("routeDistance").innerText;
    var duration = document.getElementById("routeDuration").innerText;
    var fuel = document.getElementById("fuelEstimate").innerText;

    var printWindow = window.open('', '_blank');

    // Formatear la fecha actual para el informe
    var currentDate = new Date().toLocaleDateString('es-HN');

    // Crear el contenido HTML para la impresión
    var htmlContent = `
<!DOCTYPE html>
<html>
<head>
    <title>Detalles de Ruta</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            line-height: 1.6;
        }

        .header {
            text-align: center;
            margin-bottom: 20px;
        }

        .container {
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }

        .detail-box {
            border: 1px solid #ddd;
            padding: 15px;
            margin-bottom: 20px;
        }

        .detail-item {
            margin-bottom: 10px;
        }

        .footer {
            text-align: center;
            margin-top: 30px;
            font-size: 12px;
            color: #777;
        }

        .stat-container {
            display: flex;
            justify-content: space-between;
            margin-top: 20px;
        }

        .stat-box {
            text-align: center;
            padding: 10px;
            border: 1px solid #ddd;
            flex: 1;
            margin: 0 5px;
        }

        .stat-value {
            font-size: 18px;
            font-weight: bold;
        }

        .stat-label {
            font-size: 12px;
            color: #777;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h2>Detalles de Ruta de Flete</h2>
            <p>Fecha: ${currentDate}</p>
        </div>

        <div class="detail-box">
            <div class="detail-item"><strong>Origen:</strong> ${origin}</div>
            <div class="detail-item"><strong>Destino:</strong> ${destination}</div>
        </div>

        <div class="stat-container">
            <div class="stat-box">
                <div class="stat-value">${distance}</div>
                <div class="stat-label">Distancia</div>
            </div>
            <div class="stat-box">
                <div class="stat-value">${duration}</div>
                <div class="stat-label">Tiempo Estimado</div>
            </div>
            <div class="stat-box">
                <div class="stat-value">${fuel}</div>
                <div class="stat-label">Combustible Est.</div>
            </div>
        </div>

        <div class="footer">
            <p>FerrexWeb - Sistema de Cotización de Fletes</p>
        </div>
    </div>
    <script>
        window.onload = function() { window.print(); }
    </script>
</body>
</html>`;

    printWindow.document.write(htmlContent);
    printWindow.document.close();
}

/* helpers de freightAdminHelpers.js  (fragmento) */

/*---------------------------------------------------------------
 *  Llamada desde C#:  JS.InvokeVoidAsync("buildAndPrintQuotation", …)
 *--------------------------------------------------------------*/
function buildAndPrintQuotation(   // ← ¡ahora recibe los datos!
    quotationNumber,               // string / int
    userId,                        // string / int
    origin, destination,           // string
    km,                            // number
    freightDate,                   // string (“dd/MM/yyyy”)
    truckType,                     // string
    totalCostHnl) {                // string (“L12,345.00”)

    const today = new Date().toLocaleDateString('es-HN');

    const html = `
<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<title>Cotización #${quotationNumber}</title>
<style>
    body{font-family:Arial,sans-serif;line-height:1.5}
    h2{text-align:center;margin:0 0 20px}
    .container{max-width:700px;margin:0 auto;padding:20px}
    .box{border:1px solid #ddd;padding:15px;margin-bottom:20px}
    .stats{display:flex;gap:10px}
    .stats div{flex:1;border:1px solid #ddd;padding:10px;text-align:center}
    .stats b{font-size:18px}
    .footer{text-align:center;font-size:12px;color:#777;margin-top:30px}
</style>
</head>
<body>
    <div class="container">
        <h2>Detalles de Cotización</h2>
        <p style="text-align:center">Impreso: ${today}</p>

        <div class="box">
            <strong>Número:</strong> ${quotationNumber}<br>
            <strong>ID Usuario:</strong> ${userId}<br>
            <strong>Fecha de flete:</strong> ${freightDate}<br>
            <strong>Tipo de camión:</strong> ${truckType}
        </div>

        <div class="box">
            <strong>Origen:</strong> ${origin}<br>
            <strong>Destino:</strong> ${destination}
        </div>

        <div class="stats">
            <div><b>${km.toFixed(2)} km</b><br>Distancia</div>
            <div><b>${totalCostHnl}</b><br>Total (HNL)</div>
        </div>

        <div class="footer">FerrexWeb — Sistema de Cotizaciones</div>
    </div>

    <script>window.onload = () => window.print();<\/script>
</body>
</html>`;

    const win = window.open('', '_blank');
    win.document.write(html);
    win.document.close();
}

/* botón Imprimir en el modal de detalles */
function printQuotationDetails() {
    if (window._quotRef) {
        // Invoca el método de instancia del componente Blazor
        window._quotRef.invokeMethodAsync('buildAndPrintQuotation');
    } else {
        // dotNetRef no disponible
    }
}