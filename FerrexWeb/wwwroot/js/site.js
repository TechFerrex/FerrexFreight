let cartMap = null;
let cartMarker = null;

// Helpers genéricos para interop con Blazor
window.getElementValue = function (id) {
    const el = document.getElementById(id);
    return el ? el.value : "";
};

window.focusElement = function (id) {
    const el = document.getElementById(id);
    if (!el) return;
    el.focus({ preventScroll: false });
    el.scrollIntoView({ behavior: "smooth", block: "center" });
};

// Espera a que Google Maps esté cargado
function waitForGoogleMaps() {
    return new Promise(function (resolve) {
        if (typeof google !== 'undefined' && google.maps) {
            resolve();
            return;
        }
        var interval = setInterval(function () {
            if (typeof google !== 'undefined' && google.maps) {
                clearInterval(interval);
                resolve();
            }
        }, 100);
    });
}

async function initLeafletMap() {
    await waitForGoogleMaps();

    var mapDiv = document.getElementById('map');
    if (!mapDiv) return;

    // Limpiar mapa anterior si existe
    if (cartMap) {
        cartMap = null;
        cartMarker = null;
    }
    mapDiv.innerHTML = '';

    var fixedPoint = { lat: 15.512164, lng: -88.038020 };

    cartMap = new google.maps.Map(mapDiv, {
        center: fixedPoint,
        zoom: 13,
        mapTypeControl: false,
        streetViewControl: false,
        fullscreenControl: false
    });

    // Marcador fijo (bodega/tienda)
    new google.maps.Marker({
        position: fixedPoint,
        map: cartMap,
        title: 'Ubicación fija'
    });

    // Click para colocar marcador del usuario
    cartMap.addListener('click', function (e) {
        if (cartMarker) {
            cartMarker.setMap(null);
        }
        cartMarker = new google.maps.Marker({
            position: e.latLng,
            map: cartMap,
            title: 'Tu ubicación'
        });
    });
}

function calculateLeafletDistance() {
    return new Promise(function (resolve, reject) {
        if (cartMarker) {
            var userPos = cartMarker.getPosition();
            var fixedLat = 15.512164;
            var fixedLng = -88.038020;

            // Haversine para calcular distancia en km
            var R = 6371;
            var dLat = (userPos.lat() - fixedLat) * Math.PI / 180;
            var dLng = (userPos.lng() - fixedLng) * Math.PI / 180;
            var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
                Math.cos(fixedLat * Math.PI / 180) * Math.cos(userPos.lat() * Math.PI / 180) *
                Math.sin(dLng / 2) * Math.sin(dLng / 2);
            var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
            var distance = R * c;

            resolve({
                distanceKm: distance,
                latitude: userPos.lat(),
                longitude: userPos.lng()
            });
        } else {
            alert('Por favor, selecciona una ubicación en el mapa.');
            reject('No se ha seleccionado ubicación');
        }
    });
}




function scrollToSection(sectionId) {
    var element = document.getElementById(sectionId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
    }
}

window.iniciarAnimacion = function () {
document.querySelectorAll('.truck-button').forEach(button => {
    button.addEventListener('click', e => {

        e.preventDefault();

        let box = button.querySelector('.box'),
            truck = button.querySelector('.truck');

        if (!button.classList.contains('done')) {

            if (!button.classList.contains('animation')) {

                button.classList.add('animation');

                gsap.to(button, {
                    '--box-s': 1,
                    '--box-o': 1,
                    duration: .3,
                    delay: .5
                });

                gsap.to(box, {
                    x: 0,
                    duration: .4,
                    delay: .7
                });

                gsap.to(button, {
                    '--hx': -5,
                    '--bx': 50,
                    duration: .18,
                    delay: .92
                });

                gsap.to(box, {
                    y: 0,
                    duration: .1,
                    delay: 1.15
                });

                gsap.set(button, {
                    '--truck-y': 0,
                    '--truck-y-n': -26
                });

                gsap.to(button, {
                    '--truck-y': 1,
                    '--truck-y-n': -25,
                    duration: .2,
                    delay: 1.25,
                    onComplete() {
                        gsap.timeline({
                            onComplete() {
                                button.classList.add('done');
                            }
                        }).to(truck, {
                            x: 0,
                            duration: .4
                        }).to(truck, {
                            x: 40,
                            duration: 1
                        }).to(truck, {
                            x: 20,
                            duration: .6
                        }).to(truck, {
                            x: 96,
                            duration: .4
                        });
                        gsap.to(button, {
                            '--progress': 1,
                            duration: 2.4,
                            ease: "power2.in"
                        });
                    }
                });

            }

        } else {
            button.classList.remove('animation', 'done');
            gsap.set(truck, {
                x: 4
            });
            gsap.set(button, {
                '--progress': 0,
                '--hx': 0,
                '--bx': 0,
                '--box-s': .5,
                '--box-o': 0,
                '--truck-y': 0,
                '--truck-y-n': -26
            });
            gsap.set(box, {
                x: -24,
                y: -6
            });
        }

    });
});
};