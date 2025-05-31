$(document).ready(function () {
  // Inicializar componentes Bootstrap
  $('[data-bs-toggle="tooltip"]').tooltip();
  $('[data-bs-toggle="popover"]').popover();

  // Cargar contador inicial
  cargarContadorNotificaciones();

  // Configurar dropdown de notificaciones
  $("#navbarDropdownNotifications").on("show.bs.dropdown", function () {
    cargarNotificaciones();
  });

  // Manejar "Marcar todas como leídas"
  $("#markAllAsRead").click(function (e) {
    e.preventDefault();
    e.stopPropagation();

    $.ajax({
      url: "/Notificaciones/MarcarTodasComoLeidas",
      type: "POST",
      success: function () {
        $(".notification-list")
          .children()
          .each(function (index) {
            $(this)
              .delay(100 * index)
              .fadeOut(300, function () {
                $(this).remove();
                if ($(".notification-list").children().length === 0) {
                  $(".notification-list").html(
                    '<div class="dropdown-item text-center py-3">No hay notificaciones nuevas</div>'
                  );
                }
              });
          });
        $(".notification-badge").text("");
      },
      error: function (xhr, status, error) {
        console.error("Error al marcar notificaciones:", error);
        alert("Error al procesar la solicitud");
      },
    });
  });

  // Actualización periódica del contador
  var notificationInterval = setInterval(cargarContadorNotificaciones, 30000);
  $(window).on("beforeunload", function () {
    clearInterval(notificationInterval);
  });
});

// Función principal para cargar notificaciones
function cargarNotificaciones() {
  var $notificationList = $(".notification-list");
  $notificationList.html(
    '<div class="dropdown-item text-center py-3">Cargando notificaciones...</div>'
  );

  $.get("/Notificaciones/ObtenerNotificacionesNoLeidas")
    .done(function (response) {
      $notificationList.empty();

      if (response.success && response.data && response.data.length > 0) {
        response.data.forEach(function (notificacion) {
          // Configuración de estilo según tipo (MANTENIENDO TU DISEÑO EXACTO)
          var tipoIndicator, circleColor;
          switch ((notificacion.tipo || "general").toLowerCase()) {
            case "pago":
              tipoIndicator = "Estado de Pago";
              circleColor = "#4a6bff";
              break;
            case "estado":
              tipoIndicator = "Estado Pedido";
              circleColor = "#b45656";
              break;
            case "nuevo_pedido":
              tipoIndicator = "Confirmación Pedido";
              circleColor = "#28a745";
              break;
            case "direccion":
              tipoIndicator = "Dirección Actualizada";
              circleColor = "#28a745";
              break;
            default:
              tipoIndicator = "Notificación";
              circleColor = "#6c757d";
          }

          // Plantilla HTML con tus estilos inline originales
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
                    ${notificacion.mensaje || "Sin mensaje detallado"}
                  </p>
                  <a href="/HistorialPedidos/Index" 
                     class="notification-item text-decoration-none fw-bold" 
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

          $notificationList.append(item);
        });
      } else {
        $notificationList.html(
          '<div class="dropdown-item text-center py-3">No hay notificaciones</div>'
        );
      }
    })
    .fail(function () {
      $notificationList.html(
        '<div class="dropdown-item text-center py-3">Error al cargar notificaciones</div>'
      );
    });
}

// Manejador de clic en notificaciones (Versión optimizada)
$(document).on("click", ".notification-item", function (e) {
  e.preventDefault();
  var notificacionId = $(this).data("id");

  $.ajax({
    url: "/Notificaciones/MarcarComoLeida",
    type: "POST",
    data: { id: notificacionId },
    success: function () {
      cargarContadorNotificaciones();
      window.location.href = "/HistorialPedidos/Index";
    },
    error: function () {
      window.location.href = "/HistorialPedidos/Index";
    },
  });
});

// Función para cargar el contador de notificaciones
function cargarContadorNotificaciones() {
  $.get("/Notificaciones/ContadorNoLeidas")
    .done(function (count) {
      $(".notification-badge").text(count > 0 ? count : "");
    })
    .fail(function (xhr, status, error) {
      console.error("Error al cargar contador:", error);
    });
}

// Función para formatear fechas (Mantenida igual)
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
