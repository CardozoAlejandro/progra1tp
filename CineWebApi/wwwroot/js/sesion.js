(function () {
    const CLAVE = 'cine_token';
    const CLAIM_ROL = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    function leer() {
        try { return localStorage.getItem(CLAVE); } catch { return null; }
    }

    function decodificar(token) {
        try {
            const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
            const json = decodeURIComponent(
                atob(base64).split('').map(c => '%' + c.charCodeAt(0).toString(16).padStart(2, '0')).join('')
            );
            return JSON.parse(json);
        } catch { return null; }
    }

    function usuarioActual() {
        const token = leer();
        if (!token) return null;
        const datos = decodificar(token);
        if (!datos || (datos.exp && datos.exp * 1000 < Date.now())) {
            cerrar(false);
            return null;
        }
        return datos;
    }

    function guardar(token) {
        try { localStorage.setItem(CLAVE, token); } catch { }
    }

    function cerrar(recargar = true) {
        try { localStorage.removeItem(CLAVE); } catch { }
        if (recargar) window.location.reload();
    }

    function destinoVolver() {
        const volver = new URLSearchParams(window.location.search).get('volver');
        if (volver && volver.startsWith('/') && !volver.startsWith('//')) return volver;
        return '/index.html';
    }

    function enlazarVolver() {
        const botones = document.getElementById('authButtons');
        if (!botones) return;
        const actual = window.location.pathname + window.location.search;
        botones.querySelectorAll('a').forEach((a) => {
            a.href = a.getAttribute('href') + '?volver=' + encodeURIComponent(actual);
        });
    }

    function nombreDe(datos) {
        return datos.unique_name
            ?? datos.name
            ?? datos.nombreUsuario
            ?? datos['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']
            ?? datos.sub
            ?? 'Mi cuenta';
    }

    function tieneRol(datos, rol) {
        if (!datos) return false;
        const roles = datos.role ?? datos[CLAIM_ROL] ?? [];
        return [].concat(roles).includes(rol);
    }

    function protegerEnlaces() {
        if (usuarioActual()) return;
        document.querySelectorAll('[data-requiere-sesion]').forEach((a) => {
            const destino = new URL(a.getAttribute('href'), window.location.href);
            a.href = '/auth/login.html?volver=' + encodeURIComponent(destino.pathname + destino.search);
        });
    }

    function pintarAdmin() {
        const esAdmin = tieneRol(usuarioActual(), 'Admin');
        document.querySelectorAll('[data-solo-admin]').forEach((el) => {
            el.classList.toggle('d-none', !esAdmin);
        });
    }

    function pintarNavbar() {
        enlazarVolver();
        pintarAdmin();
        protegerEnlaces();
        const botones = document.getElementById('authButtons');
        const perfil = document.getElementById('menuPerfil');
        if (!botones || !perfil) return;

        const datos = usuarioActual();
        botones.classList.toggle('d-none', datos !== null);
        perfil.classList.toggle('d-none', datos === null);

        if (datos) {
            const nombre = String(nombreDe(datos));
            document.getElementById('nombrePerfil').textContent = nombre;
            document.getElementById('avatarPerfil').textContent = nombre.charAt(0).toUpperCase();
            document.getElementById('cerrarSesion').addEventListener('click', () => cerrar());
        }
    }

    window.Sesion = { guardar, cerrar, usuarioActual, destinoVolver, tieneRol, token: leer };
    document.addEventListener('DOMContentLoaded', pintarNavbar);
})();

