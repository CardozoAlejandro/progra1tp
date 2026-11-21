const contenedor = document.getElementById('cartelera');
const estado = document.getElementById('estadoCartelera');

function crear(etiqueta, clase, texto) {
    const el = document.createElement(etiqueta);
    if (clase) el.className = clase;
    if (texto !== undefined) el.textContent = texto;
    return el;
}

function icono(clase) {
    const i = crear('i', `bi ${clase} me-1`);
    i.setAttribute('aria-hidden', 'true');
    return i;
}

function formatearFecha(valor) {
    if (!valor) return '';
    const [anio, mes, dia] = valor.substring(0, 10).split('-');
    return `${dia}/${mes}/${anio}`;
}

function dato(etiqueta, valor) {
    const d = crear('div', 'pelicula-dato');
    d.append(crear('span', 'pelicula-etiqueta', etiqueta), crear('strong', '', valor));
    return d;
}

function crearCard(p, funciones) {
    const col = crear('div', 'col');
    const card = crear('div', 'card pelicula-card h-100');
    const cuerpo = crear('div', 'card-body d-flex gap-3');

    const poster = crear('div', 'pelicula-poster');
    if (p.posterUrl) {
        const img = crear('img');
        img.src = p.posterUrl;
        img.alt = `Póster de ${p.titulo}`;
        img.loading = 'lazy';
        poster.appendChild(img);
    } else {
        const placeholder = crear('div', 'pelicula-poster-placeholder');
        const i = crear('i', 'bi bi-film');
        i.setAttribute('aria-hidden', 'true');
        placeholder.appendChild(i);
        poster.appendChild(placeholder);
    }

    const info = crear('div', 'pelicula-info flex-grow-1');
    const titulo = crear('h2', 'pelicula-titulo');
    const enlace = crear('a', '', p.titulo);
    enlace.href = `/pelicula.html?id=${encodeURIComponent(p.id)}`;
    titulo.appendChild(enlace);
    info.appendChild(titulo);

    const meta = crear('p', 'pelicula-meta');
    const duracion = crear('span');
    duracion.append(icono('bi-clock'), `${p.duracionMinutos} min`);
    const genero = crear('span');
    genero.append(icono('bi-film'), p.genero ?? '');
    meta.append(duracion, genero);
    info.appendChild(meta);

    const lanzamiento = crear('p', 'pelicula-meta');
    lanzamiento.append(icono('bi-calendar-event'), 'Lanzamiento: ', crear('strong', '', formatearFecha(p.fechaEstreno)));
    info.appendChild(lanzamiento);

    info.appendChild(dato('Clasificación', p.clasificacion ?? '-'));
    info.appendChild(Horarios.crearBloque(funciones, p.titulo));

    cuerpo.append(poster, info);
    card.appendChild(cuerpo);
    col.appendChild(card);
    return col;
}

let controlador;

async function cargarCartelera(titulo = '') {
    controlador?.abort();
    controlador = new AbortController();

    estado.textContent = titulo ? 'Buscando...' : 'Cargando cartelera...';
    estado.hidden = false;

    try {
        const url = '/api/peliculas/en-cartelera' +
            (titulo ? `?titulo=${encodeURIComponent(titulo)}` : '');

        const [respuesta, horarios] = await Promise.all([
            fetch(url, { signal: controlador.signal }),
            Horarios.cargar(),
        ]);
        if (!respuesta.ok) throw new Error(`Error ${respuesta.status}`);

        const peliculas = await respuesta.json();
        contenedor.replaceChildren(...peliculas.map((p) => crearCard(p, horarios.get(p.id) ?? [])));

        if (peliculas.length === 0) {
            estado.textContent = titulo
                ? `No se encontraron películas para "${titulo}".`
                : 'No hay películas en cartelera por ahora.';
            estado.hidden = false;
        } else {
            estado.hidden = true;
        }
    } catch (error) {
        if (error.name === 'AbortError') return;
        console.error(error);
        estado.textContent = 'No se pudo cargar la cartelera. Intentá de nuevo más tarde.';
        estado.hidden = false;
    }
}

const buscador = document.getElementById('buscarPelicula');
let temporizador;
buscador.addEventListener('input', () => {
    clearTimeout(temporizador);
    temporizador = setTimeout(() => cargarCartelera(buscador.value.trim()), 300);
});

cargarCartelera();