const form = document.getElementById('registerForm');
const errorBox = document.getElementById('registerError');
const boton = form.querySelector('button[type="submit"]');
const nacimiento = document.getElementById('registerNacimiento');
nacimiento.max = new Date().toISOString().split('T')[0]; // no se puede nacer en el futuro

const valor = (id) => document.getElementById(id).value.trim();
function mostrarError(texto) {
errorBox.textContent = texto;
errorBox.classList.remove('d-none');
 }

async function mensajeDeError(respuesta) {
try {
   const datos = await respuesta.json();
   if (datos.mensaje) return datos.mensaje;
   if (datos.errors) return Object.values(datos.errors).flat().join(' ');
   } catch { /* la respuesta no era JSON */}
   return 'No se pudo completar el registro.';
}

form.addEventListener('submit', async (e) => {
     e.preventDefault();
    errorBox.classList.add('d-none');

    const usuario = valor('registerUser');
    const password = document.getElementById('registerPassword').value;
    const telefono = valor('registerTelefono');

    boton.disabled = true;
    try {
    const registro = await fetch('/api/auth/register', 
    {
        method: 'POST',
        headers: {'Content-Type': 'application/json' },
        body: JSON.stringify({
        nombreUsuario: usuario,
        email: valor('registerEmail'),
        password: password,
        nombre: valor('registerNombre'),
        apellido: valor('registerApellido'),
        dni: valor('registerDni').replace(/[.\s]/g, ''),
        fechaNacimiento: nacimiento.value,
        telefono: telefono === '' ? null : telefono,
    }),
});

    if (!registro.ok) {
        mostrarError(await mensajeDeError(registro));
    return;
      }

  const login = await fetch('/api/auth/login', {
      method: 'POST',
    headers: {'Content-Type': 'application/json' },
    body: JSON.stringify({nombreUsuario: usuario, password: password }),
    });
  if (login.ok) {
      const {token} = await login.json();
      Sesion.guardar(token);
      window.location.href = Sesion.destinoVolver();
  }
  else {
      window.location.href = 'login.html';
    }
  } catch {
      mostrarError('No se pudo conectar con el servidor.');
  } finally {
      boton.disabled = false;
    }
});

document.querySelectorAll('a[href="register.html"], a[href="login.html"]').forEach((a) => {
    a.href += window.location.search;
});