
function validateFileSelected() {

    var file = document.getElementById("FileUpload");
    var errorMsg = document.getElementById("fileNotFoundError");

    if (file.files.length == 0) {
        errorMsg.innerHTML = "No file selected";
        return false;
    }
}