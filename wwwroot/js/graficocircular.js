document.addEventListener("DOMContentLoaded", function () {
  let pieChart;
  const currentCategory = "Bolsas";
  const messageContainer = document.getElementById("pie-chart-message");
  const datePicker = document.getElementById("pie-date-picker");

  // Inicializar con fecha actual
  datePicker.valueAsDate = new Date();

  // Función para formatear fecha igual que en el gráfico principal
  function formatDate(date) {
    const d = new Date(date);
    return d.toISOString().split("T")[0];
  }

  // Inicializar gráfico
  function initPieChart() {
    const ctx = document.getElementById("pieChart").getContext("2d");

    if (pieChart) {
      pieChart.destroy();
    }

    pieChart = new Chart(ctx, {
      type: "pie",
      data: {
        labels: ["Inicializando..."],
        datasets: [
          {
            data: [1],
            backgroundColor: ["rgba(200, 200, 200, 0.7)"],
            borderColor: "#ffffff",
            borderWidth: 2,
          },
        ],
      },
      options: getPieChartOptions("Ventas de Bolsas"),
    });
  }

  function getPieChartOptions(title) {
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: "right",
          labels: {
            padding: 20,
            usePointStyle: true,
            font: { size: 12 },
          },
        },
        title: {
          display: true,
          text: title,
          font: { size: 16, weight: "bold" },
          padding: { top: 10, bottom: 20 },
        },
        tooltip: {
          callbacks: {
            label: function (context) {
              return `${context.label}: ${context.raw}`;
            },
          },
        },
      },
    };
  }

  // Actualizar gráfico con timeout
  async function updatePieChart() {
    const rawDate = datePicker.value;
    if (!rawDate) return;

    const selectedDate = formatDate(rawDate);
    showMessage("Cargando datos...", "info");
    showLoader();

    // Timeout para evitar carga infinita
    const timeoutPromise = new Promise((_, reject) =>
      setTimeout(() => reject(new Error("Tiempo de espera agotado")), 10000)
    );

    try {
      const url = `/AnalisisVentas/GetChartData?period=custom&category=${currentCategory}&customDate=${selectedDate}`;
      console.log("Solicitando datos:", url);

      const response = await Promise.race([fetch(url), timeoutPromise]);

      if (!response.ok) {
        throw new Error(`Error ${response.status}: ${response.statusText}`);
      }

      const data = await response.json();
      console.log("Datos recibidos:", data);

      if (!data.success) {
        throw new Error(data.error || "Respuesta no exitosa del servidor");
      }

      // Procesar datos
      const hasValidData = data.values && data.values.some((v) => v > 0);

      if (!hasValidData) {
        showEmptyChart("No hay ventas para esta fecha");
        return;
      }

      // Actualizar gráfico
      updateChartData(data.labels, data.values, rawDate);
    } catch (error) {
      console.error("Error al actualizar gráfico:", error);
      showEmptyChart(error.message);
    } finally {
      hideLoader();
    }
  }

  function updateChartData(labels, values, date) {
    // Limitar a 10 elementos para mejor visualización
    const maxItems = 10;
    const limitedLabels = labels.slice(0, maxItems);
    const limitedValues = values.slice(0, maxItems);

    pieChart.data.labels = limitedLabels;
    pieChart.data.datasets[0].data = limitedValues;
    pieChart.data.datasets[0].backgroundColor = generateColors(
      limitedLabels.length
    );
    pieChart.options.plugins.title.text = `Ventas de Bolsas (${date})`;

    pieChart.update();
    showMessage("", "success");
  }

  function showEmptyChart(message) {
    pieChart.data.labels = ["Sin datos"];
    pieChart.data.datasets[0].data = [1];
    pieChart.data.datasets[0].backgroundColor = ["rgba(200, 200, 200, 0.7)"];
    pieChart.options.plugins.title.text = "Ventas de Bolsas";
    pieChart.update();

    showMessage(message || "No hay datos disponibles", "warning");
  }

  function showLoader() {
    const chartCanvas = document.getElementById("pieChart");
    chartCanvas.style.visibility = "hidden";

    let loader = chartCanvas.parentNode.querySelector(".chart-loader");
    if (!loader) {
      loader = document.createElement("div");
      loader.className = "chart-loader spinner-border text-success";
      loader.style.position = "absolute";
      loader.style.top = "50%";
      loader.style.left = "50%";
      loader.style.transform = "translate(-50%, -50%)";
      chartCanvas.parentNode.appendChild(loader);
    }
  }

  function hideLoader() {
    const chartCanvas = document.getElementById("pieChart");
    chartCanvas.style.visibility = "visible";

    const loader = chartCanvas.parentNode.querySelector(".chart-loader");
    if (loader) loader.remove();
  }

  function showMessage(text, type) {
    messageContainer.innerHTML = text;
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

  function generateColors(count) {
    const colors = [];
    const baseHue = 120; // Verde base
    const hueStep = 30; // Variación

    for (let i = 0; i < count; i++) {
      const hue = (baseHue + i * hueStep) % 360;
      colors.push(`hsla(${hue}, 70%, 50%, 0.7)`);
    }

    return colors;
  }

  // Event listeners
  datePicker.addEventListener("change", function () {
    const selectedDate = new Date(this.value);
    const today = new Date();

    if (selectedDate > today) {
      showMessage("No se pueden seleccionar fechas futuras", "error");
      this.valueAsDate = today;
      return;
    }

    updatePieChart();
  });

  // Inicialización
  initPieChart();
  updatePieChart();
});
