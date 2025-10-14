document.addEventListener("DOMContentLoaded", function () {
  const filterButtons = {
    all: document.querySelector("#filter-all"),
    espera: document.querySelector("#filter-espera"),
    proceso: document.querySelector("#filter-proceso"),
    cancelado: document.querySelector("#filter-cancelado"),
    entregado: document.querySelector("#filter-entregado"),
  };
  Object.keys(filterButtons).forEach((status) => {
    if (filterButtons[status]) {
      filterButtons[status].addEventListener("click", () =>
        filterOrders(status)
      );
    }
  });
  
  function filterOrders(status) {
    const rows = document.querySelectorAll("#ordersTable tbody tr");
    let visibleRows = 0;
  
    rows.forEach((row) => {
      const statusCell = row.querySelector(".status-select");
      const currentStatus = statusCell ? statusCell.value : "";
  
      if (status === "all" || currentStatus === status) {
        row.style.display = "";
        visibleRows++;
      } else {
        row.style.display = "none";
      }
    });
  
    updateFilterButtonStyles(status);
  
    if (visibleRows === 0) {
      Swal.fire({
        title: "Sin resultados",
        text: "No se encontraron pedidos con ese filtro",
        icon: "info",
      });
    }
  }
  
  function updateFilterButtonStyles(activeFilter) {
    Object.keys(filterButtons).forEach((status) => {
      if (filterButtons[status]) {
        if (status === activeFilter) {
          filterButtons[status].classList.add("text-success", "fw-bold");
          filterButtons[status].classList.remove("text-muted");
        } else {
          filterButtons[status].classList.remove("text-success", "fw-bold");
          filterButtons[status].classList.add("text-muted");
        }
      }
    });
  }

  
  const btnFiltrar = document.querySelector("#btnFiltrar");
  const btnResetear = document.querySelector("#btnResetear");
  const fechaInicio = document.querySelector("#fechaInicio");
  const fechaFin = document.querySelector("#fechaFin");
  const ordersTable = document.querySelector("#ordersTable");

  if (btnFiltrar && btnResetear && fechaInicio && fechaFin && ordersTable) {
    btnFiltrar.addEventListener("click", filtrarPedidos);
    btnResetear.addEventListener("click", resetearFiltro);
  }

  function parseDate(dateStr) {
    const [day, month, year] = dateStr.split("/");
    return new Date(year, month - 1, day);
  }

  function filtrarPedidos() {
    try {
      if (!fechaInicio.value || !fechaFin.value) {
        Swal.fire("Error", "Por favor seleccione ambas fechas", "error");
        return;
      }

      const inicio = new Date(fechaInicio.value);
      const fin = new Date(fechaFin.value);
      fin.setHours(23, 59, 59, 999);

      if (inicio > fin) {
        Swal.fire(
          "Error",
          "La fecha de inicio no puede ser mayor a la fecha final",
          "error"
        );
        return;
      }

      const filas = ordersTable.querySelectorAll("tbody tr");
      let filasMostradas = 0;

      filas.forEach((fila) => {
        try {
          const fechaEmisionTexto = fila.cells[4].textContent.trim();
          const fechaEmision = parseDate(fechaEmisionTexto);

          if (fechaEmision >= inicio && fechaEmision <= fin) {
            fila.style.display = "";
            filasMostradas++;
          } else {
            fila.style.display = "none";
          }
        } catch (e) {
          console.error("Error procesando fila:", e);
          fila.style.display = "none";
        }
      });

      if (filasMostradas === 0) {
        Swal.fire(
          "Sin resultados",
          "No hay pedidos en el rango de fechas seleccionado",
          "info"
        );
      }
    } catch (error) {
      console.error("Error en filtrarPedidos:", error);
      Swal.fire("Error", "Ocurrió un problema al filtrar los pedidos", "error");
    }
  }

  function resetearFiltro() {
    fechaInicio.value = "";
    fechaFin.value = "";

    const filas = ordersTable.querySelectorAll("tbody tr");
    filas.forEach((fila) => {
      fila.style.display = "";
    });

    // También resetear los filtros de estado
    filterOrders("all");
  }

  // --------------------------
  // Manejo de guardado
  // --------------------------
  const botonesGuardar = document.querySelectorAll(".btn-confirmar-guardar");

  botonesGuardar.forEach((boton) => {
    boton.addEventListener("click", function () {
      const tr = this.closest("tr");
      const pedidoId = tr.dataset.pedidoId;
      const selectStatus = tr.querySelector('select[name="status"]');
      const selectPago = tr.querySelector('select[name="pago"]');
      const inputFechaEntrega = tr.querySelector('input[name="fechaEntrega"]');

      Swal.fire({
        title: "¿Deseas guardar los cambios?",
        text: "Se actualizará la información del pedido",
        icon: "question",
        showCancelButton: true,
        confirmButtonColor: "#198754",
        cancelButtonColor: "#d33",
        confirmButtonText: "Sí, guardar",
        cancelButtonText: "Cancelar",
      }).then((result) => {
        if (result.isConfirmed) {
          const formData = new FormData();
          formData.append("pedidoId", pedidoId);
          formData.append("status", selectStatus.value);
          formData.append("pago", selectPago.value);
          formData.append("fechaEntrega", inputFechaEntrega.value || "");

          fetch("/Pedidos/ActualizarPedido", {
            method: "POST",
            body: formData,
          })
            .then((response) => response.json())
            .then((data) => {
              if (data.success) {
                Swal.fire(
                  "¡Guardado!",
                  "Los cambios han sido guardados correctamente.",
                  "success"
                );
                // Resetear filtros después de guardar
                resetearFiltro();
              } else {
                Swal.fire(
                  "Error",
                  data.error || "Ha ocurrido un error al guardar los cambios.",
                  "error"
                );
              }
            })
            .catch((error) => {
              console.error("Error:", error);
              Swal.fire(
                "Error",
                "Ha ocurrido un error al procesar la solicitud.",
                "error"
              );
            });
        }
      });
    });
  });

  // Inicialización de jQuery (si es necesaria)
  $(function () {
    if ($(".fecha-entrega").length) {
      $(".fecha-entrega").each(function () {
        $(this).data("original-value", $(this).val());
      });
    }
  });
});

$(document).ready(function () {
  $("#ordersTable").DataTable({
    paging: true,
    lengthChange: false,
    searching: false,
    info: false,
    language: {
      paginate: {
        previous: "Anterior",
        next: "Siguiente",
      },
    },
  });
});
