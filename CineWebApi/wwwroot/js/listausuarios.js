const errorBox = document.getElementById('errorUsuarios');
const CLAIM_ID = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';

const datosActuales = Sesion.usuarioActual();
const idActual = String(datosActuales?.sub ?? datosActuales?.[CLAIM_ID] ?? '');

function mostrarError(texto) {
    errorBox.textContent = texto;
    errorBox.classList.remove('d-none');
}

function cabeceras(conJson = false) {
    const h = { Authorization: 'Bearer ' + Sesion.token() };
    if (conJson) h['Content-Type'] = 'application/json';
    return h;
}

async function mensajeDeError(respuesta, porDefecto) {
    try {
        const datos = await respuesta.json();
        if (datos.mensaje) return datos.mensaje;
    } catch {  }
    return porDefecto;
}

function selectorEstado(cell) {
    const usuario = cell.getData();
    const select = document.createElement('select');
    select.className = 'form-select form-select-sm ' +
        (usuario.activo ? 'estado-activo' : 'estado-suspendido');
    select.setAttribute('aria-label', 'Estado de ' + usuario.nombreUsuario);

    for (const [valor, texto] of [['true', 'Activo'], ['false', 'Suspendido']]) {
        const opcion = document.createElement('option');
        opcion.value = valor;
        opcion.textContent = texto;
        select.appendChild(opcion);
    }
    select.value = String(usuario.activo);

    if (String(usuario.id) === idActual) {
        select.disabled = true;
        select.title = 'No podés cambiar tu propio estado';
        return select;
    }

    select.addEventListener('change', async () => {
        const nuevo = select.value === 'true';
        select.disabled = true;
        errorBox.classList.add('d-none');
        try {
            const r = await fetch(`/api/usuarios/${usuario.id}/estado`, {
                method: 'PUT',
                headers: cabeceras(true),
                body: JSON.stringify({ activo: nuevo }),
            });
            if (!r.ok) {
                select.value = String(!nuevo);
                mostrarError(await mensajeDeError(r, 'No se pudo cambiar el estado.'));
                return;
            }
            select.classList.toggle('estado-activo', nuevo);
            select.classList.toggle('estado-suspendido', !nuevo);
            cell.getRow().update({ activo: nuevo });
        } catch {
            select.value = String(!nuevo);
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
        placeholder: 'No hay usuarios para mostrar.',
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
                title: 'Usuario',
                field: 'nombreUsuario',
                headerFilter: 'input',
                headerFilterParams: { elementAttributes: { 'aria-label': 'Filtrar por usuario' } },
            },
            {
                title: 'Email',
                field: 'email',
                headerFilter: 'input',
                headerFilterParams: { elementAttributes: { 'aria-label': 'Filtrar por email' } },
            },
            {
                title: 'Rol',
                field: 'rol',
                width: 130,
                headerFilter: 'list',
                headerFilterParams: {
                    values: ['', 'Admin', 'Cliente'],
                    elementAttributes: { 'aria-label': 'Filtrar por rol' },
                },
            },
            {
                title: 'Estado',
                field: 'activo',
                width: 160,
                formatter: selectorEstado,
                headerFilter: 'list',
                headerFilterParams: {
                    values: { '': '', true: 'Activo', false: 'Suspendido' },
                    elementAttributes: { 'aria-label': 'Filtrar por estado' },
                },
                headerFilterFunc: (filtro, valor) => !filtro || String(valor) === String(filtro),
            },
            {
                title: 'Fecha de alta',
                field: 'fechaAlta',
                width: 150,
                formatter: (cell) => new Date(cell.getValue()).toLocaleDateString('es-AR'),
                sorter: (a, b) => new Date(a) - new Date(b),
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
                { field: 'nombreUsuario', type: 'like', value: valor },
                { field: 'email', type: 'like', value: valor },
            ]]);
        } else {
            table.clearFilter();
        }
    });
}

async function iniciar() {
    if (!Sesion.tieneRol(datosActuales, 'Admin')) {
        window.location.href = '/index.html';
        return;
    }
    try {
        const r = await fetch('/api/usuarios', { headers: cabeceras() });
        if (r.status === 401 || r.status === 403) {
            window.location.href = '/index.html';
            return;
        }
        if (!r.ok) {
            mostrarError(await mensajeDeError(r, 'No se pudo cargar la lista de usuarios.'));
            return;
        }
        crearTabla(await r.json());
    } catch {
        mostrarError('No se pudo conectar con el servidor.');
    }
}

iniciar();