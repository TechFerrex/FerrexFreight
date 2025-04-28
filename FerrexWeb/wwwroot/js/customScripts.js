// wwwroot/js/customScripts.js

// --- scrollToSection ---
window.scrollToSection = function (id) {
    var element = document.getElementById(id);
    if (element) element.scrollIntoView({ behavior: 'smooth' });
};

// --- imageExists ---
window.imageExists = function (url) {
    return new Promise(function (resolve) {
        var img = new Image();
        img.onload = function () { resolve(true); };
        img.onerror = function () { resolve(false); };
        img.src = url;
    });
};

// --- carouselHelper (antes en DOMContentLoaded) ---
document.addEventListener("DOMContentLoaded", function () {
    var carousels = document.querySelectorAll('.image-carousel');
    carousels.forEach(function (carousel) {
        var slides = carousel.querySelectorAll('img');
        var idx = 0;
        slides[idx].classList.add('active');
        setInterval(function () {
            slides[idx].classList.remove('active');
            idx = (idx + 1) % slides.length;
            slides[idx].classList.add('active');
        }, 3000);
    });
});

// --- stubs para mapsInterop (por si el mapa se inicializa antes) ---
window.loadGoogleMapsScript = window.loadGoogleMapsScript || function () { console.warn('stub loadGoogleMapsScript'); };
window.showMapErrorModal = window.showMapErrorModal || function (show, msg) { console.warn('stub showMapErrorModal', show, msg); };
window.initMap = window.initMap || function () { console.warn('stub initMap'); };
