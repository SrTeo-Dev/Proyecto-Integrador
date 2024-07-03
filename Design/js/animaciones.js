function mostrarOpciones() {
    var btnCrearCuenta = document.querySelector('.btn-crear');
    var opciones = document.getElementById("opciones");

    // Ocultamos el botón "Crear Cuenta"
    btnCrearCuenta.style.display = 'none';
    // Mostramos el contenedor de opciones
    opciones.style.display = 'flex';
}
