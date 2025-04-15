document.addEventListener("DOMContentLoaded", function () {
  let mainChart;
  let currentPeriod = "mes";
  let currentCategory = "todos";
  let currentChartType = "bar";

  initChart();

  document.getElementById("btn-semana").addEventListener("click", function () {
    setActivePeriodButton("semana");
    currentPeriod = "semana";
    updateChart();
  });

  document.getElementById("btn-mes").addEventListener("click", function () {
    setActivePeriodButton("mes");
    currentPeriod = "mes";
    updateChart();
  });

  document.getElementById("btn-año").addEventListener("click", function () {
    setActivePeriodButton("año");
    currentPeriod = "año";
    updateChart();
  });

  document
    .getElementById("date-picker")
    .addEventListener("change", function () {
      currentPeriod = "custom";
      setActivePeriodButton("custom");
      updateChart();
    });

  document
    .getElementById("bolsas-selector")
    .addEventListener("click", function () {
      setActiveCategoryButton("bolsas");
      currentCategory = "Bolsas";
      updateChart();
    });

  document
    .getElementById("mangas-selector")
    .addEventListener("click", function () {
      setActiveCategoryButton("mangas");
      currentCategory = "Mangas";
      updateChart();
    });

  document
    .getElementById("todos-selector")
    .addEventListener("click", function () {
      setActiveCategoryButton("todos");
      currentCategory = "todos";
      updateChart();
    });

  document
    .getElementById("btn-bar-chart")
    .addEventListener("click", function () {
      setActiveChartTypeButton("bar");
      currentChartType = "bar";
      updateChartType();
    });

  document
    .getElementById("btn-line-chart")
    .addEventListener("click", function () {
      setActiveChartTypeButton("line");
      currentChartType = "line";
      updateChartType();
    });

  document
    .getElementById("btn-pie-chart")
    .addEventListener("click", function () {
      setActiveChartTypeButton("pie");
      currentChartType = "pie";
      updateChartType();
    });

  function initChart() {
    const ctx = document.getElementById("mainChart").getContext("2d");
    mainChart = new Chart(ctx, {
      type: "bar",
      data: {
        labels: [],
        datasets: [
          {
            label: "Cantidad Vendida",
            data: [],
            backgroundColor: "rgba(9, 102, 35, 0.7)",
            borderColor: "rgba(9, 102, 35, 1)",
            borderWidth: 1,
            barPercentage: 0.6,
            categoryPercentage: 0.8,
          },
        ],
      },
      options: getBarLineOptions(),
    });
    updateChart();
  }

  function getBarLineOptions() {
    return {
      responsive: true,
      maintainAspectRatio: false,
      aspectRatio: 2,
      layout: {
        padding: {
          top: 20,
          right: 20,
          bottom: 20,
          left: 20,
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          title: {
            display: true,
            text: "Cantidad Vendida",
            font: { weight: "bold" },
          },
          ticks: {
            stepSize: 2,
            precision: 0,
            font: { size: 12 },
          },
          grid: {
            drawOnChartArea: true,
            color: "rgba(0, 0, 0, 0.1)",
          },
        },
        x: {
          title: {
            display: true,
            text: "Productos",
            font: { weight: "bold" },
          },
          ticks: {
            font: { size: 10 },
            maxRotation: 45,
            minRotation: 45,
            autoSkip: true,
            maxTicksLimit: 15,
          },
          grid: { display: false },
        },
      },
      plugins: {
        legend: { display: false },
        title: {
          display: true,
          text: "Ventas por Producto",
          font: { size: 16, weight: "bold" },
          padding: { top: 10, bottom: 20 },
        },
        tooltip: {
          callbacks: {
            label: function (context) {
              return `Cantidad: ${context.raw}`;
            },
            title: function (context) {
              return mainChart.data.labels[context[0].dataIndex];
            },
          },
        },
      },
    };
  }

  function getPieOptions() {
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: "right",
          labels: {
            padding: 20,
            usePointStyle: true,
            pointStyle: "circle",
            font: {
              size: 12,
            },
          },
        },
        title: {
          display: true,
          text: mainChart.options.plugins.title.text,
          font: { size: 16, weight: "bold" },
        },
        tooltip: {
          callbacks: {
            label: function (context) {
              const label = context.label || "";
              const value = context.raw || 0;
              const total = context.dataset.data.reduce((a, b) => a + b, 0);
              const percentage = Math.round((value / total) * 100);
              return `${label}: ${value} (${percentage}%)`;
            },
          },
        },
      },
    };
  }

  function updateChart() {
    const datePicker = document.getElementById("date-picker");
    const customDate = currentPeriod === "custom" ? datePicker.value : null;

    // Mostrar loader
    const chartCanvas = document.getElementById("mainChart");
    chartCanvas.style.display = "none";
    const loader = document.createElement("div");
    loader.className = "spinner-border text-success";
    chartCanvas.parentNode.appendChild(loader);

    fetch(
      `/AnalisisVentas/GetChartData?period=${currentPeriod}&category=${currentCategory}&customDate=${customDate}`
    )
      .then((response) => {
        if (!response.ok) throw new Error("Error en la respuesta del servidor");
        return response.json();
      })
      .then((data) => {
        if (data.success) {
          // Limitar el número de productos mostrados si es necesario
          const maxItems = currentChartType === "pie" ? 10 : 15;
          const labels = data.labels.slice(0, maxItems);
          const values = data.values.slice(0, maxItems);

          mainChart.data.labels = labels;
          mainChart.data.datasets[0].data = values;

          // Generar colores dinámicos para el gráfico de pie
          if (currentChartType === "pie") {
            mainChart.data.datasets[0].backgroundColor = generateColors(
              labels.length
            );
            mainChart.data.datasets[0].borderColor = "#ffffff";
            mainChart.data.datasets[0].borderWidth = 2;
          } else {
            mainChart.data.datasets[0].backgroundColor =
              "rgba(9, 102, 35, 0.7)";
            mainChart.data.datasets[0].borderColor = "rgba(9, 102, 35, 1)";
          }

          // Actualizar título
          let categoryLabel =
            currentCategory === "todos"
              ? "Todos los Productos"
              : currentCategory;

          mainChart.options.plugins.title.text = `Ventas de ${categoryLabel} (${getPeriodLabel()})`;

          // Ajustar escala Y automáticamente (solo para gráficos de barra/línea)
          if (currentChartType !== "pie" && values && values.length > 0) {
            const maxValue = Math.max(...values);
            mainChart.options.scales.y.max = Math.ceil(maxValue * 1.1);
            mainChart.options.scales.y.ticks.stepSize = Math.max(
              1,
              Math.floor(maxValue / 5)
            );
          }

          mainChart.update();
        } else {
          showError(data.error || "Error al cargar datos");
        }
      })
      .catch((error) => {
        console.error("Error:", error);
        showError("Error al conectarse con el servidor");
      })
      .finally(() => {
        loader.remove();
        chartCanvas.style.display = "block";
      });
  }

  function generateColors(count) {
    const colors = [];
    const baseColor = [9, 102, 35]; // RGB del color verde oscuro
    const variation = 50; // Variación de color

    for (let i = 0; i < count; i++) {
      const r = Math.max(
        0,
        Math.min(
          255,
          baseColor[0] + Math.floor(Math.random() * variation - variation / 2)
        )
      );
      const g = Math.max(
        0,
        Math.min(
          255,
          baseColor[1] + Math.floor(Math.random() * variation - variation / 2)
        )
      );
      const b = Math.max(
        0,
        Math.min(
          255,
          baseColor[2] + Math.floor(Math.random() * variation - variation / 2)
        )
      );
      colors.push(`rgba(${r}, ${g}, ${b}, 0.7)`);
    }

    return colors;
  }

  function showError(message) {
    const errorDiv = document.createElement("div");
    errorDiv.className = "alert alert-danger";
    errorDiv.textContent = message;

    const chartContainer = document.getElementById("mainChart").parentNode;
    chartContainer.appendChild(errorDiv);

    // Eliminar el mensaje después de 5 segundos
    setTimeout(() => errorDiv.remove(), 5000);
  }

  function updateChartType() {
    mainChart.config.type = currentChartType;

    if (currentChartType === "pie") {
      mainChart.options = getPieOptions();
      mainChart.options.plugins.legend.display = true;
    } else if (currentChartType === "line") {
      mainChart.data.datasets[0].fill = true;
      mainChart.data.datasets[0].backgroundColor = "rgba(9, 102, 35, 0.2)";
      mainChart.options = getBarLineOptions();
      mainChart.options.plugins.legend.display = false;
    } else {
      mainChart.data.datasets[0].fill = false;
      mainChart.data.datasets[0].backgroundColor = "rgba(9, 102, 35, 0.7)";
      mainChart.options = getBarLineOptions();
      mainChart.options.plugins.legend.display = false;
    }

    // Actualizar el gráfico con los nuevos datos y opciones
    updateChart();
  }

  function setActivePeriodButton(period) {
    document.querySelectorAll("[data-period]").forEach((btn) => {
      btn.classList.remove("btn-outline-success");
      btn.classList.add("btn-outline-secondary");
    });

    if (period === "custom") {
      document
        .getElementById("date-picker")
        .classList.remove("btn-outline-secondary");
      document
        .getElementById("date-picker")
        .classList.add("btn-outline-success");
    } else {
      document
        .getElementById(`btn-${period}`)
        .classList.remove("btn-outline-secondary");
      document
        .getElementById(`btn-${period}`)
        .classList.add("btn-outline-success");
    }
  }

  function setActiveCategoryButton(category) {
    document
      .getElementById("bolsas-selector")
      .querySelector("i")
      .classList.remove("text-white");
    document
      .getElementById("bolsas-selector")
      .querySelector("i")
      .classList.add("text-dark-green");
    document
      .getElementById("bolsas-selector")
      .querySelector("span")
      .classList.remove("text-white");
    document
      .getElementById("bolsas-selector")
      .querySelector("span")
      .classList.add("text-dark-green");
    document
      .getElementById("mangas-selector")
      .querySelector("i")
      .classList.remove("text-white");
    document
      .getElementById("mangas-selector")
      .querySelector("i")
      .classList.add("text-dark-green");
    document
      .getElementById("mangas-selector")
      .querySelector("span")
      .classList.remove("text-white");
    document
      .getElementById("mangas-selector")
      .querySelector("span")
      .classList.add("text-dark-green");
    document.getElementById("todos-selector").classList.remove("btn-dark");
    document.getElementById("todos-selector").classList.add("btn-success");

    if (category === "bolsas") {
      document
        .getElementById("bolsas-selector")
        .querySelector("i")
        .classList.remove("text-dark-green");
      document
        .getElementById("bolsas-selector")
        .querySelector("i")
        .classList.add("text-white");
      document
        .getElementById("bolsas-selector")
        .querySelector("span")
        .classList.remove("text-dark-green");
      document
        .getElementById("bolsas-selector")
        .querySelector("span")
        .classList.add("text-white");
    } else if (category === "mangas") {
      document
        .getElementById("mangas-selector")
        .querySelector("i")
        .classList.remove("text-dark-green");
      document
        .getElementById("mangas-selector")
        .querySelector("i")
        .classList.add("text-white");
      document
        .getElementById("mangas-selector")
        .querySelector("span")
        .classList.remove("text-dark-green");
      document
        .getElementById("mangas-selector")
        .querySelector("span")
        .classList.add("text-white");
    } else {
      document.getElementById("todos-selector").classList.remove("btn-success");
      document.getElementById("todos-selector").classList.add("btn-dark");
    }
  }

  function setActiveChartTypeButton(type) {
    document.getElementById("btn-bar-chart").classList.remove("bg-dark-green");
    document.getElementById("btn-bar-chart").classList.add("bg-custom-gray");
    document.getElementById("btn-line-chart").classList.remove("bg-dark-green");
    document.getElementById("btn-line-chart").classList.add("bg-custom-gray");
    document.getElementById("btn-pie-chart").classList.remove("bg-dark-green");
    document.getElementById("btn-pie-chart").classList.add("bg-custom-gray");

    if (type === "bar") {
      document
        .getElementById("btn-bar-chart")
        .classList.remove("bg-custom-gray");
      document.getElementById("btn-bar-chart").classList.add("bg-dark-green");
    } else if (type === "line") {
      document
        .getElementById("btn-line-chart")
        .classList.remove("bg-custom-gray");
      document.getElementById("btn-line-chart").classList.add("bg-dark-green");
    } else if (type === "pie") {
      document
        .getElementById("btn-pie-chart")
        .classList.remove("bg-custom-gray");
      document.getElementById("btn-pie-chart").classList.add("bg-dark-green");
    }
  }

  function getPeriodLabel() {
    switch (currentPeriod) {
      case "semana":
        return "Última Semana";
      case "mes":
        return "Este Mes";
      case "año":
        return "Este Año";
      case "custom":
        const date = document.getElementById("date-picker").value;
        return date ? `Fecha: ${date}` : "Fecha Personalizada";
      default:
        return "Período";
    }
  }

  setActivePeriodButton("mes");
  setActiveCategoryButton("todos");
  setActiveChartTypeButton("bar");
});

document.getElementById("date-picker").valueAsDate = new Date();
document.getElementById("date-picker").addEventListener("change", function () {
  const selectedDate = new Date(this.value);
  const today = new Date();

  // Validar fecha
  if (selectedDate > today) {
    showError("No se pueden seleccionar fechas futuras");
    this.valueAsDate = today;
    return;
  }

  // Formatear fecha para el servidor (YYYY-MM-DD)
  const formattedDate = selectedDate.toISOString().split("T")[0];

  currentPeriod = "custom";
  setActivePeriodButton("custom");
  updateChart();
});

function showError(message) {
  const errorDiv = document.createElement("div");
  errorDiv.className = "alert alert-danger";
  errorDiv.textContent = message;

  const chartContainer = document.getElementById("mainChart").parentNode;
  chartContainer.appendChild(errorDiv);

  // Eliminar el mensaje después de 5 segundos
  setTimeout(() => errorDiv.remove(), 5000);
}
