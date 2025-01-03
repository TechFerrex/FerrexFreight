let map;
let userMarker;

function initLeafletMap() {
    map = L.map('map').setView([15.512164, -88.038020], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    L.marker([15.512164, -88.038020]).addTo(map)
        .bindPopup('Ubicación fija')
        .openPopup();

    map.on('click', function (e) {
        if (userMarker) {
            map.removeLayer(userMarker);
        }
        userMarker = L.marker(e.latlng).addTo(map)
            .bindPopup('Tu ubicación')
            .openPopup();
    });
}

function calculateLeafletDistance() {
    return new Promise((resolve, reject) => {
        if (userMarker) {
            const userLatLng = userMarker.getLatLng();
            const fixedLatLng = L.latLng(15.512164, -88.038020);
            const distance = userLatLng.distanceTo(fixedLatLng);
            const result = {
                distanceKm: distance / 1000, // Retorna la distancia en kilómetros
                latitude: userLatLng.lat,
                longitude: userLatLng.lng
            };
            console.log('calculateLeafletDistance result:', result); // Agrega este log
            resolve(result);
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