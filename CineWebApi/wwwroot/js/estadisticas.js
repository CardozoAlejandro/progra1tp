(async () => {
    const dinero = (v) => '$' + Number(v).toLocaleString('es-AR', { maximumFractionDigits: 0 });
    const datosActuales = Sesion.usuarioActual();

    if (!Sesion.tieneRol(datosActuales, 'Admin')) {
        window.location.href = '/index.html';
        return;
    }

    async function obtener(url) {
        const r = await fetch(url, { headers: { Authorization: 'Bearer ' + Sesion.token() } });
        if (r.status === 401 || r.status === 403) {
            window.location.href = '/index.html';
            return null;
        }
        if (!r.ok) throw new Error('Error al cargar ' + url);
        return r.json();
    }

    function mostrarVacio(canvasId, texto) {
        const canvas = document.getElementById(canvasId);
        canvas.parentElement.innerHTML =
            `<div class="text-center text-secondary pt-5">${texto}</div>`;
    }

    try {
        const [topPeliculas, mensuales] = await Promise.all([
            obtener('/api/comprobantes/ingresos-por-pelicula'),
            obtener('/api/comprobantes/ingresos-mensuales'),
        ]);

        if (!topPeliculas?.length) {
            mostrarVacio('donut-chart', 'Todavía no hay ventas confirmadas.');
        } else {
            new Chart(document.getElementById('donut-chart'), {
                type: 'doughnut',
                data: {
                    labels: topPeliculas.map((p) => p.pelicula),
                    datasets: [{
                        label: 'Ingresos',
                        data: topPeliculas.map((p) => p.total),
                        backgroundColor: [
                            '#0d6efd', '#20c997', '#ffc107', '#d63384', '#6f42c1',
                            '#fd7e14', '#0dcaf0', '#198754', '#dc3545', '#6c757d',
                        ],
                    }],
                },
                options: {
                    cutout: '60%',
                    plugins: {
                        legend: { position: 'right' },
                        tooltip: {
                            callbacks: { label: (c) => ` ${c.label}: ${dinero(c.parsed)}` },
                        },
                    },
                },
            });
        }

        if (!mensuales?.length) {
            mostrarVacio('line-chart', 'Todavía no hay ventas confirmadas.');
        } else {
            new Chart(document.getElementById('line-chart'), {
                type: 'line',
                data: {
                    labels: mensuales.map((m) =>
                        new Date(m.anio, m.mes - 1, 1)
                            .toLocaleDateString('es-AR', { month: 'short', year: 'numeric' })),
                    datasets: [{
                        label: 'Ingresos',
                        data: mensuales.map((m) => m.total),
                        borderColor: '#0d6efd',
                        backgroundColor: '#0d6efd',
                        borderWidth: 3,
                        pointRadius: 4,
                        tension: 0.3,
                    }],
                },
                options: {
                    plugins: {
                        legend: { display: false },
                        tooltip: {
                            callbacks: { label: (c) => ` Ingresos: ${dinero(c.parsed.y)}` },
                        },
                    },
                    scales: {
                        y: { beginAtZero: true, ticks: { callback: (v) => dinero(v) } },
                    },
                },
            });
        }
    } catch (e) {
        console.error(e);
        mostrarVacio('donut-chart', 'No se pudieron cargar las estadísticas.');
        mostrarVacio('line-chart', 'No se pudieron cargar las estadísticas.');
    }
})();