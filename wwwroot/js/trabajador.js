$(document).ready(function () {
  // Inicializar DataTable
  var table = $("#trabajadoresTable").DataTable({
    language: {
      url: "//cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json",
    },
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
      { data: "posicionLaboral" },
      {
        data: "idTrabajador",
        render: function (data, type, row) {
          return `
                        <button class="btn btn-sm btn-warning edit-btn" data-id="${data}" 
                                data-nombre="${row.nombre}" 
                                data-apellidos="${row.apellidos}" 
                                data-telefono="${row.telefono}" 
                                data-dni="${row.dni}" 
                                data-posicion="${row.posicionLaboral}" 
                                data-bs-toggle="modal" 
                                data-bs-target="#editModal">
                            <i class="fas fa-edit"></i> Editar
                        </button>
                        <button class="btn btn-sm btn-danger delete-btn" data-id="${data}">
                            <i class="fas fa-trash"></i> Eliminar
                        </button>
                    `;
        },
        orderable: false,
      },
    ],
  });

  // Configurar modal de edición

  $(document).on("click", ".delete-btn", function () {
    var id = $(this).data("id");
    var url = `${urls.delete}/${id}`;

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
          url: url,
          type: "POST",
          headers: {
            RequestVerificationToken: $(
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
            console.error(xhr.responseText);
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
});
