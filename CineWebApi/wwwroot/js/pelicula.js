(async function () {
    const params = new URLSearchParams(window.location.search);
    const id = params.get('id');

    const estado = document.getElementById('estadoPelicula');
    const errorBox = document.getElementById('errorPelicula');
    const contenido = document.getElementById('contenidoPelicula');
    const selectorDias = document.getElementById('selectorDias');
    const horariosDia = document.getElementById('horariosDia');
    const sinFunciones = document.getElementById('sinFunciones');

    function fallo(texto) {
        estado.classList.add('d-none');
        errorBox.textContent = texto;
        errorBox.classList.remove('d-none');
    }

    function crear(etiqueta, clase, texto) {
        const nodo = document.createElement(etiqueta);
        if (clase) nodo.className = clase;
        if (texto !== undefined) nodo.textContent = texto;
        return nodo;
    }

    const capitalizar = (t) => t.charAt(0).toUpperCase() + t.slice(1);

    // "2026-11-19" -> Date local (sin corrimientos por zona horaria)
    function fechaLocal(iso) {
        const [a, m, d] = iso.split('-').map(Number);
        return new Date(a, m - 1, d);
    }

    function formatearFecha(valor) {
        if (!valor) return '-';
        const [anio, mes, dia] = valor.substring(0, 10).split('-');
        return `${dia}/${mes}/${anio}`;
    }

    // "2026-11-19T15:40:00" -> "15:40"
    const hora = (fechaHora) => String(fechaHora).substring(11, 16);

    if (!/^\d+$/.test(id ?? '')) {
        fallo('No se indicó una película válida.');
        return;
    }

    let pelicula;
    let funciones = [];
    try {
        const [rPelicula, rFunciones] = await Promise.all([
            fetch(`/api/peliculas/${id}/detalle`),
            fetch(`/api/funciones/pelicula/${id}`),
        ]);

        if (rPelicula.status === 404) {
            fallo('No se encontró la película.');
            return;
        }
        if (!rPelicula.ok) throw new Error(`Error ${rPelicula.status}`);
        pelicula = await rPelicula.json();

        if (rFunciones.ok) funciones = await rFunciones.json();
        else console.warn('No se pudieron cargar las funciones. Estado:', rFunciones.status);
    } catch (e) {
        console.error(e);
        fallo('No se pudo cargar la película. Intentá de nuevo más tarde.');
        return;
    }

    // ===== Datos de la película =====
    document.title = `Cine | ${pelicula.titulo}`;
    document.getElementById('tituloPagina').textContent = pelicula.titulo;
    document.getElementById('peliTitulo').textContent = pelicula.titulo;
    document.getElementById('peliGenero').textContent = pelicula.genero ?? '-';
    document.getElementById('peliClasificacion').textContent = pelicula.clasificacion ?? '-';
    document.getElementById('peliDuracion').textContent = `${pelicula.duracionMinutos} min`;
    document.getElementById('peliEstreno').textContent = formatearFecha(pelicula.fechaEstreno);

    const sinopsis = (pelicula.sinopsis ?? '').trim();
    document.getElementById('peliSinopsis').textContent =
        sinopsis && sinopsis.toUpperCase() !== 'NULL' ? sinopsis : 'Sin sinopsis disponible.';

    // ===== Días y horarios =====
    const porDia = new Map();
    for (const f of funciones) {
        const dia = String(f.fechaHoraInicio).substring(0, 10);
        if (!porDia.has(dia)) porDia.set(dia, []);
        porDia.get(dia).push(f);
    }
    const dias = [...porDia.keys()].sort();

    function mostrarHorarios(dia) {
        horariosDia.replaceChildren();
        const lista = porDia.get(dia).sort((a, b) => a.fechaHoraInicio.localeCompare(b.fechaHoraInicio));
        for (const f of lista) {
            horariosDia.appendChild(Horarios.crearBoton(pelicula.titulo, f));
        }
    }

    function seleccionarDia(dia) {
        for (const b of selectorDias.children) {
            const activo = b.dataset.dia === dia;
            b.classList.toggle('active', activo);
            b.setAttribute('aria-pressed', String(activo));
        }
        mostrarHorarios(dia);
    }

    if (dias.length === 0) {
        selectorDias.classList.add('d-none');
        sinFunciones.classList.remove('d-none');
    } else {
        for (const dia of dias) {
            const f = fechaLocal(dia);
            const boton = crear('button', 'btn btn-outline-primary text-center flex-shrink-0');
            boton.type = 'button';
            boton.style.minWidth = '6.5rem';
            boton.dataset.dia = dia;
            boton.append(
                crear('span', 'd-block small', capitalizar(f.toLocaleDateString('es-AR', { weekday: 'long' }))),
                crear('span', 'd-block fs-4 lh-1 my-1', String(f.getDate())),
                crear('span', 'd-block small', capitalizar(f.toLocaleDateString('es-AR', { month: 'long' })))
            );
            boton.addEventListener('click', () => seleccionarDia(dia));
            selectorDias.appendChild(boton);
        }
        seleccionarDia(dias[0]);
    }

    estado.classList.add('d-none');
    contenido.hidden = false;
})();