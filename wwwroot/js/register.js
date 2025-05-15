$(document).ready(function () {
  $("#docType").change(function () {
    var maxLength = $(this).val() === "DNI" ? 8 : 20;
    $("#maxLength").text(maxLength);
    $("#docNumber").attr("maxlength", maxLength);

    $("#docNumber").val("");
    $("#Input_NumDoc-error").text("");
  });
  $("#docNumber").attr("maxlength", 8);

  $("#docNumber").on("input", function () {
    var value = $(this).val();
    var docType = $("#docType").val();

    $(this).val(value.replace(/[^0-9]/g, ""));

    if (docType === "DNI" && value.length > 8) {
      $(this).val(value.substring(0, 8));
      $("#Input_NumDoc-error").text("El DNI debe tener máximo 8 dígitos");
    } else if (docType === "RUC" && value.length > 20) {
      $(this).val(value.substring(0, 20));
      $("#Input_NumDoc-error").text("El RUC debe tener máximo 20 dígitos");
    } else {
      $("#Input_NumDoc-error").text("");
    }
  });

  $("#registerForm").submit(function (e) {
    var docType = $("#docType").val();
    var docNumber = $("#docNumber").val();

    if (docType === "DNI" && docNumber.length !== 8) {
      $("#Input_NumDoc-error").text("El DNI debe tener exactamente 8 dígitos");
      e.preventDefault();
      return false;
    }

    if (docType === "RUC" && (docNumber.length < 11 || docNumber.length > 20)) {
      $("#Input_NumDoc-error").text("El RUC debe tener entre 11 y 20 dígitos");
      e.preventDefault();
      return false;
    }

    return true;
  });
});
