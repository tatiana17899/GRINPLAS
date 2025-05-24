document.addEventListener("DOMContentLoaded", function () {
  const hero = document.querySelector(".header");
  const sliderIcons = document.querySelectorAll(".slider i");
  let currentSlide = 0;

  // Array de imágenes de fondo
  const images = [
    "https://i.ibb.co/mry5W1tX/b31edadeb493ee7e5858827193910ed0.jpg",
    "https://i.ibb.co/sdyYs29Y/bolsas-de-plastico-de-colores-dispuestos-sobre-fondo-gris.jpg",
    "https://i.ibb.co/dwjTXWGF/rollos-de-bolsas-de-basura-en-espacio-de-fondo-blanco-para-el-texto.jpg",
  ];

  // Función para actualizar la imagen de fondo
  function updateSlide(index) {
    hero.style.backgroundImage = `linear-gradient(rgba(0, 186, 167, 0.4), rgba(0, 186, 167, 0.4)),
    url(${images[index]})`;

    // Actualizar estado activo de los íconos
    sliderIcons.forEach((icon, i) => {
      icon.classList.toggle("active", i === index);
    });
  }

  // Eventos click para los íconos
  sliderIcons.forEach((icon, index) => {
    icon.addEventListener("click", () => {
      currentSlide = index;
      updateSlide(currentSlide);
      resetInterval();
    });
  });

  // Auto-deslizamiento
  let slideInterval;

  function startInterval() {
    slideInterval = setInterval(() => {
      currentSlide = (currentSlide + 1) % images.length;
      updateSlide(currentSlide);
    }, 5000);
  }

  function resetInterval() {
    clearInterval(slideInterval);
    startInterval();
  }

  // Inicializar
  updateSlide(currentSlide);
  startInterval();
});
