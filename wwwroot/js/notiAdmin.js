$(document).ready(function () {
  cargarContadorStockBajo();

  // Mostrar notificaciones al abrir el dropdown
  $("#navbarDropdownNotifications").on("show.bs.dropdown", function () {
    cargarNotificacionesStockBajo();
  });

  // Botón para refrescar notificaciones
  $("#refreshNotifications").click(function (e) {
    e.preventDefault();
    cargarNotificacionesStockBajo();
    cargarContadorStockBajo();
  });

  // Marcar todas las notificaciones como leídas
  $(document).on("click", "#markAllAsRead", function () {
    $.post("/Notificaciones/MarcarTodasComoLeidas").done(function () {
      // Limpia la lista y el contador inmediatamente
      $("#notificationList").html(
        '<div class="text-center py-3">No hay alertas de stock.</div>'
      );
      $("#notificationCounter").text("0");
    });
  });
});

function cargarContadorStockBajo() {
  // Primero genera las notificaciones de bajo stock
  $.post("/Notificaciones/GenerarNotificacionesStockBajo").always(function () {
    // Luego consulta las no leídas
    $.get("/Notificaciones/ObtenerNotificacionesNoLeidas")
      .done(function (response) {
        if (response.success && response.data) {
          var count = response.data.filter(
            (n) => n.tipo === "bajo_stock"
          ).length;
          $("#notificationCounter").text(count > 0 ? count : "0");
        } else {
          $("#notificationCounter").text("0");
        }
      })
      .fail(function () {
        $("#notificationCounter").text("0");
      });
  });
}

function cargarNotificacionesStockBajo() {
  var notificationList = $("#notificationList");
  notificationList.empty();

  // Primero genera las notificaciones de bajo stock
  $.post("/Notificaciones/GenerarNotificacionesStockBajo").always(function () {
    // Luego consulta las no leídas
    $.get("/Notificaciones/ObtenerNotificacionesNoLeidas")
      .done(function (response) {
        if (response.success && response.data && response.data.length > 0) {
          let hayNotificaciones = false;
          response.data.forEach(function (noti) {
            if (noti.tipo === "bajo_stock") {
              hayNotificaciones = true;
              var item = `
                <div class="mb-3" style="background: #e6f4ea; border-radius: 12px; padding: 16px;">
                  <div class="d-flex align-items-center mb-2">
                    <span style="color: #d10000; font-size: 1.2em; margin-right: 8px;">●</span>
                    <span class="fw-bold" style="color: #555;">${noti.titulo}</span>
                  </div>
                  <div style="background: #d1f5e0; border-radius: 8px; padding: 10px 14px; color: #2d4a2d;">
                    ${noti.mensaje}
                  </div>
                </div>
              `;
              notificationList.append(item);
            }
          });
          if (!hayNotificaciones) {
            notificationList.html(
              '<div class="text-center py-3">No hay alertas de stock.</div>'
            );
          }
        } else {
          notificationList.html(
            '<div class="text-center py-3">No hay alertas de stock.</div>'
          );
        }
      })
      .fail(function () {
        notificationList.html(
          '<div class="text-center py-3 text-danger">Error al cargar notificaciones</div>'
        );
      });
  });
}

function marcarComoLeida(id) {
  $.post("/Notificaciones/MarcarComoLeida", { id: id }).done(function () {
    cargarNotificacionesStockBajo();
    cargarContadorStockBajo();
  });
}
