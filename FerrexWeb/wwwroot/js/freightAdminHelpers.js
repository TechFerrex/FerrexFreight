
function showRouteModal(origin, destination, distance, truckType) {
    document.getElementById("modalOrigin").innerText = origin;
    document.getElementById("modalDestination").innerText = destination;

    // Inicializamos el mapa en el contenedor del modal
    var map = new google.maps.Map(document.getElementById("modalMapContainer"), {
        center: { lat: 14.0723, lng: -87.1921 }, // Tegucigalpa, Honduras
        zoom: 7,
        mapTypeControl: true,
        streetViewControl: false,
        fullscreenControl: true
    });

    var directionsService = new google.maps.DirectionsService();
    var directionsRenderer = new google.maps.DirectionsRenderer({
        suppressMarkers: false,
        polylineOptions: {
            strokeColor: "#1E88E5",
            strokeWeight: 5,
            strokeOpacity: 0.8
        }
    });

    directionsRenderer.setMap(map);

    directionsService.route({
        origin: origin,
        destination: destination,
        travelMode: google.maps.TravelMode.DRIVING
    }, function (response, status) {
        if (status === 'OK') {
            directionsRenderer.setDirections(response);

            // Calcular y mostrar detalles de la ruta
            var route = response.routes[0];
            var distanceValue = route.legs[0].distance.text;
            var durationValue = route.legs[0].duration.text;

            document.getElementById("routeDistance").innerText = distanceValue;
            document.getElementById("routeDuration").innerText = durationValue;

            // Calcular estimación de combustible basado en el tipo de camión
            var distanceInKm = distance;
            var fuelConsumption;

            switch (truckType.toLowerCase()) {
                case "small":
                    fuelConsumption = 12; // km/L
                    break;
                case "medium":
                    fuelConsumption = 8; // km/L
                    break;
                case "large":
                    fuelConsumption = 5; // km/L
                    break;
                default:
                    fuelConsumption = 10; // km/L
            }

            var fuelNeeded = (distanceInKm / fuelConsumption).toFixed(2);
            document.getElementById("fuelEstimate").innerText = fuelNeeded + " L";
        } else {
            console.error("Error al obtener la ruta: " + status);
            document.getElementById("routeDetails").innerHTML =
                "<div class='alert alert-danger'><i class='fas fa-exclamation-triangle mr-2'></i>Error al calcular la ruta. Verifique las direcciones.</div>";
        }
    });

    // Mostrar el modal usando Bootstrap
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
        console.error('dotNetRef no inicializado');
    }
}