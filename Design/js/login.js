document.getElementById("loginForm").addEventListener("submit", function(event) {
    event.preventDefault(); // Previene el comportamiento por defecto del formulario
    window.location.href = "inicio.html"; // Redirige a la página de inicio
});
