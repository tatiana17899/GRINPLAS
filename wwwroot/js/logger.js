document.addEventListener("DOMContentLoaded", () => {
  setTimeout(() => {
    const loadingScreen = document.getElementById("loading-screen");
    const mainContent = document.querySelector("main");

    loadingScreen.style.display = "none";

    mainContent.classList.add("show");
  }, 3000);
});
