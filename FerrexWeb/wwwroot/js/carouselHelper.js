function initializeCarousels() {
    const carousels = document.querySelectorAll('.card-carousel');
    carousels.forEach(carousel => {
        let index = 0;
        const images = carousel.querySelectorAll('.carousel-image');
        // Solo inicia el ciclo si hay más de una imagen
        if (images.length > 1) {
            setInterval(() => {
                images[index].classList.remove('active');
                index = (index + 1) % images.length;
                images[index].classList.add('active');
            }, 3000); // Cambia la imagen cada 3 segundos
        }
    });
}