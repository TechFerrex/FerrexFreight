
(function () {
    // –– Variables de estado globales ––
    let map, directionsService, directionsRenderer;
    let originMarker = null, destinationMarker = null;
    let stopMarkers = [];
    let isMapSelectionMode = false, currentSelection = "";
    const truckRates = { small: 37, medium: 50, pickup: 33 };
    window.dotNetRef = null;
    window.calculationData = {};

    // Detectar si estamos en la página /freight
    function isFreightPage() {
        return window.location.pathname.includes('/freight');
    }

    // –– Inicialización del mapa y autocomplete ––
    function initMap() {
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

        // Autocomplete Origen
        const originInput = document.getElementById("origin-input");
        if (originInput) {
            const acOrigin = new google.maps.places.Autocomplete(originInput, {
                componentRestrictions: { country: "hn" },
                fields: ["geometry", "name"]
            });
            acOrigin.addListener("place_changed", () => {
                const place = acOrigin.getPlace();
                if (!place.geometry) return;
                if (originMarker) originMarker.setMap(null);
                originMarker = new google.maps.Marker({
                    position: place.geometry.location,
                    map, title: "Origen"
                });
                map.setCenter(place.geometry.location);
                map.setZoom(15);
            });
            originInput.addEventListener("focus", () => {
                isMapSelectionMode = false;
                updateSelectionTip("");
            });
        }

        // Autocomplete Destino
        const destInput = document.getElementById("destination-input");
        if (destInput) {
            const acDest = new google.maps.places.Autocomplete(destInput, {
                componentRestrictions: { country: "hn" },
                fields: ["geometry", "name"]
            });
            acDest.addListener("place_changed", () => {
                const place = acDest.getPlace();
                if (!place.geometry) return;
                if (destinationMarker) destinationMarker.setMap(null);
                destinationMarker = new google.maps.Marker({
                    position: place.geometry.location,
                    map, title: "Destino"
                });
                map.setCenter(place.geometry.location);
                map.setZoom(15);
            });
            destInput.addEventListener("focus", () => {
                isMapSelectionMode = false;
                updateSelectionTip("");
            });
        }

        // Clic en el mapa (origen → paradas → destino)
        map.addListener("click", (e) => {
            if (!isMapSelectionMode) return;

            const pos = e.latLng;

            if (currentSelection === "origin") {
                // limpias y creas originMarker...
                originMarker && originMarker.setMap(null);
                originMarker = new google.maps.Marker({ position: pos, map, title: "Origen" });
                document.getElementById("origin-input").value = `${pos.lat().toFixed(5)}, ${pos.lng().toFixed(5)}`;
            }
            else if (currentSelection === "destination") {
                destinationMarker && destinationMarker.setMap(null);
                destinationMarker = new google.maps.Marker({ position: pos, map, title: "Destino" });
                document.getElementById("destination-input").value = `${pos.lat().toFixed(5)}, ${pos.lng().toFixed(5)}`;
            }
            else if (currentSelection === "stop") {
                // Remover marcador anterior si existía
                if (stopMarkers[selectedStopIndex]) {
                    stopMarkers[selectedStopIndex].setMap(null);
                }
                // Crear nuevo marcador
                const mk = new google.maps.Marker({
                    position: pos,
                    map,
                    title: `Parada ${selectedStopIndex + 1}`
                });
                stopMarkers[selectedStopIndex] = mk;

                // Deshabilitar el input y mostrar mensaje
                const input = document.getElementById(`stop-input-${selectedStopIndex}`);
                // Limpia su valor real (no lo usaremos para geocodificar)
                input.value = '';
                // Ponemos placeholder para que el usuario vea el mensaje
                input.setAttribute('placeholder', 'Seleccionado en el mapa');
                // Deshabilitamos la casilla para que no se pueda editar
                input.disabled = true;
                // (Opcional) Añadir una clase CSS que cambie estilo visual
                input.classList.add('map-selected');

                // Desactivamos modo mapa
                isMapSelectionMode = false;
                currentSelection = "";
                selectedStopIndex = null;
                updateSelectionTip("");
            }


        });
    }

    // –– Carga dinámica del script de Google Maps ––
    function loadGoogleMapsScript(dotNetObjectRef, apiKey) {
        window.dotNetRef = dotNetObjectRef;

        // Remover script anterior si existe
        const prevScript = document.getElementById('googleMapsScript');
        if (prevScript) prevScript.remove();

        // Ocultar modal de error
        showMapErrorModal(false);

        // Validar que se proporcione la API key
        if (!apiKey) {
            showMapErrorModal(true, 'Error: API key de Google Maps no configurada.');
            return;
        }

        // Crear nuevo script
        const script = document.createElement('script');
        script.id = 'googleMapsScript';
        script.src = `https://maps.googleapis.com/maps/api/js?key=${apiKey}&libraries=places&callback=initMap`;
        script.async = true;
        script.defer = true;

        script.onload = function () {
            console.log("Google Maps cargado correctamente");
        };

        script.onerror = function () {
            console.error("Error al cargar Google Maps");
            showMapErrorModal(true, 'Error al cargar Google Maps. Verifique su conexión a internet.');
        };

        document.head.appendChild(script);
    }

    function initializeAutocomplete(inputId) {
        const input = document.getElementById(inputId);
        if (!input || !google || !google.maps || !google.maps.places) {
            console.warn('No se pudo inicializar autocompletado para ${ inputId }');
            return;
        }

        const autocomplete = new google.maps.places.Autocomplete(input, {
            componentRestrictions: { country: "hn" },
            fields: ["geometry", "name", "formatted_address"]
        });

        autocomplete.addListener("place_changed", function () {
            const place = autocomplete.getPlace();
            if (!place.geometry) {
                console.warn("No se encontró geometría para el lugar seleccionado");
                return;
            }

            // Actualizar el valor del input con la dirección formateada
            input.value = place.formatted_address || place.name || input.value;
        });
    }

    function showCalculationModal(distanceKm, totalStops, baseCost, insuranceCost, extraStopCost, totalCost, freightDate) {
        const html = `
        <div class="modal-header">
            <h5 class="modal-title">Resultado del Cálculo</h5>
        </div>
        <div class="modal-body">
            <p><strong>Fecha:</strong> ${freightDate}</p>
            <p><strong>Distancia:</strong> ${distanceKm.toFixed(2)} km</p>
            <p><strong>Paradas:</strong> ${totalStops} (L${extraStopCost.toFixed(2)})</p>
            <p><strong>Costo base:</strong> L${baseCost.toFixed(2)}</p>
            <p><strong>Seguro:</strong> L${insuranceCost.toFixed(2)}</p>
            <hr>
            <h4 class="text-center text-success">Total: L${totalCost.toFixed(2)}</h4>
        </div>
        <div class="modal-footer">
            <button id="btnCotizar" class="btn btn-primary">Cotizar</button>
            <button id="btnOrdenar" class="btn btn-success">Hacer Flete</button>
            <button id="btnSalir" class="btn btn-secondary">Cerrar</button>
        </div>;`
        document.getElementById("modalBody").innerHTML = html;
        openModal();

        // Agregar event listeners
        document.getElementById("btnCotizar").addEventListener("click", actionCotizar);
        document.getElementById("btnOrdenar").addEventListener("click", actionOrdenar);
        document.getElementById("btnSalir").addEventListener("click", actionSalir);
    }
    // –– Modales y alertas ––
    function showMapErrorModal(show, message) {
        let m = document.getElementById('mapErrorModal');
        if (!m) {
            m = document.createElement('div');
            m.id = 'mapErrorModal';
            m.className = 'modal';
            m.innerHTML = `
            <div class="modal-content">
                <span class="close" onclick="this.parentNode.parentNode.style.display='none'">&times;</span>
                <div class="alert alert-danger" id="mapErrorMessage"></div>
            </div>`;
            document.body.appendChild(m);
        }
        m.style.display = show ? 'block' : 'none';
        if (message) document.getElementById('mapErrorMessage').innerText = message;
    }

    function updateSelectionTip(msg) {
        const tip = document.getElementById("selectionTip");
        tip.style.display = msg ? "block" : "none";
        tip.innerText = msg;
    }

    function openModal() {
        document.getElementById("customModal").style.display = "block";
    }
    function closeModal() {
        document.getElementById("customModal").style.display = "none";
    }

    function showAlert(msg) {
        const a = document.getElementById("alertMessage");
        a.innerText = msg;
        a.style.display = "block";
        setTimeout(hideAlert, 5000);
    }
    function hideAlert() {
        document.getElementById("alertMessage").style.display = "none";
    }

    // –– Botones de Blazor invocan estas funciones ––
    function activateMapSelection() {
        isMapSelectionMode = true;
        currentSelection = "origin";
        // limpiar marcadores previos
        if (originMarker) originMarker.setMap(null);
        if (destinationMarker) destinationMarker.setMap(null);
        stopMarkers.forEach(m => m.setMap(null));
        stopMarkers = [];
        document.getElementById("origin-input").value = "";
        document.getElementById("destination-input").value = "";
        document.getElementById("stops-list").innerHTML = "";
        updateSelectionTip("Selecciona el origen en el mapa");
        hideAlert();
    }
    function activateStopSelection() {
        isMapSelectionMode = true;
        currentSelection = "stop";
        updateSelectionTip("Selecciona la parada en el mapa");
    }

    // –– Agrega la parada a la lista visual ––
    function addStopToList(marker) {
        const li = document.createElement("li");
        li.className = "list-group-item";
        li.textContent = marker.getTitle() +
            " (" + marker.getPosition().lat().toFixed(5) +
            "," + marker.getPosition().lng().toFixed(5) + ")";
        document.getElementById("stops-list").appendChild(li);
    }

    // –– Validación de departamento ––
    function validateAllowedLocation(latLng, callback) {
        const geocoder = new google.maps.Geocoder();
        geocoder.geocode({ location: latLng }, (results, status) => {
            if (status === 'OK' && results[0]) {
                let isHN = false, deptOk = true;
                results[0].address_components.forEach(c => {
                    if (c.types.includes('country') && c.long_name.toLowerCase() === 'honduras') {
                        isHN = true;
                    }
                    if (c.types.includes('administrative_area_level_1')) {
                        const d = c.long_name.toLowerCase();
                        if (d.includes('gracias a dios') || d.includes('islas')) {
                            deptOk = false;
                        }
                    }
                });
                callback(isHN && deptOk);
            } else callback(false);
        });
    }

    function geocodeAddress(address) {
        return new Promise((resolve, reject) => {
            const geocoder = new google.maps.Geocoder();
            geocoder.geocode({ address: address }, (results, status) => {
                if (status === "OK" && results[0]) {
                    // devolvemos la ubicación como LatLng
                    resolve(results[0].geometry.location);
                } else {
                    reject("Geocode失败: " + status);
                }
            });
        });
    }


    // –– Cálculo de ruta con paradas y modal ––
    async function calculateRoute(stopsJson) {
        hideAlert();

        // 1) Validaciones iniciales
        const dateInput = document.getElementById("freight-date");
        if (!dateInput.value) {
            showAlert("Por favor, selecciona la fecha de programación.");
            return;
        }
        if (!originMarker || !destinationMarker) {
            showAlert("Selecciona origen y destino primero.");
            return;
        }

        // 2) Recopilar valores de los inputs de parada manual
        const stopInputs = Array.from(document.querySelectorAll(".stop-input"))
            // solo los que NO están deshabilitados
            .filter(inp => !inp.disabled)
            .map(inp => inp.value.trim())
            .filter(v => v.length > 0);

        // 3) Convertir cada dirección/texto en LatLng (o parsear "lat,lng")
        let manualPoints;
        try {
            manualPoints = await Promise.all(stopInputs.map(async addr => {
                // Si ya viene en formato "lat,lng":
                if (/^\s*-?\d+(\.\d+)?\s*,\s*-?\d+(\.\d+)?\s*$/.test(addr)) {
                    const [lat, lng] = addr.split(",").map(x => parseFloat(x.trim()));
                    return new google.maps.LatLng(lat, lng);
                }
                // Si es texto libre, llamamos al geocoder:
                return await geocodeAddress(addr);
            }));
        } catch (err) {
            showAlert("Error geocodificando alguna parada: " + err);
            return;
        }

        // 4) Validar que origen y destino estén en zona permitida
        validateAllowedLocation(originMarker.getPosition(), ok1 => {
            if (!ok1) {
                showAlert("Origen fuera de zona autorizada.");
                return;
            }
            validateAllowedLocation(destinationMarker.getPosition(), ok2 => {
                if (!ok2) {
                    showAlert("Destino fuera de zona autorizada.");
                    return;
                }

                // 5) Construir array de waypoints: manuales + marcadores del mapa
                const waypoints = manualPoints.map(loc => ({
                    location: loc,
                    stopover: true
                })).concat(
                    stopMarkers.filter(mk => mk).map(mk => ({
                        location: mk.getPosition(),
                        stopover: true
                    }))
                );

                // 6) Solicitar la ruta a Google
                directionsService.route({
                    origin: originMarker.getPosition(),
                    destination: destinationMarker.getPosition(),
                    waypoints: waypoints,
                    travelMode: google.maps.TravelMode.DRIVING
                }, (response, status) => {
                    if (status !== 'OK') {
                        showAlert('Error al calcular la ruta: ' + status);
                        return;
                    }

                    // 7) Renderizar ruta y calcular distancia total
                    directionsRenderer.setDirections(response);
                    let totalMeters = 0;
                    response.routes[0].legs.forEach(leg => totalMeters += leg.distance.value);
                    const distanceKm = totalMeters / 1000;

                    // 8) Cálculo de costos
                    const truckType = document.getElementById("truck-type").value || "pickup";
                    const costPerKm = truckRates[truckType] || 33;
                    let baseCost = 0;
                    if (distanceKm <= 6) {
                        baseCost = (truckType === "small") ? 600 : 350;
                    } else {
                        const extraKm = distanceKm - 6;
                        baseCost = (truckType === "small" ? 600 : 350) + extraKm * costPerKm;
                    }
                    const insOpt = document.getElementById("insurance-option")?.value || "none";
                    let insuranceCost = insOpt === "basic" ? baseCost * 0.05
                        : insOpt === "premium" ? baseCost * 0.10
                            : 0;
                    const totalStops = manualPoints.length + stopMarkers.filter(m => m).length;
                    const extraStopCost = totalStops * 150;
                    const totalCost = baseCost + insuranceCost + extraStopCost;

                    // 9) Preparar datos para enviar a Blazor
                    const allStops = [];
                    // Paradas manuales (texto)
                    stopInputs.forEach(addr => allStops.push({ Address: addr, Lat: 0, Lng: 0 }));
                    // Paradas del mapa
                    stopMarkers.filter(mk => mk).forEach(mk => allStops.push({
                        Address: mk.getTitle() || "Parada",
                        Lat: mk.getPosition().lat(),
                        Lng: mk.getPosition().lng()
                    }));

                    window.calculationData = {
                        distanceKm,
                        costPerKm,
                        baseCost,
                        insuranceCost,
                        totalCost,
                        truckType,
                        insuranceOption: insOpt,
                        origin: document.getElementById("origin-input").value,
                        destination: document.getElementById("destination-input").value,
                        freightDate: dateInput.value,
                        freightLatitude: originMarker.getPosition().lat(),
                        freightLongitude: originMarker.getPosition().lng(),
                        stopsJson: JSON.stringify(allStops)
                    };

                    // 10) Mostrar modal con resultados
                    showCalculationModal(
                        distanceKm,
                        totalStops,
                        baseCost,
                        insuranceCost,
                        extraStopCost,
                        totalCost,
                        dateInput.value
                    );
                });
            });
        });
    }


    function actionCotizar() {
        if (!window.dotNetRef || !window.calculationData) {
            showAlert("Error: Datos de cálculo no disponibles");
            return;
        }

        window.dotNetRef.invokeMethodAsync('SaveFreightQuotation',
            window.calculationData.distanceKm,
            window.calculationData.costPerKm,
            window.calculationData.baseCost,
            window.calculationData.insuranceCost,
            window.calculationData.totalCost,
            window.calculationData.truckType,
            window.calculationData.insuranceOption,
            window.calculationData.origin,
            window.calculationData.destination,
            window.calculationData.freightDate,
            window.calculationData.freightLatitude,
            window.calculationData.freightLongitude,
            window.calculationData.stopsJson
        ).then(res => {
            const err = document.getElementById("modalError");
            if (err) {
                err.style.display = "none";
                err.innerText = "";
            }

            if (res && res.includes("Por favor, inicie sesión")) {
                if (err) {
                    err.innerText = res;
                    err.style.display = "block";
                }
            } else {
                alert(res || "Cotización guardada correctamente");
                closeModal();
            }
        }).catch(error => {
            console.error("Error al guardar cotización:", error);
            showAlert("Error al guardar la cotización");
        });
    }

    function actionOrdenar() {
        if (!window.dotNetRef || !window.calculationData) {
            showAlert("Error: Datos de cálculo no disponibles");
            return;
        }

        window.dotNetRef.invokeMethodAsync('SaveFreightOrder',
            window.calculationData.distanceKm,
            window.calculationData.costPerKm,
            window.calculationData.baseCost,
            window.calculationData.insuranceCost,
            window.calculationData.totalCost,
            window.calculationData.truckType,
            window.calculationData.insuranceOption,
            window.calculationData.origin,
            window.calculationData.destination,
            window.calculationData.freightDate,
            window.calculationData.freightLatitude,
            window.calculationData.freightLongitude,
            window.calculationData.stopsJson
        )
            .then(res => {
                const err = document.getElementById("modalError");
                if (err) { err.style.display = "none"; err.innerText = ""; }
                if (res.includes("Por favor") || res.includes("inválida") || res.includes("futura")) {
                    err.innerText = res; err.style.display = "block";
                } else {
                    alert(res || "Flete ordenado exitosamente");
                    closeModal();
                }
            })
            .catch(error => {
                console.error("Error al ordenar el flete:", error);
                showAlert("Error al ordenar el flete");
            });
    }


    function actionSalir() {
        closeModal();
    }

    // –– Limpia todo ––
    function clearData() {
        if (originMarker) originMarker.setMap(null);
        if (destinationMarker) destinationMarker.setMap(null);
        stopMarkers.forEach(m => m.setMap(null));
        stopMarkers = [];
        document.getElementById("origin-input").value = "";
        document.getElementById("destination-input").value = "";
        document.getElementById("stops-list").innerHTML = "";
        if (directionsRenderer) directionsRenderer.set('directions', null);
        updateSelectionTip("");
        hideAlert();
    }
    function updateTruckCosts(truckType) {
        console.log('Tipo de camión actualizado a: ${ truckType }');
    }
    // –– Efectos UI para selección de camión ––
    function applyTruckSelectionEffect(type) {
        // Remover clase active de todas las tarjetas
        document.querySelectorAll('.truck-card').forEach(card => {
            card.classList.remove('active');
        });

        // Agregar clase active a la tarjeta seleccionada
        const selectedCard = document.querySelector(`[data-truck-type="${type}"]`);
        if (selectedCard) {
            selectedCard.classList.add('active');
            createRippleEffect(selectedCard);
        }

        // Actualizar el campo hidden
        const hiddenInput = document.getElementById("truck-type");
        if (hiddenInput) {
            hiddenInput.value = type;
        }
    }

    // Variable global para el observer para poder desconectarlo si es necesario
    let truckSelectionObserver = null;

    function initTruckSelectionEffects() {
        // Desconectar observer anterior si existe
        if (truckSelectionObserver) {
            truckSelectionObserver.disconnect();
        }

        truckSelectionObserver = new MutationObserver(muts => {
            muts.forEach(m => {
                if (m.attributeName === 'class') {
                    const t = m.target;
                    if (t.classList.contains('active')) {
                        t.style.animation = 'none';
                        t.offsetHeight;  // reflow
                        t.style.animation = null;
                        createRippleEffect(t);
                    }
                }
            });
        });

        document.querySelectorAll('.truck-card').forEach(c => {
            // Evitar múltiples listeners usando una bandera
            if (!c._hasRippleListener) {
                truckSelectionObserver.observe(c, { attributes: true });
                c.addEventListener('click', () => createRippleEffect(c));
                c._hasRippleListener = true;
            }
        });
    }
    function createRippleEffect(el) {
        const r = document.createElement('span');
        r.className = 'ripple';
        Object.assign(r.style, {
            position: 'absolute', borderRadius: '50%',
            background: 'rgba(255,255,255,0.7)',
            transform: 'scale(0)', animation: 'ripple 0.6s linear',
            pointerEvents: 'none', width: '100%', height: '100%',
            left: 0, top: 0
        });
        el.appendChild(r);
        setTimeout(() => r.remove(), 600);
    }
    function addRippleStyle() {
        if (!document.getElementById('rippleStyleSheet')) {
            const s = document.createElement('style');
            s.id = 'rippleStyleSheet';
            s.textContent =
                `@keyframes ripple {
                to { transform: scale(4); opacity: 0; }
            }`;
            document.head.appendChild(s);
        }
    }

    // –– Inicializaciones al cargar el DOM ––
    window.addEventListener('DOMContentLoaded', () => {
        addRippleStyle();
        setTimeout(initTruckSelectionEffects, 500);
    });

    // –– Exponer APIs a Blazor ––Z
    window.selectLocationOnMap = function (mode) {
        isMapSelectionMode = true;
        currentSelection = mode.startsWith("stop") ? "stop" : mode;
        // para "stop-0", "stop-1", etc. guardamos índice:
        selectedStopIndex = mode.startsWith("stop")
            ? parseInt(mode.split("-")[1], 10)
            : null;
        updateSelectionTip(
            mode === "origin" ? "Selecciona origen en mapa" :
                mode === "destination" ? "Selecciona destino en mapa" :
                    `Selecciona ubicación para parada #${selectedStopIndex + 1}`
        );
    };
    window.loadGoogleMapsScript = loadGoogleMapsScript;
    window.initMap = initMap;
    window.showMapErrorModal = showMapErrorModal;
    window.activateMapSelection = activateMapSelection;
    window.activateStopSelection = activateStopSelection;
    window.calculateRoute = calculateRoute;
    window.applyTruckSelectionEffect = applyTruckSelectionEffect;
    window.showAlert = showAlert;
    window.hideAlert = hideAlert;
    window.clearData = clearData;
    window.openModal = openModal;
    window.closeModal = closeModal;
    window.actionCotizar = actionCotizar;
    window.actionOrdenar = actionOrdenar;
    window.actionSalir = actionSalir;
    window.updateTruckCosts = updateTruckCosts;
    window.initializeAutocomplete = initializeAutocomplete;
    window.showCalculationModal = showCalculationModal;
    window.activateStopSelection = activateStopSelection;
    window.activateMapSelection = activateMapSelection;
})();