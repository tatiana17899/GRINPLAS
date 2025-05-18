// cliente-notificaciones.js - Versión corregida
$(document).ready(function () {
  // Cargar notificaciones al iniciar
  cargarNotificacionesCliente();

  // Configurar intervalo para actualizar (cada 30 segundos)
  setInterval(cargarNotificacionesCliente, 30000);

  // Manejar clic en notificaciones
  $(document).on("click", ".notification-item", function (e) {
    e.preventDefault();
    var notificacionId = $(this).data("id");
    var url = $(this).attr("href");
    marcarNotificacionComoLeida(notificacionId, url);
  });

  // Marcar todas como leídas
  $("#markAllAsRead").click(function (e) {
    e.preventDefault();
    $.post("/Notificaciones/MarcarTodasComoLeidas", function () {
      cargarNotificacionesCliente();
    }).fail(function () {
      console.error("Error al marcar todas como leídas");
    });
  });
});

function cargarNotificacionesCliente() {
  $.get("/Notificaciones/ObtenerNotificacionesNoLeidas")
    .done(function (response) {
      if (response.success) {
        actualizarListaNotificacionesCliente(response.data);
        actualizarContadorNotificaciones(response.data.length);
      } else {
        console.error("Error en la respuesta:", response.message);
      }
    })
    .fail(function (error) {
      console.error("Error al cargar notificaciones:", error);
    });
}

function actualizarListaNotificacionesCliente(notificaciones) {
  const $list = $(".notification-list");

  if (!notificaciones || notificaciones.length === 0) {
    $list.html(
      '<div class="dropdown-item text-center py-3">No hay notificaciones nuevas</div>'
    );
    return;
  }

  let html = "";
  notificaciones.forEach((notif) => {
    const tipo = notif.tipo || "general";
    const icono = obtenerIconoPorTipo(tipo);

    html += `
            <a href="${obtenerUrlNotificacion(
              notif
            )}" class="dropdown-item notification-item ${
      notif.leida ? "" : "fw-bold"
    }" data-id="${notif.notificacionId}">
                <div class="d-flex justify-content-between align-items-start">
                    <div>
                        <i class="fas ${icono} me-2 text-success"></i>
                        <span>${notif.titulo}</span>
                    </div>
                    <small class="text-muted">${formatearFecha(
                      notif.fechaCreacion
                    )}</small>
                </div>
                <div class="mt-1 ps-3 text-muted">${notif.mensaje}</div>
            </a>
        `;
  });

  $list.html(html);
}

function obtenerIconoPorTipo(tipo) {
  switch (tipo.toLowerCase()) {
    case "pago":
      return "fa-credit-card";
    case "pedido":
    case "nuevo_pedido":
      return "fa-shopping-cart";
    case "estado":
      return "fa-truck";
    default:
      return "fa-bell";
  }
}

function obtenerUrlNotificacion(notif) {
  if (notif.pedidoId) {
    return `/HistorialPedidos/Detalle/${notif.pedidoId}`;
  }
  return "#";
}

function formatearFecha(fechaString) {
  if (!fechaString) return "";

  try {
    const fecha = new Date(fechaString);
    return fecha.toLocaleTimeString("es-ES", {
      hour: "2-digit",
      minute: "2-digit",
      day: "2-digit",
      month: "2-digit",
    });
  } catch (e) {
    console.error("Error formateando fecha:", e);
    return "";
  }
}

function actualizarContadorNotificaciones() {
  $.get("/Notificaciones/ContadorNoLeidas", function (count) {
    $(".notification-badge")
      .text(count > 0 ? count : "")
      .toggle(count > 0);
  });
}

function marcarNotificacionComoLeida(id, url) {
  $.post("/Notificaciones/MarcarComoLeida", { id: id })
    .done(function () {
      if (url && url !== "#") {
        window.location.href = url;
      } else {
        cargarNotificacionesCliente();
      }
    })
    .fail(function (error) {
      console.error("Error al marcar como leída:", error);
      if (url && url !== "#") {
        window.location.href = url;
      }
    });
}
