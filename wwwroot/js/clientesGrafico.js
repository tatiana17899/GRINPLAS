document.addEventListener("DOMContentLoaded", function () {
  const datePicker = document.getElementById("clientes-date-picker");
  const ctx = document.getElementById("clientesChart").getContext("2d");

  // Configurar fecha inicial como hoy
  const today = new Date();
  datePicker.value = today.toISOString().split("T")[0];

  // Inicializar gráfico
  const chart = new Chart(ctx, {
    type: "line",
    data: {
      labels: [
        "Lunes",
        "Martes",
        "Miércoles",
        "Jueves",
        "Viernes",
        "Sábado",
        "Domingo",
      ],
      datasets: [
        {
          label: "Clientes registrados",
          data: [],
          backgroundColor: "rgba(9, 102, 35, 0.2)",
          borderColor: "rgba(9, 102, 35, 1)",
          borderWidth: 2,
          tension: 0.4,
          fill: true,
        },
      ],
    },
    options: {
      responsive: true,
      plugins: {
        title: {
          display: true,
          text: "Clientes por semana",
          font: {
            size: 16,
          },
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            stepSize: 1,
          },
        },
      },
    },
  });

  // Cargar datos iniciales
  loadData(today);

  // Manejar cambio de fecha
  datePicker.addEventListener("change", function () {
    loadData(new Date(this.value));
  });

  async function loadData(date) {
    try {
      const fecha = date.toISOString().split("T")[0];
      const response = await fetch(
        `/ClienteGrafico/GetClientesPorSemana?fecha=${fecha}`
      );

      if (!response.ok) {
        throw new Error(`Error HTTP: ${response.status}`);
      }

      const result = await response.json();

      if (!result.success) {
        throw new Error(result.error || "Error en los datos");
      }

      // Actualizar gráfico
      chart.data.datasets[0].data = result.data;
      chart.options.plugins.title.text = `Clientes registrados (${result.inicioSemana} a ${result.finSemana})`;
      chart.update();
    } catch (error) {
      console.error("Error al cargar datos:", error);
      // Mostrar gráfico vacío
      chart.data.datasets[0].data = [0, 0, 0, 0, 0, 0, 0];
      chart.options.plugins.title.text = "Error al cargar datos";
      chart.update();
    }
  }
});
