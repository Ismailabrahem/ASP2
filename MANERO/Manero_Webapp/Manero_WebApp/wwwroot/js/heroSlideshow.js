document.addEventListener("DOMContentLoaded", function () {
    let currentIndex = 0;
    const images = document.querySelectorAll('.hero-img');
    const maxIndex = images.length;
    const slideshowWrapper = document.querySelector('.hero-slideshow-wrapper');
    const dots = document.querySelectorAll('.dot');

    function updateSlideshow(index) {
        if (index !== undefined) {
            currentIndex = index;
        }

        const newPosition = currentIndex * -100;

        slideshowWrapper.style.transform = `translateX(${newPosition}%)`;

        dots.forEach(dot => dot.classList.remove('active'));
        dots[currentIndex].classList.add('active');
    }

    dots.forEach(dot => {
        dot.addEventListener('click', () => {
            updateSlideshow(parseInt(dot.getAttribute('data-index')));
        });
    });

    updateSlideshow();

    setInterval(() => {
        updateSlideshow((currentIndex + 1) % maxIndex);
    }, 5000);

});