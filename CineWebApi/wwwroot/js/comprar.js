(async function () {
    // Página del paso siguiente (todavía no existe)
    const URL_SIGUIENTE = '/pago.html';
    const SEGUNDOS_TEMPORIZADOR = 10 * 60;

    const idFuncion = new URLSearchParams(window.location.search).get('funcion');

    const estado = document.getElementById('estadoCompra');
    const errorBox = document.getElementById('errorCompra');
    const contenido = document.getElementById('contenidoCompra');
    const aviso = document.getElementById('avisoFuncion');
    const inputCantidad = document.getElementById('cantidad');
    const btnMenos = document.getElementById('btnMenos');
    const btnMas = document.getElementById('btnMas');
    const btnContinuar = document.getElementById('btnContinuar');
    const temporizador = document.getElementById('temporizador');

    const formatoPrecio = (n) =>
        '$ ' + Number(n).toLocaleString('es-AR', { maximumFractionDigits: 2 });

    function fallo(texto) {
        estado.classList.add('d-none');
        errorBox.textContent = texto;
        errorBox.classList.remove('d-none');
    }

    if (!/^\d+$/.test(idFuncion ?? '')) {
        fallo('No se indicó una función válida.');
        return;
    }

    let funcion = null;
    let cantidad = 0;
    let comprable = true;
    let restante = SEGUNDOS_TEMPORIZADOR;

    // "2026-11-19T18:05:00" -> "19/11/2026 - 18:05HS"
    function fechaHora(valor) {
        const [dia, hora] = String(valor).split('T');
        const [a, m, d] = dia.split('-');
        return `${d}/${m}/${a} - ${hora.substring(0, 5)}HS`;
    }

    async function pedirFuncion() {
        const r = await fetch(`/api/funciones/${idFuncion}`);
        if (r.status === 404) throw new Error('No se encontró la función.');
        if (!r.ok) throw new Error('No se pudo cargar la función.');
        return r.json();
    }

    function actualizar() {
        const max = funcion.disponibles;
        inputCantidad.value = cantidad;
        document.getElementById('compraTotal').textContent = formatoPrecio(cantidad * funcion.precioBase);
        document.getElementById('compraDisponibles').textContent = max;
        btnMenos.disabled = !comprable || cantidad <= 0;
        btnMas.disabled = !comprable || cantidad >= max;
        btnContinuar.disabled = !comprable || cantidad === 0;
    }

    function pintarFuncion() {
        document.title = `Cine | Comprar ${funcion.titulo}`;
        document.getElementById('compraTitulo').textContent = funcion.titulo;
        document.getElementById('compraDuracion').textContent = `${funcion.duracionMinutos} min`;
        document.getElementById('compraClasificacion').textContent = funcion.clasificacion ?? '-';
        document.getElementById('compraGenero').textContent = funcion.genero ?? '-';
        document.getElementById('compraFecha').textContent = fechaHora(funcion.fechaHoraInicio);
        document.getElementById('compraSala').textContent = `SALA ${funcion.idSala}`;
        document.getElementById('compraPrecio').textContent = formatoPrecio(funcion.precioBase);
        document.getElementById('btnAtras').href = `/pelicula.html?id=${encodeURIComponent(funcion.idPelicula)}`;

        // Una función que ya empezó o que está agotada no se puede comprar
        const yaEmpezo = new Date(funcion.fechaHoraInicio) <= new Date();
        comprable = !yaEmpezo && funcion.disponibles > 0;
        if (yaEmpezo) {
            aviso.textContent = 'Esta función ya comenzó.';
        } else if (funcion.disponibles === 0) {
            aviso.textContent = `Función agotada: las ${funcion.capacidad} butacas de la sala están ocupadas.`;
        }
        aviso.classList.toggle('d-none', comprable);

        cantidad = Math.min(cantidad, funcion.disponibles);
        actualizar();
    }

    // ===== Temporizador: al terminar se reinician la cantidad y los datos =====
    function pintarTemporizador() {
        const m = Math.floor(restante / 60);
        const s = String(restante % 60).padStart(2, '0');
        temporizador.textContent = `${m}:${s}`;
    }

    setInterval(async () => {
        restante--;
        if (restante <= 0) {
            restante = SEGUNDOS_TEMPORIZADOR;
            cantidad = 0;
            try {
                funcion = await pedirFuncion();
                pintarFuncion();
            } catch (e) {
                console.warn(e);
                actualizar();
            }
        }
        pintarTemporizador();
    }, 1000);

    // ===== Botones =====
    btnMenos.addEventListener('click', () => {
        if (cantidad > 0) { cantidad--; actualizar(); }
    });

    btnMas.addEventListener('click', () => {
        if (cantidad < funcion.disponibles) { cantidad++; actualizar(); }
    });

    btnContinuar.addEventListener('click', () => {
        if (cantidad === 0) return;

        // Hace falta sesión para comprar: va al login y vuelve a esta misma página
        if (!Sesion.usuarioActual()) {
            const aqui = window.location.pathname + window.location.search;
            window.location.href = '/auth/login.html?volver=' + encodeURIComponent(aqui);
            return;
        }

        sessionStorage.setItem('cine_compra', JSON.stringify({ idFuncion: funcion.idFuncion, cantidad }));
        window.location.href = URL_SIGUIENTE;
    });

    // ===== Carga inicial =====
    try {
        funcion = await pedirFuncion();
    } catch (e) {
        fallo(e.message);
        return;
    }
    pintarFuncion();
    pintarTemporizador();
    estado.classList.add('d-none');
    contenido.hidden = false;
})();