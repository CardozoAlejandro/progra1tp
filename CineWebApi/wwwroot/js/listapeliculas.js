const errorBox = document.getElementById('errorPeliculas');

const EN_CARTELERA = 'En cartelera';
const FUERA_CARTELERA = 'Fuera de cartelera';
const ESTADOS = [EN_CARTELERA, FUERA_CARTELERA];

const datosActuales = Sesion.usuarioActual();

function mostrarError(texto) {
    errorBox.textContent = texto;
    errorBox.classList.remove('d-none');
}

function cabeceras() {
    return { Authorization: 'Bearer ' + Sesion.token() };
}

async function mensajeDeError(respuesta, porDefecto) {
    try {
        const datos = await respuesta.json();
        if (datos.mensaje) return datos.mensaje;
    } catch {  }
    return porDefecto;
}

function formatearFecha(valor) {
    if (!valor) return '-';
    const [anio, mes, dia] = String(valor).substring(0, 10).split('-');
    return `${dia}/${mes}/${anio}`;
}

function normalizar(p) {
    return {
        id: p.id,
        titulo: p.titulo,
        genero: p.genero?.nombreGenero ?? '-',
        clasificacion: p.clasificacion?.codigo ?? '-',
        duracionMinutos: p.duracionMinutos,
        fechaEstreno: p.fechaEstreno ?? '',
        estado: p.estado,
        sinopsis: p.sinopsis ?? '',
        idGenero: p.idGenero ?? p.genero?.id ?? null,
        idClasificacion: p.idClasificacion ?? p.clasificacion?.id ?? null,
    };
}

function aplicarClaseEstado(select, estado) {
    const enCartelera = estado === EN_CARTELERA;
    select.classList.toggle('estado-activo', enCartelera);
    select.classList.toggle('estado-suspendido', !enCartelera);
}

function selectorEstado(cell) {
    const pelicula = cell.getData();
    let estadoActual = pelicula.estado;

    const select = document.createElement('select');
    select.className = 'form-select form-select-sm';
    select.setAttribute('aria-label', 'Estado de ' + pelicula.titulo);

    const opciones = ESTADOS.includes(estadoActual) ? ESTADOS : [...ESTADOS, estadoActual];
    for (const texto of opciones) {
        const opcion = document.createElement('option');
        opcion.value = texto;
        opcion.textContent = texto;
        select.appendChild(opcion);
    }
    select.value = estadoActual;
    aplicarClaseEstado(select, estadoActual);

    select.addEventListener('change', async () => {
        const nuevo = select.value;
        select.disabled = true;
        errorBox.classList.add('d-none');
        try {
            const r = await fetch(
                `/api/peliculas/${pelicula.id}/estado?estado=${encodeURIComponent(nuevo)}`,
                { method: 'PUT', headers: cabeceras() }
            );
            if (!r.ok) {
                select.value = estadoActual;
                mostrarError(await mensajeDeError(r, 'No se pudo cambiar el estado.'));
                return;
            }
            estadoActual = nuevo;
            aplicarClaseEstado(select, nuevo);
            cell.getRow().update({ estado: nuevo });
        } catch {
            select.value = estadoActual;
            mostrarError('No se pudo conectar con el servidor.');
        } finally {
            select.disabled = false;
        }
    });
    return select;
}

function crearTabla(datos) {
    const table = new Tabulator('#users-table', {
        data: datos,
        layout: 'fitColumns',
        pagination: true,
        paginationElement: document.getElementById('table-pagination'),
        paginationSize: 10,
        paginationSizeSelector: [10, 25, 50, 100],
        movableColumns: true,
        placeholder: 'No hay películas para mostrar.',
        locale: 'es',
        langs: {
            es: {
                pagination: {
                    first: 'Primero', first_title: 'Primera página',
                    last: 'Último', last_title: 'Última página',
                    prev: 'Anterior', prev_title: 'Página anterior',
                    next: 'Siguiente', next_title: 'Página siguiente',
                    page_size: 'Filas por página',
                    counter: { showing: 'Mostrando', of: 'de', rows: 'filas', pages: 'páginas' },
                },
                headerFilters: { default: 'Filtrar...' },
            },
        },
        columns: [
            { title: '#', field: 'id', width: 70, headerSort: true },
            {
                title: 'Título',
                field: 'titulo',
                minWidth: 200,
                headerFilter: 'input',
                headerFilterParams: { elementAttributes: { 'aria-label': 'Filtrar por título' } },
            },
            {
                title: 'Género',
                field: 'genero',
                width: 150,
                headerFilter: 'list',
                headerFilterParams: {
                    valuesLookup: true,
                    clearable: true,
                    elementAttributes: { 'aria-label': 'Filtrar por género' },
                },
            },
            {
                title: 'Clasif.',
                field: 'clasificacion',
                width: 110,
                headerFilter: 'list',
                headerFilterParams: {
                    valuesLookup: true,
                    clearable: true,
                    elementAttributes: { 'aria-label': 'Filtrar por clasificación' },
                },
            },
            {
                title: 'Duración',
                field: 'duracionMinutos',
                width: 110,
                formatter: (cell) => `${cell.getValue()} min`,
                sorter: 'number',
            },
            {
                title: 'Estreno',
                field: 'fechaEstreno',
                width: 130,
                formatter: (cell) => formatearFecha(cell.getValue()),
            },
            {
                title: 'Estado',
                field: 'estado',
                width: 200,
                formatter: selectorEstado,
                headerFilter: 'list',
                headerFilterParams: {
                    values: { '': '', [EN_CARTELERA]: EN_CARTELERA, [FUERA_CARTELERA]: FUERA_CARTELERA },
                    elementAttributes: { 'aria-label': 'Filtrar por estado' },
                },
            },
            {
                title: 'Acciones',
                field: 'acciones',
                width: 90,
                hozAlign: 'center',
                headerSort: false,
                formatter: (cell) => {
                    const boton = document.createElement('button');
                    boton.type = 'button';
                    boton.className = 'btn btn-primary btn-sm';
                    boton.style.width = '32px';
                    boton.style.height = '32px';
                    boton.style.padding = '0';
                    boton.title = 'Editar película';
                    boton.setAttribute('aria-label', 'Editar ' + cell.getData().titulo);

                    const icono = document.createElement('i');
                    icono.className = 'bi bi-pencil-fill';

                    boton.appendChild(icono);

                    boton.addEventListener('click', () => {
                        editarPelicula(cell.getData());
                    });

                    return boton;
                },
            },
        ],
    });

    table.on('tableBuilt', () => {
        const grid = document.getElementById('users-table');
        grid.removeAttribute('aria-owns');
        grid.querySelector('.tabulator-tableholder').setAttribute('role', 'rowgroup');
        grid.querySelector('.tabulator-table').setAttribute('role', 'presentation');
    });

    table.on('renderComplete', () => {
        document.querySelectorAll('#table-pagination button').forEach((button) => {
            button.classList.add('btn', 'btn-sm', 'btn-outline-secondary');
        });
    });

    return table;
}

function editarPelicula(pelicula) {
    const modalEl = document.getElementById('nuevaPeliculaModal');
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);

    document.getElementById('nuevaPeliculaTitulo').textContent = 'Editar película';
    document.getElementById('btnGuardarPelicula').textContent = 'Guardar cambios';

    document.getElementById('formNuevaPelicula').dataset.id = pelicula.id;

    document.getElementById('npTitulo').value = pelicula.titulo;
    document.getElementById('npDuracion').value = pelicula.duracionMinutos;
    document.getElementById('npEstreno').value = pelicula.fechaEstreno
        ? pelicula.fechaEstreno.substring(0, 10)
        : '';

    document.getElementById('npSinopsis').value = pelicula.sinopsis ?? '';

    modal.show();
}

let tabla;

async function cargarPeliculas() {
    const r = await fetch('/api/peliculas', { headers: cabeceras() });
    if (r.status === 401 || r.status === 403) {
        window.location.href = '/index.html';
        return;
    }
    if (!r.ok) {
        mostrarError(await mensajeDeError(r, 'No se pudo cargar la lista de películas.'));
        return;
    }
    const datos = (await r.json()).map(normalizar);
    if (tabla) tabla.setData(datos);
    else tabla = crearTabla(datos);
}

async function iniciar() {
    if (!Sesion.tieneRol(datosActuales, 'Admin')) {
        window.location.href = '/index.html';
        return;
    }
    try {
        await cargarPeliculas();
    } catch {
        mostrarError('No se pudo conectar con el servidor.');
    }
}

iniciar();

(() => {
    const modalEl = document.getElementById('nuevaPeliculaModal');
    const form = document.getElementById('formNuevaPelicula');
    const errorModal = document.getElementById('errorNuevaPelicula');
    const btnGuardar = document.getElementById('btnGuardarPelicula');
    const selGenero = document.getElementById('npGenero');
    const selClasif = document.getElementById('npClasificacion');
    const selEstado = document.getElementById('npEstado');

    for (const e of ESTADOS) {
        const o = document.createElement('option');
        o.value = e;
        o.textContent = e;
        selEstado.appendChild(o);
    }

    function llenarSelect(select, items, texto) {
        select.innerHTML = '<option value="">Sin especificar</option>';
        for (const it of items) {
            const o = document.createElement('option');
            o.value = it.id;
            o.textContent = it[texto];
            select.appendChild(o);
        }
    }

    function mostrarErrorModal(texto) {
        errorModal.textContent = texto;
        errorModal.classList.remove('d-none');
    }

    async function cargarCatalogos() {
        try {
            const [rg, rc] = await Promise.all([
                fetch('/api/catalogo/generos', { headers: cabeceras() }),
                fetch('/api/catalogo/clasificaciones', { headers: cabeceras() }),
            ]);
            if (!rg.ok || !rc.ok) throw new Error();
            llenarSelect(selGenero, await rg.json(), 'nombreGenero');
            llenarSelect(selClasif, await rc.json(), 'codigo');
        } catch {
            mostrarErrorModal('No se pudieron cargar géneros y clasificaciones.');
        }
    }

    modalEl.addEventListener('show.bs.modal', async () => {
        form.classList.remove('was-validated');
        errorModal.classList.add('d-none');

        await cargarCatalogos();

        const id = form.dataset.id;

        if (id) {
            const pelicula = tabla.getData().find(p => p.id === Number(id));

            if (pelicula) {
                selGenero.value = pelicula.idGenero ?? '';
                selClasif.value = pelicula.idClasificacion ?? '';
                selEstado.value = pelicula.estado;
            }
        }
    });
    modalEl.addEventListener('hidden.bs.modal', () => {
        form.reset();
        delete form.dataset.id;

        document.getElementById('nuevaPeliculaTitulo').textContent = 'Nueva película';
        btnGuardar.textContent = 'Guardar';

        form.classList.remove('was-validated');
        errorModal.classList.add('d-none');
    });

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        errorModal.classList.add('d-none');
        form.classList.add('was-validated');

        if (!form.checkValidity()) return;

        const pelicula = {
            titulo: document.getElementById('npTitulo').value.trim(),
            duracionMinutos: Number(document.getElementById('npDuracion').value),
            sinopsis: document.getElementById('npSinopsis').value.trim() || null,
            fechaEstreno: document.getElementById('npEstreno').value || null,
            idGenero: selGenero.value ? Number(selGenero.value) : null,
            idClasificacion: selClasif.value ? Number(selClasif.value) : null,
            estado: selEstado.value,
        };

        const id = form.dataset.id;
        const editando = Boolean(id);

        btnGuardar.disabled = true;

        try {
            const url = editando
                ? `/api/peliculas/actualizarpelicula?id=${id}`
                : '/api/peliculas';

            const r = await fetch(url, {
                method: editando ? 'PUT' : 'POST',
                headers: {
                    ...cabeceras(),
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(pelicula),
            });

            if (!r.ok) {
                mostrarErrorModal(
                    await mensajeDeError(
                        r,
                        editando
                            ? 'No se pudo actualizar la película.'
                            : 'No se pudo crear la película.'
                    )
                );
                return;
            }

            bootstrap.Modal.getInstance(modalEl).hide();

            await cargarPeliculas();

        } catch {
            mostrarErrorModal('No se pudo conectar con el servidor.');
        } finally {
            btnGuardar.disabled = false;
        }
    });
})();