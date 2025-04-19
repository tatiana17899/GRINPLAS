document.addEventListener("DOMContentLoaded", function () {
  let barChart;
  const datePicker = document.getElementById("bar-date-picker"); // Cambiado a ID único
  const productoSelector = document.getElementById("producto-selector");
  const productosDropdown = document.getElementById("productos-dropdown");

  // Inicializar con fecha actual
  datePicker.valueAsDate = new Date();

  // Cargar productos al iniciar
  loadProductos();
  initBarChart();

  // Event listeners
  datePicker.addEventListener("change", updateBarChart);
  productoSelector.addEventListener("click", loadProductos);

  // Cargar lista de productos
  async function loadProductos() {
    try {
      const response = await fetch(
        "/AnalisisVentas/GetProductos?categoria=Bolsas"
      );
      if (!response.ok) throw new Error("Error al cargar productos");

      const productos = await response.json();

      productosDropdown.innerHTML = "";
      productos.forEach((producto) => {
        const item = document.createElement("li");
        const link = document.createElement("a");
        link.className = "dropdown-item";
        link.href = "#";
        link.textContent = producto.nombre;
        link.addEventListener("click", function () {
          productoSelector.textContent = producto.nombre;
          updateBarChart();
        });
        item.appendChild(link);
        productosDropdown.appendChild(item);
      });

      // Seleccionar primer producto por defecto si existe
      if (productos.length > 0) {
        productoSelector.textContent = productos[0].nombre;
        updateBarChart();
      }
    } catch (error) {
      console.error("Error cargando productos:", error);
      showMessage("Error al cargar productos", "error");
    }
  }

  // Inicializar gráfico
  function initBarChart() {
    const ctx = document.getElementById("barChart").getContext("2d");

    barChart = new Chart(ctx, {
      type: "bar",
      data: {
        labels: ["Ventas", "Stock"],
        datasets: [
          {
            label: "Cantidad",
            data: [0, 0],
            backgroundColor: [
              "rgba(9, 102, 35, 0.7)",
              "rgba(54, 162, 235, 0.7)",
            ],
            borderColor: ["rgba(9, 102, 35, 1)", "rgba(54, 162, 235, 1)"],
            borderWidth: 1,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: {
            beginAtZero: true,
            title: {
              display: true,
              text: "Cantidad",
              font: { weight: "bold" },
            },
            ticks: {
              stepSize: 10,
            },
          },
          x: {
            title: {
              display: true,
              text: "Métricas",
              font: { weight: "bold" },
            },
          },
        },
        plugins: {
          title: {
            display: true,
            text: "Ventas y Stock por Producto",
            font: { size: 16, weight: "bold" },
          },
          legend: {
            display: false,
          },
          tooltip: {
            callbacks: {
              label: function (context) {
                return `${context.dataset.label}: ${context.raw}`;
              },
            },
          },
        },
      },
    });
  }

  // Actualizar gráfico con datos
  async function updateBarChart() {
    const fechaSeleccionada = datePicker.value;
    const productoSeleccionado = productoSelector.textContent;

    if (productoSeleccionado === "Producto" || !fechaSeleccionada) return;

    try {
      showMessage("Cargando datos...", "info");
      const fechaMostrar = new Date(fechaSeleccionada).toLocaleDateString(
        "es-PE"
      );

      // Obtener datos de ventas
      const ventasResponse = await fetch(
        `/AnalisisVentas/GetVentasProducto?producto=${encodeURIComponent(
          productoSeleccionado
        )}&fecha=${fechaSeleccionada}`
      );

      if (!ventasResponse.ok) throw new Error("Error al obtener ventas");

      const ventasData = await ventasResponse.json();
      console.log("Datos de ventas:", ventasData);

      // Obtener datos de stock
      const stockResponse = await fetch(
        `/AnalisisVentas/GetStockProducto?producto=${encodeURIComponent(
          productoSeleccionado
        )}`
      );

      if (!stockResponse.ok) throw new Error("Error al obtener stock");

      const stockData = await stockResponse.json();
      console.log("Datos de stock:", stockData);

      // Actualizar gráfico
      barChart.data.datasets[0].data = [
        ventasData.cantidad || 0,
        stockData.cantidad || 0,
      ];

      barChart.options.plugins.title.text = `${productoSeleccionado} (${fechaSeleccionada})`;
      barChart.update();

      showMessage("", "success");
    } catch (error) {
      console.error("Error actualizando gráfico:", error);
      showMessage(error.message, "error");

      // Mostrar datos en cero si hay error
      barChart.data.datasets[0].data = [0, 0];
      barChart.update();
    }
  }

  function showMessage(text, type) {
    const messageContainer =
      document.getElementById("bar-chart-message") || createMessageContainer();
    messageContainer.textContent = text;
    messageContainer.className = `text-center mt-2 ${
      type === "error"
        ? "text-danger"
        : type === "warning"
        ? "text-warning"
        : type === "info"
        ? "text-info"
        : ""
    }`;
  }

  function createMessageContainer() {
    const container = document.createElement("div");
    container.id = "bar-chart-message";
    container.className = "text-center mt-2";
    document.getElementById("barChart").parentNode.appendChild(container);
    return container;
  }
});
