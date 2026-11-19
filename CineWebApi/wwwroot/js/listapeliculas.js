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
    } catch { /* la respuesta no era JSON */ }
    return porDefecto;
}

function formatearFecha(valor) {
    if (!valor) return '-';
    const [anio, mes, dia] = String(valor).substring(0, 10).split('-');
    return `${dia}/${mes}/${anio}`;
}

// La API devuelve la entidad Pelicula (con Genero y Clasificacion anidados);
// la aplanamos para que Tabulator pueda filtrar y ordenar por cada columna.
function normalizar(p) {
    return {
        id: p.id,
        titulo: p.titulo,
        genero: p.genero?.nombreGenero ?? '-',
        clasificacion: p.clasificacion?.codigo ?? '-',
        duracionMinutos: p.duracionMinutos,
        fechaEstreno: p.fechaEstreno ?? '',
        estado: p.estado,
    };
}

function aplicarClaseEstado(select, estado) {
    const enCartelera = estado === EN_CARTELERA;
    select.classList.toggle('estado-activo', enCartelera);
    select.classList.toggle('estado-suspendido', !enCartelera);
}

// Columna "Estado": un <select> En cartelera / Fuera de cartelera
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
            // El endpoint recibe el estado por query string: PUT /api/peliculas/{id}/estado?estado=...
            const r = await fetch(
                `/api/peliculas/${pelicula.id}/estado?estado=${encodeURIComponent(nuevo)}`,
                { method: 'PUT', headers: cabeceras() }
            );
            if (!r.ok) {
                select.value = estadoActual; // vuelve al valor anterior
                mostrarError(await mensajeDeError(r, 'No se pudo cambiar el estado.'));
                return;
            }
            estadoActual = nuevo;
            aplicarClaseEstado(select, nuevo);
            cell.getRow().update({ estado: nuevo }); // refresca la fila
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

    document.getElementById('table-filter').addEventListener('input', (e) => {
        const valor = e.target.value;
        if (valor) {
            table.setFilter([[
                { field: 'titulo', type: 'like', value: valor },
                { field: 'genero', type: 'like', value: valor },
            ]]);
        } else {
            table.clearFilter();
        }
    });
}

async function iniciar() {
    // Solo Admin: si no lo es, vuelve al inicio
    if (!Sesion.tieneRol(datosActuales, 'Admin')) {
        window.location.href = '/index.html';
        return;
    }
    try {
        const r = await fetch('/api/peliculas', { headers: cabeceras() });
        if (r.status === 401 || r.status === 403) {
            window.location.href = '/index.html';
            return;
        }
        if (!r.ok) {
            mostrarError(await mensajeDeError(r, 'No se pudo cargar la lista de películas.'));
            return;
        }
        const peliculas = await r.json();
        crearTabla(peliculas.map(normalizar));
    } catch {
        mostrarError('No se pudo conectar con el servidor.');
    }
}

iniciar();