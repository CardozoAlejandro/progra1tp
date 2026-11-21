const Horarios = (function () {
    // Adónde va "Comprar" (ajustalo cuando tengas la página de compra)
    const URL_COMPRA = '/comprar.html?funcion=';

    // Devuelve un Map: idPelicula -> [funciones]
    async function cargar() {
        const porPelicula = new Map();
        try {
            const r = await fetch('/api/funciones/horarios');
            if (!r.ok) {
                console.warn('No se pudieron cargar los horarios. Estado:', r.status);
                return porPelicula;
            }
            for (const f of await r.json()) {
                if (!porPelicula.has(f.idPelicula)) porPelicula.set(f.idPelicula, []);
                porPelicula.get(f.idPelicula).push(f);
            }
        } catch (e) {
            console.warn('No se pudieron cargar los horarios.', e);
        }
        return porPelicula;
    }

    // "2026-11-19T15:40:00" -> "15:40"
    const hora = (fechaHora) => String(fechaHora).substring(11, 16);

    // "2026-11-19T15:40:00" -> "19/11/2026"
    function fecha(fechaHora) {
        const [anio, mes, dia] = String(fechaHora).substring(0, 10).split('-');
        return `${dia}/${mes}/${anio}`;
    }

    const iso = (d) =>
        `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;

    // "2026-11-19" -> "Hoy" / "Mañana" / "21/11"
    function etiquetaDia(fechaISO) {
        const hoy = new Date();
        const manana = new Date();
        manana.setDate(hoy.getDate() + 1);
        if (fechaISO === iso(hoy)) return 'Hoy';
        if (fechaISO === iso(manana)) return 'Mañana';
        const [, mes, dia] = fechaISO.split('-');
        return `${dia}/${mes}`;
    }

    function crear(etiqueta, clase, texto) {
        const nodo = document.createElement(etiqueta);
        if (clase) nodo.className = clase;
        if (texto !== undefined) nodo.textContent = texto;
        return nodo;
    }

    // ===== Popup de confirmación =====
    let modal;
    let idFuncionElegida;
    let campos;

    function asegurarModal() {
        if (modal) return;

        const div = crear('div', 'modal fade');
        div.id = 'modalFuncion';
        div.tabIndex = -1;
        div.setAttribute('aria-labelledby', 'modalFuncionTitulo');
        div.setAttribute('aria-hidden', 'true');
        // Marcado fijo: los datos de la función se cargan después con textContent
        div.innerHTML = `
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content text-center">
                    <div class="modal-body p-4">
                        <h2 class="modal-title fs-3 fw-bold text-uppercase mb-3" id="modalFuncionTitulo">Atención</h2>
                        <p>Usted ha seleccionado la siguiente función</p>
                        <p class="mb-1"><strong>Película:</strong> <span data-campo="titulo"></span></p>
                        <p class="mb-4"><strong>Fecha y hora:</strong> <span data-campo="fecha"></span></p>
                        <div class="d-flex justify-content-center gap-2">
                            <button type="button" class="btn btn-outline-primary" data-bs-dismiss="modal">Cancelar</button>
                            <button type="button" class="btn btn-primary" data-campo="comprar">Comprar</button>
                        </div>
                    </div>
                </div>
            </div>`;
        document.body.appendChild(div);

        campos = {
            titulo: div.querySelector('[data-campo="titulo"]'),
            fecha: div.querySelector('[data-campo="fecha"]'),
        };
        div.querySelector('[data-campo="comprar"]').addEventListener('click', () => {
            window.location.href = URL_COMPRA + encodeURIComponent(idFuncionElegida);
        });
        modal = new bootstrap.Modal(div);
    }

    // Muestra el popup con la película y el día y hora de la función
    function confirmar(titulo, funcion) {
        asegurarModal();
        idFuncionElegida = funcion.idFuncion;
        campos.titulo.textContent = titulo;
        campos.fecha.textContent = `${fecha(funcion.fechaHoraInicio)} ${hora(funcion.fechaHoraInicio)} hs.`;
        modal.show();
    }

    // Botón de horario que abre el popup
    function crearBoton(titulo, funcion) {
        const boton = crear('button', 'btn btn-outline-secondary btn-sm', hora(funcion.fechaHoraInicio));
        boton.type = 'button';
        boton.dataset.idFuncion = funcion.idFuncion;
        boton.addEventListener('click', () => confirmar(titulo, funcion));
        return boton;
    }

    // Bloque "Horarios" agrupado por día
    function crearBloque(funciones, titulo) {
        const bloque = crear('div', 'pelicula-dato');
        bloque.appendChild(crear('span', 'pelicula-etiqueta', 'Horarios'));

        if (!funciones || funciones.length === 0) {
            bloque.appendChild(crear('span', 'text-secondary', 'Sin funciones próximas'));
            return bloque;
        }

        const porDia = new Map();
        for (const f of funciones) {
            const dia = String(f.fechaHoraInicio).substring(0, 10);
            if (!porDia.has(dia)) porDia.set(dia, []);
            porDia.get(dia).push(f);
        }

        for (const dia of [...porDia.keys()].sort()) {
            const fila = crear('div', 'd-flex align-items-center gap-2 mt-1');
            fila.appendChild(crear('span', 'text-secondary small flex-shrink-0', etiquetaDia(dia)));

            const lista = crear('div', 'pelicula-horarios mt-0');
            const delDia = porDia.get(dia).sort((a, b) => a.fechaHoraInicio.localeCompare(b.fechaHoraInicio));
            for (const f of delDia) lista.appendChild(crearBoton(titulo, f));
            fila.appendChild(lista);
            bloque.appendChild(fila);
        }
        return bloque;
    }

    return { cargar, crearBloque, crearBoton, confirmar };
})();