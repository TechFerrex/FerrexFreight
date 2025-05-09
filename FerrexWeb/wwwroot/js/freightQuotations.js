window.freightQuotations = {
    showDetails: function (origin, destination, lat, lng) {
        // Rellena los textos
        document.getElementById("modalOrigin").textContent = origin;
        document.getElementById("modalDestination").textContent = destination;

        // Inicializa el mapa
        var center = { lat: lat || 14.0723, lng: lng || -87.1921 };
        var map = new google.maps.Map(document.getElementById("modalMap"), {
            center: center,
            zoom: 7,
            mapTypeControl: false,
            streetViewControl: false,
            fullscreenControl: false
        });

        // Direcciones
        var directionsService = new google.maps.DirectionsService();
        var directionsRenderer = new google.maps.DirectionsRenderer({ map: map });
        directionsService.route({
            origin: origin,
            destination: destination,
            travelMode: google.maps.TravelMode.DRIVING
        }, function (response, status) {
            if (status === 'OK') {
                directionsRenderer.setDirections(response);
            } else {
                console.error("Error al obtener ruta:", status);
            }
        });

        // Abre el modal de Bootstrap
        var modal = new bootstrap.Modal(document.getElementById("detailsModal"));
        modal.show();
    }
};
