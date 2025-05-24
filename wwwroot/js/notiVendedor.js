$(document).ready(function () {
  // Inicializar tooltips y popovers
  $('[data-bs-toggle="tooltip"]').tooltip();
  $('[data-bs-toggle="popover"]').popover();

  // Cargar contador inicial
  cargarContadorNotificaciones();

  // Manejar apertura del dropdown de notificaciones
  $("#navbarDropdownNotifications").on("show.bs.dropdown", function () {
    cargarNotificaciones();
  });

  // Manejar clic en una notificación individual
  $(document).on("click", ".notification-item", function (e) {
    e.preventDefault();
    var notificacionId = $(this).data("id");
    var pedidoId = $(this).data("pedido-id");

    // Marcar como leída
    $.ajax({
      url: "/Notificaciones/MarcarComoLeida",
      type: "POST",
      data: { id: notificacionId },
      success: function () {
        cargarContadorNotificaciones();

        // Redirigir si es un pedido
        if (pedidoId) {
          window.location.href = "/Pedidos/GerenteGeneral#" + pedidoId;
        }
      },
      error: function (xhr, status, error) {
        console.error("Error al marcar notificación como leída:", error);
        alert("Error al procesar la notificación. Intente nuevamente.");
      },
    });
  });

  // Manejar "Marcar todas como leídas"
  $("#markAllAsRead").click(function (e) {
    e.preventDefault();
    e.stopPropagation();

    $.ajax({
      url: "/Notificaciones/MarcarTodasComoLeidas",
      type: "POST",
      success: function () {
        // Animación para ocultar notificaciones
        $(".notification-list")
          .children()
          .each(function (index) {
            $(this)
              .delay(100 * index)
              .fadeOut(300, function () {
                $(this).remove();

                // Mostrar mensaje si no hay más notificaciones
                if ($(".notification-list").children().length === 0) {
                  $(".notification-list").html(
                    '<div class="text-center py-3">No hay notificaciones nuevas</div>'
                  );
                }
              });
          });

        // Actualizar contador
        $(".notification-badge").text("");
      },
      error: function (xhr, status, error) {
        console.error("Error al marcar todas como leídas:", error);
        alert("Error al marcar notificaciones. Intente nuevamente.");
      },
    });
  });

  // Verificar nuevos pedidos cada 30 segundos
  var notificationInterval = setInterval(function () {
    verificarNuevosPedidos();
  }, 30000);

  // Limpiar intervalo al salir de la página
  $(window).on("beforeunload", function () {
    clearInterval(notificationInterval);
  });

  // Verificar nuevos pedidos al cargar
  verificarNuevosPedidos();
});

// Función para cargar notificaciones
function cargarNotificaciones() {
  var notificationList = $(".notification-list");
  notificationList.html(
    '<div class="text-center py-3">Cargando notificaciones...</div>'
  );

  $.get("/Notificaciones/ObtenerNotificacionesNoLeidas")
    .done(function (response) {
      if (response.success && response.data) {
        actualizarListaNotificaciones(response.data);
      } else {
        notificationList.html(
          '<div class="text-center py-3">No hay notificaciones nuevas</div>'
        );
      }
    })
    .fail(function () {
      notificationList.html(
        '<div class="text-center py-3 text-danger">Error al cargar notificaciones</div>'
      );
    });
}
function actualizarListaNotificaciones(notificaciones) {
  var notificationList = $(".notification-list");
  notificationList.empty();

  if (notificaciones.length === 0) {
    notificationList.html(
      '<div class="text-center py-3">No hay notificaciones nuevas</div>'
    );
    return;
  }

  notificaciones.forEach(function (notificacion) {
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
                        background-color: #4a6bff;
                        border-radius: 50%;
                    "></div>
                    <h6 class="fw-bold ms-3 mt-1" style="font-size: 14px">${
                      notificacion.titulo || "Notificación"
                    }</h6>
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
                        <a href="#" class="notification-item text-decoration-none fw-bold" 
                            data-id="${notificacion.notificacionId}"
                            data-pedido-id="${notificacion.pedidoId || ""}"
                            style="color: rgba(19, 155, 58, 0.375); font-size: 13px">
                            Ver
                        </a>
                    </div>
                    <div class="text-end mt-1" style="font-size: 11px; color: #6c757d;">
                        ${formatearFecha(notificacion.fechaCreacion)}
                    </div>
                </div>
            </div>`;

    notificationList.append(item);
  });
}

// Función para cargar el contador de notificaciones
function cargarContadorNotificaciones() {
  $.get("/Notificaciones/ContadorNoLeidas")
    .done(function (count) {
      $(".notification-badge").text(count > 0 ? count : "");
    })
    .fail(function () {
      console.error("Error al cargar contador de notificaciones");
    });
}

// Función para verificar nuevos pedidos
// Función para verificar nuevos pedidos
function verificarNuevosPedidos() {
  console.log("Verificando nuevos pedidos...");
  $.get("/Pedidos/VerificarNuevosPedidos")
    .done(function (response) {
      console.log("Respuesta de verificación:", response);
      if (response.success && response.data && response.data.length > 0) {
        console.log("Nuevos pedidos encontrados:", response.data.length);
        // Mostrar notificación toast para cada nuevo pedido
        response.data.forEach(function (pedido) {
          console.log("Mostrando notificación para pedido:", pedido.pedidoId);
          mostrarNotificacionToast(
            "Nuevo Pedido",
            `El cliente ${pedido.clienteNombre} ha realizado un nuevo pedido (ID: ${pedido.pedidoId})`
          );
        });

        // Forzar recarga de notificaciones
        cargarNotificaciones();
        cargarContadorNotificaciones();
      } else {
        console.log("No se encontraron nuevos pedidos");
      }
    })
    .fail(function (error) {
      console.error("Error al verificar nuevos pedidos:", error);
    });
}

// Función para mostrar notificación toast
function mostrarNotificacionToast(titulo, mensaje) {
  // Solo mostrar si no estamos en la página de pedidos
  if (!window.location.pathname.includes("/Pedidos/")) {
    const toast = `
            <div class="toast show" role="alert" aria-live="assertive" aria-atomic="true" style="position: fixed; top: 20px; right: 20px; min-width: 250px; z-index: 1100;">
                <div class="toast-header" style="background-color: #c5dbcd;">
                    <strong class="me-auto">${titulo}</strong>
                    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
                <div class="toast-body">
                    ${mensaje}
                </div>
            </div>`;

    $("body").append(toast);

    // Auto-ocultar después de 5 segundos
    setTimeout(function () {
      $(".toast").remove();
    }, 5000);
  }
}

// Función para formatear fecha
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
