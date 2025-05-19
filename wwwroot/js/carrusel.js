document.addEventListener("DOMContentLoaded", function () {
  const hero = document.querySelector(".hero-content");
  const sliderIcons = document.querySelectorAll(".slider i");
  let currentSlide = 0;

  // Array de imágenes de fondo
  const images = [
    "https://images.unsplash.com/photo-1562280963-8a5475740a42?q=80&w=2069&auto=format&fit=crop",
    "https://images.unsplash.com/photo-1604382354936-07c5d9983bd3?q=80&w=2070&auto=format&fit=crop",
    "https://images.unsplash.com/photo-1565814636199-ae8133055c1c?q=80&w=2070&auto=format&fit=crop",
  ];

  // Función para actualizar la imagen de fondo
  function updateSlide(index) {
    hero.style.backgroundImage = `linear-gradient(rgba(112, 166, 127, 0.8), rgba(112, 166, 127, 0.8)), url(${images[index]})`;

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
