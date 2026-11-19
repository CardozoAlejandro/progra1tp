(async function () {
    const lista = document.getElementById('listaComprobantes');
    if (!lista) return;

    const estado = document.getElementById('estadoComprobantes');
    const errorBox = document.getElementById('errorComprobantes');
    const alerta = document.getElementById('alertaCompra');

    if (!Sesion.usuarioActual()) {
        window.location.href = '/auth/login.html?volver=' +
            encodeURIComponent(window.location.pathname + window.location.search);
        return;
    }

    const compraNueva = new URLSearchParams(window.location.search).get('compra');

    const esc = (t) => String(t ?? '').replace(/[&<>"']/g, (c) =>
    ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

    const formatoPrecio = (n) =>
        '$ ' + Number(n).toLocaleString('es-AR', { maximumFractionDigits: 2 });

    function fechaHora(valor) {
        const [dia, hora] = String(valor).split('T');
        const [a, m, d] = dia.split('-');
        return `${d}/${m}/${a} - ${hora.substring(0, 5)}HS`;
    }

    function badgeEstado(e) {
        const clave = String(e).toLowerCase();
        const clase = clave.startsWith('confirmad') ? 'text-bg-success'
            : clave.startsWith('pendiente') ? 'text-bg-warning'
                : clave.startsWith('cancelad') ? 'text-bg-danger'
                    : 'text-bg-secondary';
        return `<span class="badge ${clase}">${esc(e)}</span>`;
    }

    function tarjeta(c) {
        const grupos = new Map();
        c.entradas.forEach((e) => {
            if (!grupos.has(e.idFuncion)) grupos.set(e.idFuncion, { f: e, butacas: [] });
            grupos.get(e.idFuncion).butacas.push(e.butaca);
        });

        const funciones = [...grupos.values()].map(({ f, butacas }) => `
            <div class="mb-3">
                <div class="fw-bold text-uppercase">${esc(f.pelicula)}</div>
                <div class="text-secondary small">
                    ${esc(fechaHora(f.fechaHoraInicio))} &middot; SALA ${esc(f.idSala)}
                </div>
                <div class="small mt-1">
                    Entradas: <strong>${butacas.length}</strong> &middot;
                    Butacas: <strong>${esc(butacas.join(', '))}</strong>
                </div>
            </div>`).join('');

        const hayDescuento = c.montoFinal < c.montoTotal;
        const destacada = String(c.idComprobante) === compraNueva;

        return `
        <div class="col-lg-6">
            <div class="card h-100 ${destacada ? 'border-success border-2' : ''}">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <span class="fw-semibold">Comprobante #${esc(c.idComprobante)}</span>
                    ${badgeEstado(c.estado)}
                </div>
                <div class="card-body">
                    ${funciones}
                    <ul class="list-unstyled small text-secondary mb-0">
                        <li>Fecha de compra: ${esc(fechaHora(c.fechaHoraEmision ?? c.fechaHoraCreacion))}</li>
                        <li>Forma de pago: ${esc(c.formaPago ?? '-')}</li>
                        ${hayDescuento ? `<li>Promoción: ${esc(c.promocion ?? '-')} (subtotal ${esc(formatoPrecio(c.montoTotal))})</li>` : ''}
                    </ul>
                </div>
                <div class="card-footer d-flex justify-content-between align-items-center">
                    <span class="small text-uppercase fw-semibold">Total</span>
                    <span class="fs-5 fw-bold">${esc(formatoPrecio(c.montoFinal))}</span>
                </div>
            </div>
        </div>`;
    }

    try {
        const r = await fetch('/api/comprobantes/mis-comprobantes', {
            headers: { 'Authorization': `Bearer ${Sesion.token()}` }
        });

        if (r.status === 401) {
            Sesion.cerrar(false);
            window.location.href = '/auth/login.html?volver=' +
                encodeURIComponent(window.location.pathname + window.location.search);
            return;
        }
        if (!r.ok) throw new Error('No se pudieron cargar tus compras.');

        const datos = await r.json();
        estado.classList.add('d-none');

        if (datos.length === 0) {
            estado.textContent = 'Todavía no realizaste ninguna compra.';
            estado.classList.remove('d-none');
        } else {
            lista.innerHTML = datos.map(tarjeta).join('');
        }

        if (compraNueva && datos.some((c) => String(c.idComprobante) === compraNueva)) {
            alerta.textContent = `¡Compra realizada con éxito! Tu comprobante es el #${compraNueva}.`;
            alerta.classList.remove('d-none');
            history.replaceState(null, '', window.location.pathname);
        }
    } catch (e) {
        estado.classList.add('d-none');
        errorBox.textContent = e.message;
        errorBox.classList.remove('d-none');
    }
})();