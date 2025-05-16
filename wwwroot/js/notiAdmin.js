$(document).ready(function () {
  // Cargar contador inicial
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
});

// Cargar notificaciones de stock bajo
function cargarNotificacionesStockBajo() {
  var notificationList = $("#notificationList");
  notificationList.html(
    '<div class="text-center py-3">Cargando notificaciones...</div>'
  );

  $.get("/Notificaciones/ObtenerProductosStockBajo")
    .done(function (response) {
      if (response.success && response.data && response.data.length > 0) {
        actualizarListaStockBajo(response.data);
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
}

function actualizarListaStockBajo(productos) {
  var notificationList = $("#notificationList");
  notificationList.empty();

  productos.forEach(function (producto) {
    var item = `
      <div class="d-flex align-items-center gap-2 p-3 mb-2" style="background: #d1f5e0; border-radius: 12px;">
        <span style="color: #d10000; font-size: 1.2em;">●</span>
        <div>
          <div class="fw-bold">Alerta</div>
          <div class="bg-light mt-1 mb-0 py-1 px-2 rounded" style="font-size: 0.95em;">
            Solo queda ${producto.stock} unidad${
      producto.stock === 1 ? "" : "es"
    } del producto "${producto.nombre}"
          </div>
        </div>
      </div>
    `;
    notificationList.append(item);
  });
}

// Cargar contador de productos con stock bajo
function cargarContadorStockBajo() {
  $.get("/Notificaciones/ContadorProductosStockBajo")
    .done(function (count) {
      $("#notificationCounter").text(count > 0 ? count : "0");
    })
    .fail(function () {
      $("#notificationCounter").text("0");
    });
}
