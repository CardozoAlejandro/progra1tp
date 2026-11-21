const form = document.getElementById('loginForm');
const errorBox = document.getElementById('loginError');
const boton = form.querySelector('button[type="submit"]');

function mostrarError(texto) {
    errorBox.textContent = texto;
    errorBox.classList.remove('d-none');
}

async function mensajeDeError(respuesta) {
    try {
        const datos = await respuesta.json();
        if (datos.mensaje) return datos.mensaje;
        if (datos.errors) return Object.values(datos.errors).flat().join(' ');
    }
    catch { }
    return 'No se pudo iniciar sesión.';
}

form.addEventListener('submit', async (e) => {
    e.preventDefault();
    errorBox.classList.add('d-none');

    const usuario = document.getElementById('loginUser').value.trim();
    const password = document.getElementById('loginPassword').value;

    boton.disabled = true;
    try {
        const respuesta = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ nombreUsuario: usuario, password: password }),
        });

        if (!respuesta.ok) {
            mostrarError(await mensajeDeError(respuesta));
            return;
        }

        const { token } = await respuesta.json();
        Sesion.guardar(token);
        window.location.href = Sesion.destinoVolver();
    } catch {
        mostrarError('No se pudo conectar con el servidor.');
    } finally {
        boton.disabled = false;
    }
});

document.querySelectorAll('a[href="register.html"], a[href="login.html"]').forEach((a) => {
    a.href += window.location.search;
});