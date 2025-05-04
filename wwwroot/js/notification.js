$(document).ready(function () {
  $('[data-bs-toggle="tooltip"]').tooltip();
  $('[data-bs-toggle="popover"]').popover();

  // Carga el contador de notificaciones al cargar la página
  cargarContadorNotificaciones();

  // Maneja el evento de mostrar el dropdown de notificaciones
  $("#navbarDropdownNotifications").on("show.bs.dropdown", function () {
    cargarNotificaciones();
  });

  // Maneja el clic en una notificación individual
  $(document).on("click", ".notification-item", function (e) {
    e.preventDefault();
    var notificacionId = $(this).data("id");
    var url = $(this).attr("href");

    // Primero marcar como leída
    $.ajax({
      url: "/Notificaciones/MarcarComoLeida",
      type: "POST",
      data: { id: notificacionId },
      success: function () {
        // Actualizar contador sin eliminar la notificación
        cargarContadorNotificaciones();

        // Redirigir al destino correcto
        if (url && url !== "#") {
          window.location.href = url;
        }
      },
      error: function (xhr, status, error) {
        console.error("Error al marcar la notificación como leída:", error);
        alert(
          "Error al marcar la notificación como leída. Por favor intenta nuevamente."
        );
      },
    });
  });

  // Maneja el clic en "Marcar todas como leídas"
  $("#markAllAsRead").click(function (e) {
    e.preventDefault();
    e.stopPropagation(); // Importante para evitar que se cierre el dropdown

    $.ajax({
      url: "/Notificaciones/MarcarTodasComoLeidas",
      type: "POST",
      success: function () {
        // Ocultar todas las notificaciones con animación
        $(".notification-list")
          .children()
          .each(function (index) {
            $(this)
              .delay(100 * index)
              .fadeOut(300, function () {
                $(this).remove();

                // Verificar si es el último elemento
                if ($(".notification-list").children().length === 0) {
                  $(".notification-list").html(
                    '<div class="dropdown-item text-center py-3">No hay notificaciones nuevas</div>'
                  );
                }
              });
          });

        // Actualizar contador a 0
        $(".notification-badge").text("");
      },
      error: function (xhr, status, error) {
        console.error(
          "Error al marcar todas las notificaciones como leídas:",
          error
        );
        alert(
          "Error al marcar todas las notificaciones como leídas. Por favor intenta nuevamente."
        );
      },
    });
  });

  // Actualizar contador periódicamente
  var notificationInterval = setInterval(cargarContadorNotificaciones, 30000);

  // Limpiar intervalo al salir de la página
  $(window).on("beforeunload", function () {
    clearInterval(notificationInterval);
  });
});

// Modificación en cargarNotificaciones para manejar correctamente distintos tipos de notificaciones
function cargarNotificaciones() {
  var notificationList = $(".notification-list");
  if (!notificationList.length) return;

  notificationList.html(
    '<div class="dropdown-item text-center py-3">Cargando notificaciones...</div>'
  );

  $.get("/Notificaciones/ObtenerNotificacionesNoLeidas")
    .done(function (response) {
      notificationList.empty();

      if (response.success && response.data && response.data.length > 0) {
        response.data.forEach(function (notificacion) {
          // Validación y valores por defecto
          notificacion = {
            notificacionId: notificacion.notificacionId || 0,
            titulo: notificacion.titulo || "Notificación",
            mensaje: notificacion.mensaje || "Sin mensaje detallado",
            fechaCreacion:
              notificacion.fechaCreacion || new Date().toISOString(),
            tipo: (notificacion.tipo || "general").toLowerCase(),
            pedidoId: notificacion.pedidoId || null,
            leida: notificacion.leida || false,
          };

          var tipoClass, tipoIndicator, circleColor;

          switch (notificacion.tipo) {
            case "pago":
              tipoClass = "status-pago";
              tipoIndicator = "Estado de Pago";
              circleColor = "#4a6bff";
              break;
            case "estado": // Compatibilidad con el backend actual
            case "pedido":
              tipoClass = "status-pedido";
              tipoIndicator = "Estado Pedido";
              circleColor = "#b45656";
              break;
            default:
              tipoClass = "status-general";
              tipoIndicator = "Notificación";
              circleColor = "#6c757d";
          }

          // URL dinámica según tipo
          var url = "/HistorialPedidos/Index";
          if (notificacion.pedidoId) {
            if (notificacion.tipo === "pago") {
              url = "/HistorialPedidos/Index";
            } else {
              url = "/HistorialPedidos/Index";
            }
          }

          var item = `
            <div class="position-relative mb-3" style="
              width: 100%;
              background-color: rgba(19, 155, 57, 0.22);
              border-radius: 10px;
              padding: 8px;
            ">
              <div class="d-flex ms-2">
                <div style="
                  position: absolute;
                  top: 15px;
                  width: 10px;
                  height: 10px;
                  background-color: ${circleColor};
                  border-radius: 50%;
                "></div>
                <h6 class="fw-bold ms-3 mt-1" style="font-size: 14px">${tipoIndicator}</h6>
              </div>

              <div style="
                background-color: rgba(255, 255, 255, 0.65);
                border-radius: 8px;
                padding: 12px;
              ">
                <div class="d-flex justify-content-between align-items-end">
                  <p class="mb-0" style="font-size: 13px">
                    ${notificacion.mensaje}
                  </p>
                  <a href="${url}" class="notification-item text-decoration-none fw-bold" 
                    data-id="${notificacion.notificacionId}"
                    style="color: rgba(19, 155, 58, 0.375); font-size: 13px">
                    Ir a
                  </a>
                </div>
                <div class="text-end mt-1" style="font-size: 11px; color: #6c757d;">
                  ${formatearFecha(notificacion.fechaCreacion)}
                </div>
              </div>
            </div>`;

          notificationList.append(item);
        });
      } else {
        notificationList.html(
          '<div class="dropdown-item text-center py-3">No hay notificaciones</div>'
        );
      }
    })
    .fail(function () {
      notificationList.html(
        '<div class="dropdown-item text-center py-3">Error al cargar notificaciones</div>'
      );
    });
}

$(document).on("click", ".notification-item", function (e) {
  e.preventDefault();
  var notificacionId = $(this).data("id");
  var url = $(this).attr("href"); // Obtenemos la URL del enlace

  // Marcar como leída en el backend
  $.ajax({
    url: "/Notificaciones/MarcarComoLeida",
    type: "POST",
    data: { id: notificacionId },
    success: function () {
      cargarContadorNotificaciones();
      window.location.href = url;
    },
    error: function (xhr, status, error) {
      console.error("Error al marcar la notificación como leída:", error);
      window.location.href = url; // Redirigir igualmente
    },
  });
});

function cargarContadorNotificaciones() {
  $.get("/Notificaciones/ContadorNoLeidas")
    .done(function (count) {
      $(".notification-badge").text(count > 0 ? count : "");
    })
    .fail(function (xhr, status, error) {
      console.error("Error al cargar el contador de notificaciones:", error);
    });
}

function formatearFecha(fecha) {
  if (!fecha) return "Fecha desconocida";

  const fechaObj = new Date(fecha);
  const ahora = new Date();
  const diffMinutos = Math.floor((ahora - fechaObj) / (1000 * 60));

  if (diffMinutos < 1) return "Hace unos segundos";
  if (diffMinutos < 60)
    return `Hace ${diffMinutos} minuto${diffMinutos !== 1 ? "s" : ""}`;

  const diffHoras = Math.floor(diffMinutos / 60);
  if (diffHoras < 24)
    return `Hace ${diffHoras} hora${diffHoras !== 1 ? "s" : ""}`;

  const diffDias = Math.floor(diffHoras / 24);
  if (diffDias < 7) return `Hace ${diffDias} día${diffDias !== 1 ? "s" : ""}`;

  return fechaObj.toLocaleDateString("es-ES", {
    day: "numeric",
    month: "short",
    year: "numeric",
  });
}
