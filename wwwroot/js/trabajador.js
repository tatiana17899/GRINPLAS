$(document).ready(function () {
  var originalData = {}; // Objeto para almacenar los datos originales

  // Inicializar DataTable
  var table = $("#trabajadoresTable").DataTable({
    language: {
      search: "Buscar:",
      paginate: {
        previous: "Anterior",
        next: "Siguiente",
      },
      emptyTable: " ",
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
          console.log("Row data for buttons:", row);
          return `
          <button class="btn btn-sm edit-btn" 
                  data-id="${row.idTrabajador || ""}" 
                  data-nombre="${(row.nombre || "").replace(/"/g, '"')}" 
                  data-apellidos="${(row.apellidos || "").replace(/"/g, '"')}" 
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
    initComplete: function () {
      let api = this.api();
      if (api.data().count() === 0) {
        $("#alertaSinTrabajadores").show();
      } else {
        $("#alertaSinTrabajadores").hide();
      }
    },
  });

  table.on("draw", function () {
    if (table.data().count() === 0) {
      $("#alertaSinTrabajadores").show();
    } else {
      $("#alertaSinTrabajadores").hide();
    }
    // Forzar estilos en los botones después de cada redibujado
    $(".edit-btn").css({
      "background-color": "#096623",
      color: "white",
      border: "none",
    });
    $(".delete-btn").css({
      "background-color": "#096623",
      color: "white",
      border: "none",
    });
  });

  // Manejar click en botón editar
  $(document).on("click", ".edit-btn", function () {
    console.log("Edit button clicked");
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

    // Almacenar datos originales para comparación
    originalData = {
      id: id,
      nombre: nombre || "",
      apellidos: apellidos || "",
      telefono: telefono || "",
      dni: dni || "",
      posicionLaboral: posicion || "Vendedor",
      sueldo: parseFloat(sueldo || "0.00"),
    };

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
    var form = this;
    if (!form.checkValidity()) {
      e.preventDefault();
      e.stopPropagation();
      $("#alertaCamposVacios").show(); // Mensaje: "Debe completar todos los campos obligatorios"
      setTimeout(function () {
        $("#alertaCamposVacios").fadeOut();
      }, 3000);
      return; // Detener si el formulario no es válido
    }

    e.preventDefault();

    // Obtener valores actuales del formulario
    var currentData = {
      id: $("#editId").val(),
      nombre: $("#editNombre").val(),
      apellidos: $("#editApellidos").val(),
      telefono: $("#editTelefono").val(),
      dni: $("#editDNI").val(),
      posicionLaboral: $("#editPosicion").val(),
      sueldo: parseFloat($("#editSueldo").val() || "0.00"),
    };

    // Verificar si se realizaron cambios
    var hasChanges = false;
    for (var key in originalData) {
      if (originalData[key] != currentData[key]) {
        hasChanges = true;
        break;
      }
    }

    if (!hasChanges) {
      Swal.fire({
        icon: "info",
        title: "No se han realizado cambios.",
        showConfirmButton: false, // Sin botón de confirmación
        timer: 3000 
      });
      // Cerrar el modal de edición y recargar la tabla para mostrar la vista de historial
      $("#editModal").modal("hide");
      $("#editModal").on("hidden.bs.modal", function () {
        $("#editModal").off("hidden.bs.modal");
        table.ajax.reload(null, false); // Recargar la tabla para asegurar la vista de historial
      });
      return;
    }

    // Si hay cambios, cerrar modal de edición y mostrar modal de confirmación
    $("#editModal").modal("hide");
    $("#editModal").on("hidden.bs.modal", function () {
      $("#editModal").off("hidden.bs.modal");
      $("#confirmacionEditModal").modal("show");
    });
  });

  // Configurar el botón de confirmación para enviar el formulario
  $("#btnConfirmarEditar").off("click").on("click", function () {
    $("#confirmacionEditModal").modal("hide");

    var formData = $("#editForm").serialize();
    $.ajax({
      url: $("#editForm").attr("action"),
      type: "POST",
      data: formData,
      headers: {
        RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
      },
      success: function (response) {
        if (response.success) {
          $("#exitoEditModal").modal("show");
          setTimeout(function () {
            $("#exitoEditModal").modal("hide");
            table.ajax.reload(null, false);
          }, 2000);
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
    $("#confirmacionDeleteModal").data("trabajador-id", id);
    $("#confirmacionDeleteModal").modal("show");
  });

  // Manejar confirmación de eliminación
  $("#btnConfirmarEliminar").on("click", function () {
    var id = $("#confirmacionDeleteModal").data("trabajador-id");
    $("#confirmacionDeleteModal").modal("hide");

    $.ajax({
      url: urls.delete,
      type: "POST",
      data: {
        id: id,
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
      },
      success: function (response) {
        if (response.success) {
          $("#exitoDeleteModal").modal("show");
          setTimeout(function () {
            $("#exitoDeleteModal").modal("hide");
            table.ajax.reload(null, false);
          }, 2000);
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
        Swal.fire("Error!", "Ocurrió un error al eliminar: " + error, "error");
      },
    });
  });

  // Manejar envío del formulario de creación
  $("#createForm").on("submit", function (e) {
    var form = this;
    if (!form.checkValidity()) {
      e.preventDefault();
      e.stopPropagation();
      $("#alertaCamposVaciosCrear").show();
      setTimeout(function () {
        $("#alertaCamposVaciosCrear").fadeOut();
      }, 3000);
      return;
    }
  });
});