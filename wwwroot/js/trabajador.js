$(document).ready(function () {
  // Inicializar DataTable
  var table = $("#trabajadoresTable").DataTable({
    language: {
      url: "//cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json",
      paginate: {
        previous: "Anterior",
        next: "Siguiente",
      },
    },
    paging: true,
    lengthChange: false,
    searching: true,
    info: true,
    processing: true,
    serverSide: false,
    ajax: {
      url: urls.getTrabajadores,
      type: "GET",
      dataSrc: "data",
    },
    columns: [
      { data: "nombre" },
      { data: "apellidos" },
      { data: "telefono" },
      { data: "dni" },
      {
        data: "posicionLaboral",
        render: function (data, type, row) {
          let icon = "";
          if (data === "Vendedor") {
            icon = '<i class="fas fa-shopping-bag me-2"></i>';
          } else if (data === "GerenteGeneral") {
            icon = '<i class="fas fa-user-tie me-2"></i>';
          } else {
            icon = '<i class="fas fa-user-tag me-2"></i>';
          }
          return icon + data;
        },
      },
      {
        data: "sueldo",
        render: function (data, type, row) {
          // Debugging: verificar qué datos llegan
          console.log("Sueldo data:", data, "Row:", row);
          if (data !== null && data !== undefined && data !== "") {
            return "S/ " + parseFloat(data).toFixed(2);
          }
          return "S/ 0.00";
        },
      },
      {
        data: null,
        render: function (data, type, row) {
          // Debugging: verificar los datos de la fila
          console.log("Row data for buttons:", row);

          return `
          <button class="btn btn-sm edit-btn" 
                  data-id="${row.idTrabajador || ""}" 
                  data-nombre="${(row.nombre || "").replace(/"/g, "&quot;")}" 
                  data-apellidos="${(row.apellidos || "").replace(
                    /"/g,
                    "&quot;"
                  )}" 
                  data-telefono="${row.telefono || ""}" 
                  data-dni="${row.dni || ""}" 
                  data-posicionlaboral="${row.posicionLaboral || ""}" 
                  data-sueldo="${row.sueldo || "0"}"
                  style="background-color: #096623 !important; color: white !important; border: none !important;"
                  data-bs-toggle="modal" 
                  data-bs-target="#editModal">
              <i class="fas fa-edit"></i> 
          </button>
          <button class="btn btn-sm delete-btn" 
                  data-id="${row.idTrabajador || ""}" 
                  style="background-color: #096623 !important; color: white !important; border: none !important; margin-left: 5px;">
              <i class="fas fa-trash-alt"></i> 
          </button>
        `;
        },
        orderable: false,
      },
    ],
  });

  // Manejar click en botón editar
  $(document).on("click", ".edit-btn", function () {
    // Debugging
    console.log("Edit button clicked");

    // Obtener datos del botón
    var $button = $(this);
    var id = $button.data("id");
    var nombre = $button.data("nombre");
    var apellidos = $button.data("apellidos");
    var telefono = $button.data("telefono");
    var dni = $button.data("dni");
    var posicion = $button.data("posicionlaboral");
    var sueldo = $button.data("sueldo");

    console.log("Datos extraídos del botón:", {
      id: id,
      nombre: nombre,
      apellidos: apellidos,
      telefono: telefono,
      dni: dni,
      posicion: posicion,
      sueldo: sueldo,
    });

    // Verificar que los elementos del modal existen
    console.log("Elementos del modal:", {
      editId: $("#editId").length,
      editNombre: $("#editNombre").length,
      editApellidos: $("#editApellidos").length,
      editTelefono: $("#editTelefono").length,
      editDNI: $("#editDNI").length,
      editPosicion: $("#editPosicion").length,
      editSueldo: $("#editSueldo").length,
    });

    // Llenar los campos del modal
    $("#editId").val(id);
    $("#editNombre").val(nombre || "");
    $("#editApellidos").val(apellidos || "");
    $("#editTelefono").val(telefono || "");
    $("#editDNI").val(dni || "");
    $("#editPosicion").val(posicion || "Vendedor");
    $("#editSueldo").val(sueldo || "0.00");

    // Configurar la acción del formulario
    $("#editForm").attr("action", urls.edit);

    console.log("Valores establecidos en el modal:", {
      id: $("#editId").val(),
      nombre: $("#editNombre").val(),
      apellidos: $("#editApellidos").val(),
      telefono: $("#editTelefono").val(),
      dni: $("#editDNI").val(),
      posicion: $("#editPosicion").val(),
      sueldo: $("#editSueldo").val(),
    });
  });

  // Manejar envío del formulario de edición
  $("#editForm").on("submit", function (e) {
    e.preventDefault();
    var formData = $(this).serialize();
    $.ajax({
      url: $(this).attr("action"),
      type: "POST",
      data: formData,
      success: function (response) {
        if (response.success) {
          $("#editModal").modal("hide");
          Swal.fire(
            "Actualizado!",
            "El trabajador ha sido actualizado correctamente.",
            "success"
          ).then(() => {
            table.ajax.reload(null, false);
          });
        } else {
          Swal.fire(
            "Error!",
            response.message || "No se pudo actualizar el trabajador.",
            "error"
          );
        }
      },
      error: function (xhr, status, error) {
        console.error(xhr.responseText);
        Swal.fire(
          "Error!",
          "Ocurrió un error al actualizar: " + error,
          "error"
        );
      },
    });
  });

  // Configurar eliminación
  $(document).on("click", ".delete-btn", function () {
    var id = $(this).data("id");
    Swal.fire({
      title: "¿Estás seguro?",
      text: "¡No podrás revertir esto!",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#d33",
      cancelButtonColor: "#3085d6",
      confirmButtonText: "Sí, eliminarlo!",
      cancelButtonText: "Cancelar",
    }).then((result) => {
      if (result.isConfirmed) {
        $.ajax({
          url: urls.delete,
          type: "POST",
          data: {
            id: id,
            __RequestVerificationToken: $(
              'input[name="__RequestVerificationToken"]'
            ).val(),
          },
          success: function (response) {
            if (response.success) {
              Swal.fire(
                "Eliminado!",
                "El trabajador ha sido eliminado.",
                "success"
              ).then(() => {
                table.ajax.reload(null, false);
              });
            } else {
              Swal.fire(
                "Error!",
                response.message || "No se pudo eliminar el trabajador.",
                "error"
              );
            }
          },
          error: function (xhr, status, error) {
            console.error("Error details:", xhr.responseText);
            Swal.fire(
              "Error!",
              "Ocurrió un error al eliminar: " + error,
              "error"
            );
          },
        });
      }
    });
  });

  // Aplicar estilos después de que la tabla se cargue
  table.on("draw", function () {
    // Forzar estilos en los botones después de cada redibujado
    $(".edit-btn").css({
      "background-color": "#096623",
      color: "white",
      border: "none",
    });

    $(".delete-btn").css({
      "background-color": "#dc3545",
      color: "white",
      border: "none",
    });
  });
});
