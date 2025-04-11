const menutoggle = document.querySelector("#menu-toggle");
const sidebar = document.querySelector("#sidebar");
const maincontent = document.querySelector("#main-content");

menutoggle.addEventListener("click", () => {
  sidebar.classList.toggle("collapsed");
  maincontent.classList.toggle("expanded");
});
