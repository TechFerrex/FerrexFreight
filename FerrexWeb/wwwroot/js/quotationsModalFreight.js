// wwwroot/js/quotationsModalFreight.js

// —– Variables globales —–
let _googleMapsLoaded = false;

// —– Carga Google Maps dinámicamente —–
window.loadGoogleMapsForQuotations = function (apiKey) {
    if (_googleMapsLoaded || (typeof google !== 'undefined' && google.maps)) {
        _googleMapsLoaded = true;
        return;
    }

    if (!apiKey) {
        // API key no proporcionada
        return;
    }

    const script = document.createElement('script');
    script.src = `https://maps.googleapis.com/maps/api/js?key=${apiKey}&libraries=places`;
    script.async = true;
    script.defer = true;
    script.onload = () => {
        _googleMapsLoaded = true;
    };
    script.onerror = () => { };
    document.head.appendChild(script);
};

// —– Helpers —–

// Geocodifica direcciones de texto
function geocodeAddress(address) {
    return new Promise((resolve, reject) => {
        if (typeof google === 'undefined' || !google.maps) {
            reject("Mapa no disponible");
            return;
        }
        new google.maps.Geocoder().geocode({ address }, (results, status) => {
            if (status === "OK" && results[0]) resolve(results[0].geometry.location);
            else reject(status);
        });
    });
}

// Cierra el modal
window.closeViewModal = function () {
    document.getElementById("viewModal").style.display = "none";
};

// Toast simple sin dependencia de Toastify - Optimizado para evitar memory leaks
window.showToastSimple = function (type, message) {
    // Crear contenedor si no existe
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.style.cssText = 'position:fixed;top:20px;right:20px;z-index:9999;';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    const bgColor = type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : '#17a2b8';
    toast.style.cssText = `background:${bgColor};color:#fff;padding:12px 20px;border-radius:5px;margin-bottom:10px;box-shadow:0 2px 10px rgba(0,0,0,0.2);transition:opacity 0.3s;`;
    toast.textContent = message; // Usar textContent en lugar de innerHTML para seguridad

    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        setTimeout(() => {
            toast.remove();
            // Limpiar contenedor si está vacío
            if (container && container.children.length === 0) {
                container.remove();
            }
        }, 300);
    }, 3000);
};

// —– Muestra la cotización —–
// Ahora recibe también stopsJson (JSON con Address, Lat, Lng)
window.showQuotationModal = async function (origin, destination, stopsJson) {
    // Verificar que Google Maps esté cargado
    if (typeof google === 'undefined' || !google.maps) {
        showToastSimple("error", "El mapa aún está cargando, intenta de nuevo en unos segundos");
        return;
    }

    // 1) Rellenar textos
    document.getElementById("modalOrigin").innerText = origin;
    document.getElementById("modalDestination").innerText = destination;

    // 2) Parsear paradas
    let stops = [];
    try {
        stops = JSON.parse(stopsJson) || [];
    } catch {
        stops = [];
    }

    // 3) Resolver coordenadas (fallback a geocode si lat/lng === 0)
    const resolvedStops = await Promise.all(stops.map(async s => {
        if ((s.Lat || 0) !== 0 || (s.Lng || 0) !== 0) {
            return new google.maps.LatLng(s.Lat, s.Lng);
        }
        try {
            return await geocodeAddress(s.Address);
        } catch {
            return null;
        }
    }));
    const validStops = resolvedStops.filter(loc => loc);

    // 4) Inicializar mapa
    const mapContainer = document.getElementById("modalMapContainer");
    if (!mapContainer) return;

    const map = new google.maps.Map(mapContainer, {
        center: { lat: 14.0723, lng: -87.1921 },
        zoom: 7,
        mapTypeControl: false,
        streetViewControl: false,
        fullscreenControl: false
    });

    // 5) Configurar Directions sin marcadores por defecto
    const ds = new google.maps.DirectionsService();
    const dr = new google.maps.DirectionsRenderer({
        map: map,
        suppressMarkers: true,
        polylineOptions: {
            strokeColor: "#007bff",
            strokeOpacity: 0.8,
            strokeWeight: 5
        }
    });

    // 6) Waypoints para la ruta
    const waypoints = validStops.map(loc => ({ location: loc, stopover: true }));

    // 7) Calcular ruta
    ds.route({
        origin,
        destination,
        waypoints,
        travelMode: google.maps.TravelMode.DRIVING
    }, (resp, status) => {
        if (status !== "OK") {
            showToastSimple("error", "No se pudo calcular la ruta");
            return;
        }
        dr.setDirections(resp);

        // Ajustar bounds
        const bounds = new google.maps.LatLngBounds();
        resp.routes[0].legs.forEach(leg => {
            bounds.extend(leg.start_location);
            bounds.extend(leg.end_location);
        });
        map.fitBounds(bounds);

        // Marcadores personalizados:
        // Origen (verde)
        new google.maps.Marker({
            position: resp.routes[0].legs[0].start_location,
            map,
            title: "Origen",
            icon: {
                path: google.maps.SymbolPath.CIRCLE,
                scale: 10,
                fillColor: "#28a745",
                fillOpacity: 1,
                strokeColor: "#fff",
                strokeWeight: 2
            }
        });

        // Paradas numeradas (azul)
        validStops.forEach((loc, i) => {
            new google.maps.Marker({
                position: loc,
                map,
                label: { text: `${i + 1}`, color: "#fff", fontWeight: "bold" },
                title: `Parada ${i + 1}`
            });
        });

        // Destino (rojo)
        const lastLeg = resp.routes[0].legs.slice(-1)[0];
        new google.maps.Marker({
            position: lastLeg.end_location,
            map,
            title: "Destino",
            icon: {
                path: google.maps.SymbolPath.CIRCLE,
                scale: 10,
                fillColor: "#dc3545",
                fillOpacity: 1,
                strokeColor: "#fff",
                strokeWeight: 2
            }
        });
    });

    // 8) Mostrar modal
    const modal = document.getElementById("viewModal");
    modal.style.display = "block";

    // 9) Enlazar cierre al "×" - usando addEventListener con once o reemplazando handler
    const btnClose = modal.querySelector(".close");
    if (btnClose && !btnClose._hasCloseHandler) {
        btnClose.addEventListener('click', window.closeViewModal);
        btnClose._hasCloseHandler = true;
    }

    // 10) Permitir cerrar si hacen click fuera de la caja - evitar múltiples listeners
    if (!modal._hasClickHandler) {
        modal.addEventListener('click', function (e) {
            if (e.target === modal) {
                window.closeViewModal();
            }
        });
        modal._hasClickHandler = true;
    }
};
