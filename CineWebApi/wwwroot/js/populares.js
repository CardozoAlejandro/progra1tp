(async function () {
    const seccion = document.getElementById('seccionPopulares');
    const contenedor = document.getElementById('masVendidas');
    if (!seccion || !contenedor) return;

    function el(etiqueta, clase, texto) {
        const nodo = document.createElement(etiqueta);
        if (clase) nodo.className = clase;
        if (texto !== undefined) nodo.textContent = texto;
        return nodo;
    }

    function dato(icono, texto) {
        const span = el('span');
        span.append(el('i', `bi ${icono} me-1`), texto);
        return span;
    }

    function crearCard(p, posicion) {
        const col = el('div', 'col');
        const card = el('div', 'card pelicula-card h-100');
        const cuerpo = el('div', 'card-body d-flex gap-3');

        const poster = el('div', 'pelicula-poster');
        const placeholder = el('div', 'pelicula-poster-placeholder');
        placeholder.appendChild(el('i', 'bi bi-film'));
        poster.appendChild(placeholder);

        const info = el('div', 'pelicula-info flex-grow-1');
        info.appendChild(el('h3', 'pelicula-titulo', p.titulo));

        const meta = el('p', 'pelicula-meta');
        if (p.duracionMinutos) meta.appendChild(dato('bi-clock', `${p.duracionMinutos} min`));
        if (p.genero) meta.appendChild(dato('bi-film', p.genero));
        info.appendChild(meta);

        if (p.fechaEstreno) {
            const estreno = el('p', 'pelicula-meta');
            const fecha = new Date(p.fechaEstreno).toLocaleDateString('es-AR');
            estreno.append(el('i', 'bi bi-calendar-event me-1'), 'Lanzamiento: ');
            estreno.appendChild(el('strong', '', fecha));
            info.appendChild(estreno);
        }

        if (p.clasificacion) {
            const clas = el('div', 'pelicula-dato');
            clas.append(el('span', 'pelicula-etiqueta', 'Clasificación'), el('strong', '', p.clasificacion));
            info.appendChild(clas);
        }

        cuerpo.append(poster, info);
        card.appendChild(cuerpo);
        col.appendChild(card);
        return col;
    }

    try {
        const r = await fetch('/api/peliculas/mas-vendidas');
        if (!r.ok) return;
        const peliculas = await r.json();
        if (!peliculas.length) return;

        peliculas.forEach((p) => contenedor.appendChild(crearCard(p)));
        seccion.classList.remove('d-none');
    } catch {
    }
})();