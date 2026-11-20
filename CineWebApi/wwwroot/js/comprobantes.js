(async function () {
    const ESTADO_CONFIRMADA = 'Confirmada';
    const ESTADO_PENDIENTE = 'Pendiente';
    const ESTADO_CANCELADO = 'Cancelado';
    const ESTADOS = [ESTADO_CONFIRMADA, ESTADO_PENDIENTE, ESTADO_CANCELADO];

    const mensajeBox = document.getElementById('mensajeComprobantes');
    let tabla = null;
    const formasPagoMap = new Map();

    // Restringido a rol Admin en el panel de administración (igual que listapeliculas.js y estadisticas.js)
    const datosActuales = Sesion.usuarioActual();
    if (!Sesion.tieneRol(datosActuales, 'Admin')) {
        window.location.href = '/index.html';
        return;
    }

    function manejarSinPermiso(status) {
        if (status === 401) {
            Sesion.cerrar(false);
            window.location.href = '/auth/login.html?volver=' + encodeURIComponent('/comprobantes.html');
            return true;
        }
        if (status === 403) {
            window.location.href = '/index.html';
            return true;
        }
        return false;
    }

    function cabeceras(conJson = false) {
        const h = { Authorization: 'Bearer ' + Sesion.token() };
        if (conJson) h['Content-Type'] = 'application/json';
        return h;
    }

    function mostrarMensaje(tipo, texto) {
        mensajeBox.className = 'alert alert-' + tipo;
        mensajeBox.textContent = texto;
        mensajeBox.classList.remove('d-none');
    }

    function ocultarMensaje() {
        mensajeBox.classList.add('d-none');
    }

    async function mensajeDeError(respuesta, porDefecto) {
        try {
            const datos = await respuesta.json();
            if (datos.mensaje) return datos.mensaje;
            if (datos.errors) return Object.values(datos.errors).flat().join(' ');
        } catch { /* la respuesta no era JSON */ }
        return porDefecto;
    }

    const formatoPrecio = (n) =>
        '$ ' + Number(n ?? 0).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    function formatearFechaHora(valor) {
        if (!valor) return '-';
        const s = String(valor);
        const [fecha, hora] = s.split('T');
        if (!fecha) return '-';
        const [anio, mes, dia] = fecha.split('-');
        return hora ? `${dia}/${mes}/${anio} ${hora.substring(0, 5)}` : `${dia}/${mes}/${anio}`;
    }

    function nombreFormaPago(id) {
        if (id == null) return '-';
        return formasPagoMap.get(Number(id)) ?? `#${id}`;
    }

    function crearBadgeEstado(estado) {
        const span = document.createElement('span');
        const clase = estado === ESTADO_CONFIRMADA ? 'text-bg-success'
            : estado === ESTADO_PENDIENTE ? 'text-bg-warning'
                : estado === ESTADO_CANCELADO ? 'text-bg-danger'
                    : 'text-bg-secondary';
        span.className = `badge ${clase}`;
        span.textContent = estado ?? '-';
        return span;
    }

    function aplicarClaseEstado(select, estado) {
        select.classList.toggle('estado-activo', estado === ESTADO_CONFIRMADA);
        select.classList.toggle('estado-suspendido', estado === ESTADO_CANCELADO);
    }

    function normalizar(c) {
        const detalles = Array.isArray(c.comprobanteDetalles) ? c.comprobanteDetalles : [];
        return {
            id: c.id,
            idCliente: c.idCliente != null ? String(c.idCliente) : '-',
            fechaHoraCreacion: c.fechaHoraCreacion ?? '',
            formaPagoNombre: c.formaPago?.nombre ?? nombreFormaPago(c.idFormaPago),
            cantidadDetalles: detalles.length,
            montoTotal: Number(c.montoTotal ?? 0),
            montoFinal: Number(c.montoFinal ?? 0),
            estado: c.estado ?? '',
        };
    }

    // ===== Modificación de estado (PUT /api/Comprobantes/{id}/estado) =====
    function selectorEstado(cell) {
        const comp = cell.getData();
        let estadoActual = comp.estado;

        const select = document.createElement('select');
        select.className = 'form-select form-select-sm';
        select.setAttribute('aria-label', 'Estado del comprobante #' + comp.id);

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
            ocultarMensaje();
            try {
                const r = await fetch(
                    `/api/Comprobantes/${comp.id}/estado?estado=${encodeURIComponent(nuevo)}`,
                    { method: 'PUT', headers: cabeceras() }
                );
                if (manejarSinPermiso(r.status)) return;
                if (!r.ok) {
                    select.value = estadoActual;
                    mostrarMensaje('danger', await mensajeDeError(r, 'No se pudo actualizar el estado.'));
                    return;
                }
                estadoActual = nuevo;
                aplicarClaseEstado(select, nuevo);
                cell.getRow().update({ estado: nuevo });
                mostrarMensaje('success', `Estado del comprobante #${comp.id} actualizado a "${nuevo}".`);
            } catch {
                select.value = estadoActual;
                mostrarMensaje('danger', 'No se pudo conectar con el servidor.');
            } finally {
                select.disabled = false;
            }
        });

        return select;
    }

    // ===== Ver detalle (GET /api/Comprobantes/{id}) =====
    const detalleModalEl = document.getElementById('detalleComprobanteModal');
    const detalleModal = new bootstrap.Modal(detalleModalEl);
    const errorDetalle = document.getElementById('errorDetalleComprobante');
    const cargandoDetalle = document.getElementById('cargandoDetalle');
    const contenidoDetalle = document.getElementById('contenidoDetalle');
    const detTablaBody = document.getElementById('detTablaBody');

    async function verDetalle(id) {
        document.getElementById('detalleComprobanteTitulo').textContent = `Detalle del comprobante #${id}`;
        errorDetalle.classList.add('d-none');
        cargandoDetalle.classList.remove('d-none');
        contenidoDetalle.hidden = true;
        detTablaBody.replaceChildren();
        detalleModal.show();

        try {
            const r = await fetch(`/api/Comprobantes/${id}`, { headers: cabeceras() });
            if (manejarSinPermiso(r.status)) return;
            if (!r.ok) {
                cargandoDetalle.classList.add('d-none');
                errorDetalle.textContent = await mensajeDeError(r, 'No se pudo obtener el detalle del comprobante.');
                errorDetalle.classList.remove('d-none');
                return;
            }

            const c = await r.json();
            document.getElementById('detId').textContent = `#${c.id}`;
            const detEstado = document.getElementById('detEstado');
            detEstado.replaceChildren(crearBadgeEstado(c.estado));
            document.getElementById('detCliente').textContent = c.idCliente != null ? `Cliente #${c.idCliente}` : '-';
            document.getElementById('detFormaPago').textContent = c.formaPago?.nombre ?? nombreFormaPago(c.idFormaPago);
            document.getElementById('detFechaCreacion').textContent = formatearFechaHora(c.fechaHoraCreacion);
            document.getElementById('detFechaEmision').textContent = formatearFechaHora(c.fechaHoraEmision);
            document.getElementById('detMontoTotal').textContent = formatoPrecio(c.montoTotal);
            document.getElementById('detMontoFinal').textContent = formatoPrecio(c.montoFinal);

            const detalles = Array.isArray(c.comprobanteDetalles) ? c.comprobanteDetalles : [];
            if (detalles.length === 0) {
                const tr = document.createElement('tr');
                const td = document.createElement('td');
                td.colSpan = 5;
                td.className = 'text-center text-secondary';
                td.textContent = 'Sin ítems de detalle.';
                tr.appendChild(td);
                detTablaBody.appendChild(tr);
            } else {
                for (const d of detalles) {
                    const tr = document.createElement('tr');

                    const tdId = document.createElement('td');
                    tdId.textContent = d.id;

                    const tdFuncion = document.createElement('td');
                    tdFuncion.textContent = `Función #${d.idFuncion}`;

                    const tdButaca = document.createElement('td');
                    tdButaca.textContent = d.butaca ? `${d.butaca.fila}${d.butaca.numero}` : `Butaca #${d.idButaca}`;

                    const tdPrecio = document.createElement('td');
                    tdPrecio.className = 'text-end';
                    tdPrecio.textContent = formatoPrecio(d.precioBase);

                    const tdOcupada = document.createElement('td');
                    tdOcupada.className = 'text-center';
                    const badge = document.createElement('span');
                    badge.className = d.butacaOcupada ? 'badge text-bg-success' : 'badge text-bg-secondary';
                    badge.textContent = d.butacaOcupada ? 'Sí' : 'No';
                    tdOcupada.appendChild(badge);

                    tr.append(tdId, tdFuncion, tdButaca, tdPrecio, tdOcupada);
                    detTablaBody.appendChild(tr);
                }
            }

            cargandoDetalle.classList.add('d-none');
            contenidoDetalle.hidden = false;
        } catch {
            cargandoDetalle.classList.add('d-none');
            errorDetalle.textContent = 'No se pudo conectar con el servidor.';
            errorDetalle.classList.remove('d-none');
        }
    }

    // ===== Baja (DELETE /api/Comprobantes/{id}) =====
    async function eliminarComprobante(id) {
        if (!confirm(`¿Estás seguro que deseas eliminar el comprobante #${id}?`)) return;

        ocultarMensaje();
        try {
            const r = await fetch(`/api/Comprobantes/${id}`, {
                method: 'DELETE',
                headers: cabeceras(),
            });
            if (manejarSinPermiso(r.status)) return;
            if (!r.ok) {
                mostrarMensaje('danger', await mensajeDeError(r, `No se pudo eliminar el comprobante #${id}.`));
                return;
            }
            await cargarComprobantes();
            mostrarMensaje('success', `Comprobante #${id} eliminado correctamente.`);
        } catch {
            mostrarMensaje('danger', 'No se pudo conectar con el servidor.');
        }
    }

    // ===== Tabla maestro (GET /api/Comprobantes) =====
    function crearTabla(datos) {
        const table = new Tabulator('#comprobantes-table', {
            data: datos,
            layout: 'fitColumns',
            pagination: true,
            paginationElement: document.getElementById('table-pagination'),
            paginationSize: 10,
            paginationSizeSelector: [10, 25, 50, 100],
            placeholder: 'No hay comprobantes para mostrar.',
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
                    title: 'Fecha',
                    field: 'fechaHoraCreacion',
                    minWidth: 145,
                    formatter: (cell) => formatearFechaHora(cell.getValue()),
                },
                {
                    title: 'Cliente',
                    field: 'idCliente',
                    width: 100,
                    formatter: (cell) => (cell.getValue() !== '-' ? `#${cell.getValue()}` : '-'),
                    headerFilter: 'input',
                    headerFilterParams: { elementAttributes: { 'aria-label': 'Filtrar por ID cliente' } },
                },
                {
                    title: 'Forma de pago',
                    field: 'formaPagoNombre',
                    minWidth: 130,
                    headerFilter: 'list',
                    headerFilterParams: {
                        valuesLookup: true,
                        clearable: true,
                        elementAttributes: { 'aria-label': 'Filtrar por forma de pago' },
                    },
                },
                {
                    title: 'Entradas',
                    field: 'cantidadDetalles',
                    width: 95,
                    hozAlign: 'center',
                    sorter: 'number',
                },
                {
                    title: 'Monto total',
                    field: 'montoTotal',
                    width: 125,
                    hozAlign: 'right',
                    formatter: (cell) => formatoPrecio(cell.getValue()),
                    sorter: 'number',
                },
                {
                    title: 'Monto final',
                    field: 'montoFinal',
                    width: 125,
                    hozAlign: 'right',
                    formatter: (cell) => formatoPrecio(cell.getValue()),
                    sorter: 'number',
                },
                {
                    title: 'Estado',
                    field: 'estado',
                    width: 165,
                    formatter: selectorEstado,
                    headerFilter: 'list',
                    headerFilterParams: {
                        values: {
                            '': '',
                            [ESTADO_CONFIRMADA]: ESTADO_CONFIRMADA,
                            [ESTADO_PENDIENTE]: ESTADO_PENDIENTE,
                            [ESTADO_CANCELADO]: ESTADO_CANCELADO,
                        },
                        elementAttributes: { 'aria-label': 'Filtrar por estado' },
                    },
                },
                {
                    title: 'Acciones',
                    field: 'acciones',
                    width: 105,
                    hozAlign: 'center',
                    headerSort: false,
                    formatter: (cell) => {
                        const id = cell.getData().id;
                        const grupo = document.createElement('div');
                        grupo.className = 'd-inline-flex gap-1';

                        const btnVer = document.createElement('button');
                        btnVer.type = 'button';
                        btnVer.className = 'btn btn-info btn-sm';
                        btnVer.style.width = '32px';
                        btnVer.style.height = '32px';
                        btnVer.style.padding = '0';
                        btnVer.title = 'Ver detalle';
                        btnVer.setAttribute('aria-label', `Ver detalle del comprobante #${id}`);
                        const icoVer = document.createElement('i');
                        icoVer.className = 'bi bi-eye-fill';
                        btnVer.appendChild(icoVer);
                        btnVer.addEventListener('click', () => verDetalle(id));

                        const btnDel = document.createElement('button');
                        btnDel.type = 'button';
                        btnDel.className = 'btn btn-danger btn-sm';
                        btnDel.style.width = '32px';
                        btnDel.style.height = '32px';
                        btnDel.style.padding = '0';
                        btnDel.title = 'Eliminar comprobante';
                        btnDel.setAttribute('aria-label', `Eliminar comprobante #${id}`);
                        const icoDel = document.createElement('i');
                        icoDel.className = 'bi bi-trash-fill';
                        btnDel.appendChild(icoDel);
                        btnDel.addEventListener('click', () => eliminarComprobante(id));

                        grupo.append(btnVer, btnDel);
                        return grupo;
                    },
                },
            ],
        });

        table.on('tableBuilt', () => {
            const grid = document.getElementById('comprobantes-table');
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

    async function cargarFormasPago() {
        try {
            const r = await fetch('/api/formas-pago');
            if (!r.ok) return;
            const lista = await r.json();
            formasPagoMap.clear();
            const selFormaPago = document.getElementById('ncFormaPago');
            selFormaPago.replaceChildren(new Option('Seleccionar...', ''));
            for (const f of lista) {
                formasPagoMap.set(Number(f.id), f.nombre);
                selFormaPago.add(new Option(f.nombre, f.id));
            }
        } catch { /* se mantiene fallback #id */ }
    }

    async function cargarComprobantes() {
        const r = await fetch('/api/Comprobantes', { headers: cabeceras() });
        if (manejarSinPermiso(r.status)) return;
        if (!r.ok) {
            mostrarMensaje('danger', await mensajeDeError(r, 'No se pudo cargar la lista de comprobantes.'));
            return;
        }
        const datos = (await r.json())
            .map(normalizar)
            .sort((a, b) => b.id - a.id);

        if (tabla) tabla.setData(datos);
        else tabla = crearTabla(datos);
    }

    // ===== Alta maestro-detalle (POST /api/Comprobantes) =====
    const nuevoModalEl = document.getElementById('nuevoComprobanteModal');
    const formNuevo = document.getElementById('formNuevoComprobante');
    const errorNuevo = document.getElementById('errorNuevoComprobante');
    const selFormaPago = document.getElementById('ncFormaPago');
    const selPelicula = document.getElementById('ncPelicula');
    const selFuncion = document.getElementById('ncFuncion');
    const inputCantidad = document.getElementById('ncCantidad');
    const infoFuncion = document.getElementById('ncInfoFuncion');
    const btnAgregarDetalle = document.getElementById('btnAgregarDetalle');
    const tbodyDetalles = document.getElementById('ncTablaDetallesBody');
    const totalEstimadoEl = document.getElementById('ncTotalEstimado');
    const btnGuardar = document.getElementById('btnGuardarComprobante');

    let detallesTemporales = [];
    let funcionActual = null;

    function mostrarErrorNuevo(texto) {
        errorNuevo.textContent = texto;
        errorNuevo.classList.remove('d-none');
    }

    function renderizarDetallesTemporales() {
        tbodyDetalles.replaceChildren();

        if (detallesTemporales.length === 0) {
            const tr = document.createElement('tr');
            const td = document.createElement('td');
            td.colSpan = 6;
            td.className = 'text-center text-secondary py-3';
            td.textContent = 'Todavía no agregaste ítems al detalle.';
            tr.appendChild(td);
            tbodyDetalles.appendChild(tr);
            totalEstimadoEl.textContent = formatoPrecio(0);
            return;
        }

        let total = 0;
        detallesTemporales.forEach((item, index) => {
            const subtotal = item.cantidad * item.precioBase;
            total += subtotal;

            const tr = document.createElement('tr');

            const tdPeli = document.createElement('td');
            tdPeli.textContent = item.titulo;

            const tdFunc = document.createElement('td');
            tdFunc.textContent = `${formatearFechaHora(item.fechaHoraInicio)} · Sala ${item.idSala ?? '-'}`;

            const tdCant = document.createElement('td');
            tdCant.className = 'text-center';
            tdCant.textContent = String(item.cantidad);

            const tdPrecio = document.createElement('td');
            tdPrecio.className = 'text-end';
            tdPrecio.textContent = formatoPrecio(item.precioBase);

            const tdSub = document.createElement('td');
            tdSub.className = 'text-end fw-semibold';
            tdSub.textContent = formatoPrecio(subtotal);

            const tdQuitar = document.createElement('td');
            tdQuitar.className = 'text-center';
            const btnQuitar = document.createElement('button');
            btnQuitar.type = 'button';
            btnQuitar.className = 'btn btn-outline-danger btn-sm';
            btnQuitar.title = 'Quitar ítem';
            btnQuitar.setAttribute('aria-label', `Quitar ${item.titulo}`);
            const ico = document.createElement('i');
            ico.className = 'bi bi-x-lg';
            btnQuitar.appendChild(ico);
            btnQuitar.addEventListener('click', () => {
                detallesTemporales.splice(index, 1);
                renderizarDetallesTemporales();
            });
            tdQuitar.appendChild(btnQuitar);

            tr.append(tdPeli, tdFunc, tdCant, tdPrecio, tdSub, tdQuitar);
            tbodyDetalles.appendChild(tr);
        });

        totalEstimadoEl.textContent = formatoPrecio(total);
    }

    async function cargarPeliculasCartelera() {
        selPelicula.replaceChildren(new Option('Cargando películas...', ''));
        try {
            const r = await fetch('/api/peliculas/en-cartelera');
            if (!r.ok) throw new Error();
            const peliculas = await r.json();
            selPelicula.replaceChildren(new Option('Seleccionar película...', ''));
            for (const p of peliculas) {
                selPelicula.add(new Option(p.titulo, p.id));
            }
        } catch {
            selPelicula.replaceChildren(new Option('No se pudieron cargar películas', ''));
        }
    }

    selPelicula.addEventListener('change', async () => {
        funcionActual = null;
        infoFuncion.classList.add('d-none');
        const idPelicula = selPelicula.value;

        if (!idPelicula) {
            selFuncion.replaceChildren(new Option('Elegí una película primero', ''));
            selFuncion.disabled = true;
            return;
        }

        selFuncion.replaceChildren(new Option('Cargando funciones...', ''));
        selFuncion.disabled = true;

        try {
            const r = await fetch(`/api/funciones/pelicula/${encodeURIComponent(idPelicula)}`);
            if (!r.ok) throw new Error();
            const funciones = await r.json();

            if (funciones.length === 0) {
                selFuncion.replaceChildren(new Option('Sin funciones próximas', ''));
                return;
            }

            selFuncion.replaceChildren(new Option('Seleccionar función...', ''));
            for (const f of funciones) {
                selFuncion.add(new Option(`${formatearFechaHora(f.fechaHoraInicio)} (Función #${f.idFuncion})`, f.idFuncion));
            }
            selFuncion.disabled = false;
        } catch {
            selFuncion.replaceChildren(new Option('Error al cargar funciones', ''));
        }
    });

    selFuncion.addEventListener('change', async () => {
        funcionActual = null;
        infoFuncion.classList.add('d-none');
        const idFuncion = selFuncion.value;
        if (!idFuncion) return;

        try {
            const r = await fetch(`/api/funciones/${encodeURIComponent(idFuncion)}`);
            if (!r.ok) throw new Error();
            funcionActual = await r.json();
            infoFuncion.textContent =
                `Sala ${funcionActual.idSala ?? '-'} · Precio unitario: ${formatoPrecio(funcionActual.precioBase)} · Disponibles: ${funcionActual.disponibles}`;
            infoFuncion.classList.remove('d-none');
            inputCantidad.max = String(Math.max(1, funcionActual.disponibles));
            inputCantidad.value = '1';
        } catch {
            mostrarErrorNuevo('No se pudo obtener el detalle de la función seleccionada.');
        }
    });

    btnAgregarDetalle.addEventListener('click', () => {
        errorNuevo.classList.add('d-none');

        if (!funcionActual) {
            mostrarErrorNuevo('Seleccioná una película y una función antes de agregar al detalle.');
            return;
        }

        const cantidad = Number.parseInt(inputCantidad.value, 10);
        if (!Number.isInteger(cantidad) || cantidad <= 0) {
            mostrarErrorNuevo('Ingresá una cantidad válida mayor a 0.');
            return;
        }

        if (funcionActual.disponibles <= 0) {
            mostrarErrorNuevo('La función seleccionada no tiene entradas disponibles.');
            return;
        }

        const existente = detallesTemporales.find((d) => d.idFuncion === funcionActual.idFuncion);
        const totalSolicitado = (existente ? existente.cantidad : 0) + cantidad;
        if (totalSolicitado > funcionActual.disponibles) {
            mostrarErrorNuevo(`Solo hay ${funcionActual.disponibles} entradas disponibles para esa función.`);
            return;
        }

        if (existente) {
            existente.cantidad = totalSolicitado;
        } else {
            detallesTemporales.push({
                idFuncion: funcionActual.idFuncion,
                titulo: funcionActual.titulo,
                fechaHoraInicio: funcionActual.fechaHoraInicio,
                idSala: funcionActual.idSala,
                precioBase: Number(funcionActual.precioBase ?? 0),
                cantidad,
            });
        }

        renderizarDetallesTemporales();
        inputCantidad.value = '1';
    });

    nuevoModalEl.addEventListener('show.bs.modal', async () => {
        formNuevo.reset();
        formNuevo.classList.remove('was-validated');
        errorNuevo.classList.add('d-none');
        detallesTemporales = [];
        funcionActual = null;
        selFuncion.replaceChildren(new Option('Elegí una película primero', ''));
        selFuncion.disabled = true;
        infoFuncion.classList.add('d-none');
        renderizarDetallesTemporales();

        if (formasPagoMap.size === 0) await cargarFormasPago();
        await cargarPeliculasCartelera();
    });

    formNuevo.addEventListener('submit', async (e) => {
        e.preventDefault();
        errorNuevo.classList.add('d-none');
        formNuevo.classList.add('was-validated');

        const idFormaPago = Number(selFormaPago.value);
        if (!idFormaPago) {
            mostrarErrorNuevo('Seleccioná una forma de pago.');
            return;
        }
        if (detallesTemporales.length === 0) {
            mostrarErrorNuevo('Agregá al menos un ítem al detalle antes de guardar.');
            return;
        }

        btnGuardar.disabled = true;
        const creados = [];

        try {
            // POST /api/Comprobantes recibe CrearComprobanteDTO { idFuncion, cantidad, idFormaPago }
            for (const item of detallesTemporales) {
                const r = await fetch('/api/Comprobantes', {
                    method: 'POST',
                    headers: cabeceras(true),
                    body: JSON.stringify({
                        idFuncion: item.idFuncion,
                        cantidad: item.cantidad,
                        idFormaPago,
                    }),
                });

                if (manejarSinPermiso(r.status)) return;

                if (!r.ok) {
                    const msg = await mensajeDeError(r, 'No se pudo registrar el comprobante.');
                    if (creados.length > 0) await cargarComprobantes();
                    mostrarErrorNuevo(msg);
                    return;
                }

                const creado = await r.json();
                creados.push(creado.idComprobante);
            }

            bootstrap.Modal.getOrCreateInstance(nuevoModalEl).hide();
            await cargarComprobantes();
            mostrarMensaje(
                'success',
                creados.length === 1
                    ? `Comprobante #${creados[0]} creado con éxito.`
                    : `Comprobantes #${creados.join(', #')} creados con éxito.`
            );
        } catch {
            mostrarErrorNuevo('No se pudo conectar con el servidor.');
        } finally {
            btnGuardar.disabled = false;
        }
    });

    // ===== Carga inicial =====
    try {
        await cargarFormasPago();
        await cargarComprobantes();
    } catch {
        mostrarMensaje('danger', 'No se pudo conectar con el servidor.');
    }
})();
