(async function () {
    const SEGUNDOS_TEMPORIZADOR = 10 * 60;
    const modalEl = document.getElementById('modalConfirmar');
    const modal = new bootstrap.Modal(modalEl);
    const modalError = document.getElementById('modalError');
    const btnConfirmar = document.getElementById('btnConfirmarCompra');
    const selectPago = document.getElementById('formaPago');

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

    async function cargarFormasPago() {
        const r = await fetch('/api/formas-pago');
        if (!r.ok) throw new Error('No se pudieron cargar las formas de pago.');
        const lista = await r.json();

        selectPago.replaceChildren(new Option('Seleccionar...', ''));
        lista.forEach(f => selectPago.add(new Option(f.nombre, f.id)));
    }

    selectPago.addEventListener('change', () => { if (funcion) actualizar(); });
    function actualizar() {
        const max = funcion.disponibles;
        inputCantidad.value = cantidad;
        document.getElementById('compraTotal').textContent = formatoPrecio(cantidad * funcion.precioBase);
        document.getElementById('compraDisponibles').textContent = max;
        btnMenos.disabled = !comprable || cantidad <= 0;
        btnMas.disabled = !comprable || cantidad >= max;
        btnContinuar.disabled = !comprable || cantidad === 0 || !selectPago.value;
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
            selectPago.value = '';
            modal.hide();
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

    btnMenos.addEventListener('click', () => {
        if (cantidad > 0) { cantidad--; actualizar(); }
    });

    btnMas.addEventListener('click', () => {
        if (cantidad < funcion.disponibles) { cantidad++; actualizar(); }
    });

    btnContinuar.addEventListener('click', () => {
        if (cantidad === 0) return;

        if (!Sesion.usuarioActual()) {
            const aqui = window.location.pathname + window.location.search;
            window.location.href = '/auth/login.html?volver=' + encodeURIComponent(aqui);
            return;
        }

        document.getElementById('modalPelicula').textContent = funcion.titulo;
        document.getElementById('modalFecha').textContent = fechaHora(funcion.fechaHoraInicio);
        document.getElementById('modalSala').textContent = `SALA ${funcion.idSala}`;
        document.getElementById('modalCantidad').textContent = cantidad;
        document.getElementById('modalTotal').textContent = formatoPrecio(cantidad * funcion.precioBase);
        modalError.classList.add('d-none');
        btnConfirmar.disabled = false;
        document.getElementById('modalFormaPago').textContent =
            selectPago.selectedOptions[0].textContent;
        modal.show();
    });

    btnConfirmar.addEventListener('click', async () => {
        btnConfirmar.disabled = true;
        modalError.classList.add('d-none');

        try {
            const token = Sesion.token();
            const r = await fetch('/api/comprobantes', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...(token ? { 'Authorization': `Bearer ${token}` } : {})
                },
                body: JSON.stringify({
                    idFuncion: funcion.idFuncion,
                    cantidad,
                    idFormaPago: Number(selectPago.value)
                })
            });

            if (r.status === 401) {
                window.location.href = '/auth/login.html?volver=' +
                    encodeURIComponent(window.location.pathname + window.location.search);
                return;
            }

            const data = await r.json().catch(() => ({}));
            if (!r.ok) throw new Error(data.mensaje || 'No se pudo registrar la compra.');

            sessionStorage.setItem('cine_compra', JSON.stringify({
                idComprobante: data.idComprobante,
                idFuncion: funcion.idFuncion,
                cantidad
            }));
            window.location.href = '/mi-cuenta.html?compra=' + data.idComprobante;
        } catch (e) {
            btnConfirmar.disabled = false;
            try { funcion = await pedirFuncion(); pintarFuncion(); } catch { }
            modal.hide();
            aviso.textContent = e.message;
            aviso.classList.remove('d-none');
        }
    });

    try {
        funcion = await pedirFuncion();
        await cargarFormasPago();
    } catch (e) {
        fallo(e.message);
        return;
    }
    pintarFuncion();
    pintarTemporizador();
    estado.classList.add('d-none');
    contenido.hidden = false;
})();

