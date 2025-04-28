// wwwroot/js/quotationsModal.js

// Función invocada desde Blazor
window.showQuotationModal = function (origin, destination) {
    document.getElementById("modalOrigin").innerText = origin;
    document.getElementById("modalDestination").innerText = destination;

    // carga el mapa dentro del modal
    const map = new google.maps.Map(
        document.getElementById("modalMapContainer"), {
        center: { lat: 14.0723, lng: -87.1921 },
        zoom: 7,
        mapTypeControl: false,
        streetViewControl: false,
        fullscreenControl: false
    }
    );
    const ds = new google.maps.DirectionsService();
    const dr = new google.maps.DirectionsRenderer({ suppressMarkers: false });
    dr.setMap(map);
    ds.route({
        origin: origin,
        destination: destination,
        travelMode: google.maps.TravelMode.DRIVING
    }, (resp, status) => {
        if (status === 'OK') dr.setDirections(resp);
        else console.error("Ruta:", status);
    });

    // muestra el modal
    document.getElementById("viewModal").style.display = "block";
};

// Cierra el modal
window.closeViewModal = function () {
    document.getElementById("viewModal").style.display = "none";
};

// Al arrancar, engancha el listener al “botón” de cerrar
document.addEventListener("DOMContentLoaded", () => {
    const btn = document.querySelector("#viewModal .close");
    if (btn) btn.addEventListener("click", window.closeViewModal);
});

// delegar el click en el close del modal
document.addEventListener("click", function (e) {
    if (e.target.matches("#viewModal .close")) {
        e.preventDefault();
        closeViewModal();
    }
});
