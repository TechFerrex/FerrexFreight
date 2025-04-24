
        (function(){
        let map, directionsService, directionsRenderer;
        let originMarker = null, destinationMarker = null;
        let isMapSelectionMode = false, currentSelection = "";
        const truckRates = { small: 30, medium: 50, pickup:20 };
        window.dotNetRef = null;
        window.calculationData = {};
            window.setDotNetReference = function(dotNetObjectRef) {
                console.log("setDotNetReference invocado con:", dotNetObjectRef);
                window.dotNetRef = dotNetObjectRef;
                return true;// Para confirmar que se estableció
    }
        function initMap(dotNetObjectRef) {
    console.log("initMap recibió dotNetObjectRef:", dotNetObjectRef);
    window.dotNetRef = dotNetObjectRef; // Aseguramos que se asigne globalmente
        const hondurasCenter = { lat: 14.0723, lng: -87.1921 };

        map = new google.maps.Map(document.getElementById("map"), {
            center: hondurasCenter,
            zoom: 7,
            mapTypeControl: false,
            streetViewControl: false,
            fullscreenControl: false
        });

        directionsService = new google.maps.DirectionsService();
        directionsRenderer = new google.maps.DirectionsRenderer({ suppressMarkers: true });
        directionsRenderer.setMap(map);

        const originInput = document.getElementById("origin-input");
        const destinationInput = document.getElementById("destination-input");

        if (originInput) {
            const autocompleteOrigin = new google.maps.places.Autocomplete(originInput, {
                componentRestrictions: { country: "hn" },
                fields: ["geometry", "name"]
            });
            autocompleteOrigin.addListener("place_changed", function () {
                const place = autocompleteOrigin.getPlace();
                if (!place.geometry) {
                    console.warn("No se encontraron detalles para: " + place.name);
                    return;
                }
                map.setCenter(place.geometry.location);
                map.setZoom(15);
                if (originMarker) { originMarker.setMap(null); }
                originMarker = new google.maps.Marker({
                    position: place.geometry.location,
                    map: map,
                    title: "Origen"
                });
            });
            // Agrega un listener para cancelar la selección del mapa al enfocar el input
            originInput.addEventListener("focus", function () {
                isMapSelectionMode = false;
                updateSelectionTip("");
            });
        }

        if (destinationInput) {
            const autocompleteDestination = new google.maps.places.Autocomplete(destinationInput, {
                componentRestrictions: { country: "hn" },
                fields: ["geometry", "name"]
            });
            autocompleteDestination.addListener("place_changed", function () {
                const place = autocompleteDestination.getPlace();
                if (!place.geometry) {
                    console.warn("No se encontraron detalles para: " + place.name);
                    return;
                }
                map.setCenter(place.geometry.location);
                map.setZoom(15);
                if (destinationMarker) { destinationMarker.setMap(null); }
                destinationMarker = new google.maps.Marker({
                    position: place.geometry.location,
                    map: map,
                    title: "Destino"
                });
            });
            // Cancela el modo selección cuando se enfoca el input de destino
            destinationInput.addEventListener("focus", function () {
                isMapSelectionMode = false;
                updateSelectionTip("");
            });
        }

        map.addListener("click", function (event) {
            if (isMapSelectionMode) {
                if (currentSelection === "origin") {
                    if (originMarker) { originMarker.setMap(null); }
                    originMarker = new google.maps.Marker({
                        position: event.latLng,
                        map: map,
                        title: "Origen (seleccionado en mapa)"
                    });
                    originInput.value = event.latLng.lat().toFixed(5) + ", " + event.latLng.lng().toFixed(5);
                    currentSelection = "destination";
                    updateSelectionTip("Ahora selecciona el destino en el mapa");
                } else if (currentSelection === "destination") {
                    if (destinationMarker) { destinationMarker.setMap(null); }
                    destinationMarker = new google.maps.Marker({
                        position: event.latLng,
                        map: map,
                        title: "Destino (seleccionado en mapa)"
                    });
                    destinationInput.value = event.latLng.lat().toFixed(5) + ", " + event.latLng.lng().toFixed(5);
                    isMapSelectionMode = false;
                    currentSelection = "";
                    updateSelectionTip("");
                }
            }
        });
    }
      function loadGoogleMapsScript() {
      const prev = document.getElementById('googleMaps');
      if (prev) prev.remove();

      showMapErrorModal(false);
      const script = document.createElement('script');
      script.id = 'googleMaps';
          script.src = 'https://maps.googleapis.com/maps/api/js?key=AIzaSyDPNM-zV0g6n0fZca7P3DNAB24goUwb_ro&libraries=places&callback=initMap';
      script.async = true;
      script.defer = true;
      script.onload = () => initMap(window.dotNetRef);
      script.onerror = () => {
        showMapErrorModal(true, 'No se pudo cargar el mapa. Reintentando en 3s...');
        setTimeout(loadGoogleMapsScript, 3000);
      };
      document.head.appendChild(script);
    }

    function showMapErrorModal(show, message) {
      let modal = document.getElementById('mapErrorModal');
      if (!modal) {
        modal = document.createElement('div');
        modal.id = 'mapErrorModal';
        modal.classList.add('modal');
        modal.innerHTML = `
          <div class="modal-content">
            <span class="close" onclick="document.getElementById('mapErrorModal').style.display='none'">&times;</span>
            <div class="alert alert-danger p-3" id="mapErrorMessage"></div>
          </div>`;
        document.body.appendChild(modal);
      }
      modal.style.display = show ? 'block' : 'none';
      if (message) document.getElementById('mapErrorMessage').innerText = message;
    }

        function updateSelectionTip(message) {
            const tip = document.getElementById("selectionTip");
            tip.style.display = message ? "block" : "none";
            if (message) tip.innerText = message;
        }

        function activateMapSelection() {
            isMapSelectionMode = true;
            currentSelection = "origin";
            if (originMarker) { originMarker.setMap(null); originMarker = null; }
            if (destinationMarker) { destinationMarker.setMap(null); destinationMarker = null; }
            document.getElementById("origin-input").value = "";
            document.getElementById("destination-input").value = "";
            updateSelectionTip("Selecciona el origen en el mapa");
            hideAlert();
            console.info("Modo de selección en mapa activado. Selecciona el origen.");
        }

        function validateLocation(latLng, callback) {
            const geocoder = new google.maps.Geocoder();
            geocoder.geocode({ location: latLng }, (results, status) => {
                if (status === "OK" && results[0]) {
                    let isValid = true;
                    results[0].address_components.forEach(component => {
                        if (component.types.includes("administrative_area_level_1")) {
                            const adminArea = component.long_name.toLowerCase();
                            if (adminArea.includes("gracias a dios") || adminArea.includes("islas")) {
                                isValid = false;
                            }
                        }
                    });
                    callback(isValid);
                } else {
                    callback(true);
                }
            });
        }
            // Añade esto dentro de tu función JavaScript que ya tienes
    function initTruckSelectionEffects() {
        // Observador de mutaciones para detectar cambios en la clase .active
        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                if (mutation.attributeName === 'class') {
                    const target = mutation.target;
                    if (target.classList.contains('active')) {
                        // Reinicia la animación de destello
                        target.style.animation = 'none';
                        target.offsetHeight; // Trigger reflow
                        target.style.animation = null;

                        // Efecto de "ondulación" al hacer clic
                        createRippleEffect(target);
                    }
                }
            });
        });

        // Observa todas las tarjetas de camiones
        document.querySelectorAll('.truck-card').forEach(card => {
            observer.observe(card, { attributes: true });

            // Añade el evento de clic para el efecto de ondulación
            card.addEventListener('click', function(e) {
                createRippleEffect(this);
            });
        });
    }

    // Efecto de ondulación al hacer clic
    function createRippleEffect(element) {
        const ripple = document.createElement('span');
        ripple.classList.add('ripple');

        // Estilos inline para el efecto
        ripple.style.position = 'absolute';
        ripple.style.borderRadius = '50%';
        ripple.style.background = 'rgba(255, 255, 255, 0.7)';
        ripple.style.transform = 'scale(0)';
        ripple.style.animation = 'ripple 0.6s linear';
        ripple.style.pointerEvents = 'none';

        const rect = element.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        ripple.style.width = ripple.style.height = `${size}px`;
        ripple.style.left = '0px';
        ripple.style.top = '0px';
        // Añadir al elemento y limpiar después
        element.appendChild(ripple);
        setTimeout(() => ripple.remove(), 600);
    }

    // Añade keyframes para la animación de ripple
        function addRippleStyle() {
            if (!document.getElementById('rippleStyleSheet')) {
                const style = document.createElement('style');
                style.id = 'rippleStyleSheet';
                style.textContent = `
            @@keyframes ripple {
                to {
                    transform: scale(4);
                    opacity: 0;
                }
            }
        `;
                document.head.appendChild(style);
            }
        }


    // Llamar a estas funciones después de que se cargue la página
    window.addEventListener('DOMContentLoaded', () => {
        addRippleStyle();
        // Inicializa los efectos después de que Blazor termine de renderizar
        setTimeout(initTruckSelectionEffects, 500);
    });

    // También modifica la función SelectTruckType en Blazor para llamar a este método
    function applyTruckSelectionEffect(type) {
        // Este método se llamará desde el código de Blazor
        document.querySelectorAll('.truck-card').forEach(card => {
            if (card.dataset.truckType === type) {
                // Asegúrate de que la clase active esté aplicada
                if (!card.classList.contains('active')) {
                    card.classList.add('active');
                }
            } else {
                card.classList.remove('active');
            }
        });
    }

    // Agrégalo a las funciones expuestas
      window.applyTruckSelectionEffect = applyTruckSelectionEffect;
          console.log("Datos de cálculo:", window.calculationData);
    console.log("dotNetRef disponible:", window.dotNetRef !== null && window.dotNetRef !== undefined);

    function calculateRoute() {
        hideAlert();
        const freightDateInput = document.getElementById("freight-date");
        if (!freightDateInput || freightDateInput.value === "") {
            showAlert("Por favor, selecciona la fecha de programación.");
            return;
        }

        if (!originMarker || !destinationMarker) {
            showAlert("Por favor, selecciona ambos puntos: origen y destino.");
            return;
        }

        const originPos = originMarker.getPosition();
        const destinationPos = destinationMarker.getPosition();

        // 1) Validar ORIGEN
        validateAllowedLocation(originPos, function(originAllowed) {
            if (!originAllowed) {
                showAlert("Origen fuera de zona: sólo Villanueva, San Pedro Sula, Chamelecón, Cofradía o Choloma.");
                return;
            }

            // 2) Validar DESTINO
            validateAllowedLocation(destinationPos, function(destAllowed) {
                if (!destAllowed) {
                    showAlert("Destino fuera de zona: sólo Villanueva, San Pedro Sula, Chamelecón, Cofradía o Choloma.");
                    return;
                }

                // 3) Ambos permitidos: calculamos la ruta
                directionsService.route({
                    origin: originPos,
                    destination: destinationPos,
                    travelMode: google.maps.TravelMode.DRIVING
                }, (response, status) => {
                    if (status !== "OK") {
                        showAlert("Error al calcular la ruta. Intenta nuevamente.");
                        return;
                    }

                    directionsRenderer.setDirections(response);
                    const leg = response.routes[0].legs[0];
                    const distanceKm = leg.distance.value / 1000;
                    const truckType = document.getElementById("truck-type").value;
                    const costPerKm = truckRates[truckType];

                    // Cálculo de costos
                    let baseCost = 0;
                    if (distanceKm <= 6) {
                        if (truckType === "small") baseCost = 600;
                        else if (truckType === "medium") baseCost = 2200;
                        else if (truckType === "pickup") baseCost = 350;
                    } else {
                        if (truckType === "small") baseCost = 600 + (distanceKm - 6) * costPerKm;
                        else if (truckType === "medium") baseCost = 2200 + (distanceKm - 6) * costPerKm;
                        else if (truckType === "pickup") baseCost = 350 + (distanceKm - 6) * costPerKm;
                    }

                    const insuranceOption = document.getElementById("insurance-option")?.value ?? "none";
                    let insuranceCost = 0;
                    if (insuranceOption === "basic") insuranceCost = baseCost * 0.05;
                    else if (insuranceOption === "premium") insuranceCost = baseCost * 0.10;

                    const finalCost = baseCost + insuranceCost;

                    const truckLabels = {
                        pickup: "Tipo Pickup",
                        small: "Camión Pequeño (6 ton)",
                        medium: "Camión Mediano (12 ton)",
                        // …
                    };
                    const truckLabel = truckLabels[truckType] || truckType;

                    window.calculationData = {
                        distanceKm,
                        costPerKm,
                        baseCost,
                        insuranceCost,
                        finalCost,
                        truckType,
                        insuranceOption,
                        origin: document.getElementById("origin-input").value,
                        destination: document.getElementById("destination-input").value,
                        freightDate: freightDateInput.value,
                        freightLatitude: originPos.lat(),
                        freightLongitude: originPos.lng()
                    };

                    const modalContent =`
                        <p><strong>Fecha de Programación:</strong> ${freightDateInput.value}</p>
                        <p><strong>Distancia:</strong> ${distanceKm.toFixed(2)} km</p>
                        <p><strong>Tipo de Camión:</strong> ${truckLabel}</p>
                        <p><strong>Costo de flete:</strong> L${baseCost.toFixed(2)}</p>
                        <hr>
                        <p class="h5 text-center"><strong>Costo total: L${finalCost.toFixed(2)}</strong></p>
                        <div class="text-center mt-3">
                            <button class="btn btn-primary" onclick="actionCotizar()">Cotizar</button>
                            <button class="btn btn-success" onclick="actionOrdenar()">Hacer Flete</button>
                            <button class="btn btn-secondary" onclick="actionSalir()">Salir</button>
                        </div>`
                    ;
                    document.getElementById("modalBody").innerHTML = modalContent;
                    openModal();
                        console.log("Modal abierto con dotNetRef:", window.dotNetRef);
    console.log("Funciones de acción disponibles:", {
        actionCotizar: typeof window.actionCotizar === 'function',
        actionOrdenar: typeof window.actionOrdenar === 'function',
        actionSalir: typeof window.actionSalir === 'function'
    });
                });
            });
        });
    }

    function validateAllowedLocation(latLng, callback) {
        const geocoder = new google.maps.Geocoder();
        geocoder.geocode({ location: latLng }, (results, status) => {
            if (status === "OK" && results[0]) {
                const allowed = [
                    "villanueva",
                    "san pedro sula",
                    "chamelecon",
                    "cofradia",
                    "choloma"
                ];
                let isAllowed = false;
                results[0].address_components.forEach(component => {
                    if (component.types.includes("administrative_area_level_2") ||
                        component.types.includes("locality")) {
                        const name = component.long_name.toLowerCase();
                        if (allowed.some(a => name.includes(a))) {
                            isAllowed = true;
                        }
                    }
                });
                callback(isAllowed);
            } else {
                callback(false);
            }
        });
    }

       window.actionCotizar = function() {
        if (window.dotNetRef && window.calculationData) {
            window.dotNetRef.invokeMethodAsync('SaveFreightQuotation',
                window.calculationData.distanceKm,
                window.calculationData.costPerKm,
                window.calculationData.baseCost,
                window.calculationData.insuranceCost,
                window.calculationData.finalCost,
                window.calculationData.truckType,
                window.calculationData.insuranceOption,
                window.calculationData.origin,
                window.calculationData.destination,
                window.calculationData.freightDate,
                window.calculationData.freightLatitude,
                window.calculationData.freightLongitude
            ).then(result => {
                // Limpia el mensaje de error anterior, si existe
                const errorElem = document.getElementById("modalError");
                errorElem.style.display = "none";
                errorElem.innerText = "";

                // Si se retorna el mensaje de error, se muestra dentro del modal
                if (result.includes("Por favor, inicie sesión")) {
                    errorElem.innerText = result;
                    errorElem.style.display = "block";
                } else {
                    alert(result);
                    closeModal();
                }
            }).catch(error => {
                console.error("Error al invocar SaveFreightQuotation:", error);
                showAlert("Error al procesar la cotización. Revise la consola para más detalles.");
            });
        } else {
            console.error("dotNetRef o calculationData no están disponibles", {
                dotNetRef: window.dotNetRef,
                calculationData: window.calculationData
            });
            showAlert("Error al procesar la cotización. Faltan datos necesarios.");
        }
    };

    window.actionOrdenar = function() {
        if (window.dotNetRef && window.calculationData) {
            window.dotNetRef.invokeMethodAsync('SaveFreightOrder',
                window.calculationData.distanceKm,
                window.calculationData.costPerKm,
                window.calculationData.baseCost,
                window.calculationData.insuranceCost,
                window.calculationData.finalCost,
                window.calculationData.truckType,
                window.calculationData.insuranceOption,
                window.calculationData.origin,
                window.calculationData.destination,
                window.calculationData.freightDate,
                window.calculationData.freightLatitude,
                window.calculationData.freightLongitude
            ).then(result => {
                const errorElem = document.getElementById("modalError");
                errorElem.style.display = "none";
                errorElem.innerText = "";

                if (result.includes("Por favor, inicie sesión")) {
                    errorElem.innerText = result;
                    errorElem.style.display = "block";
                } else {
                    alert(result);
                    closeModal();
                }
            }).catch(error => {
                console.error("Error al invocar SaveFreightOrder:", error);
                showAlert("Error al procesar la orden. Revise la consola para más detalles.");
            });
        } else {
            console.error("dotNetRef o calculationData no están disponibles", {
                dotNetRef: window.dotNetRef,
                calculationData: window.calculationData
            });
            showAlert("Error al procesar la orden. Faltan datos necesarios.");
        }
    };

    window.actionSalir = function() {
        closeModal();
    };

        // Exponemos las funciones de acción globalmente
        window.actionCotizar = actionCotizar;
        window.actionOrdenar = actionOrdenar;
        window.actionSalir = actionSalir;

        function showAlert(message) {
            const alertBox = document.getElementById("alertMessage");
            alertBox.innerText = message;
            alertBox.style.display = "block";
            setTimeout(hideAlert, 5000);
        }

        function hideAlert() {
            document.getElementById("alertMessage").style.display = "none";
        }

        function updateTruckCosts() { /* Opcional: recalcula si es necesario */ }

        function openModal() {
            document.getElementById("customModal").style.display = "block";
        }

        function closeModal() {
            document.getElementById("customModal").style.display = "none";
        }

        function clearData() {
            if (originMarker) { originMarker.setMap(null); originMarker = null; }
            if (destinationMarker) { destinationMarker.setMap(null); destinationMarker = null; }
            document.getElementById("origin-input").value = "";
            document.getElementById("destination-input").value = "";
            if (directionsRenderer) { directionsRenderer.set('directions', null); }
            updateSelectionTip("");
            hideAlert();
            console.info("Datos limpiados");
        }
        window.loadGoogleMapsScript = loadGoogleMapsScript;
        window.showMapErrorModal = showMapErrorModal;
        window.initMap = initMap;
        window.activateMapSelection = activateMapSelection;
        window.calculateRoute = calculateRoute;
        window.showAlert = showAlert;
        window.clearData = clearData;
        window.applyTruckSelectionEffect = applyTruckSelectionEffect;
        window.updateTruckCosts = updateTruckCosts;
        window.closeModal = closeModal;
    })();