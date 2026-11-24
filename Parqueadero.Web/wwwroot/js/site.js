// Plano del parqueadero (panel): al tocar un cupo se resalta y se llenan los datos del panel.
(function () {
    const plano = document.getElementById('plano');
    if (!plano) return; // Solo existe en el panel.

    const panel = document.getElementById('plano-panel');
    let seleccionado = null;

    plano.addEventListener('click', function (evento) {
        const cupo = evento.target.closest('.cupo-plano');
        if (!cupo) return;

        if (seleccionado) seleccionado.classList.remove('cupo-plano--seleccionado');
        seleccionado = cupo;
        cupo.classList.add('cupo-plano--seleccionado');

        // En SVG el último elemento se dibuja encima: así el borde amarillo no queda tapado por los vecinos.
        plano.appendChild(cupo);

        const libre = cupo.dataset.estado === 'Libre';

        document.getElementById('plano-ayuda').hidden = true;
        document.getElementById('plano-seleccion').hidden = false;
        document.getElementById('sel-codigo').textContent = cupo.dataset.codigo;
        document.getElementById('sel-estado').textContent = cupo.dataset.tipo + ' · ' + cupo.dataset.estado;
        document.getElementById('sel-zona').textContent = cupo.dataset.zona;
        document.getElementById('sel-metros').textContent = 'A unos ' + cupo.dataset.metros + ' m de la portería';

        // Cupo ocupado: placa y hora de entrada del vehículo.
        const ocupado = document.getElementById('sel-ocupado');
        ocupado.hidden = libre;
        ocupado.textContent = 'Placa ' + cupo.dataset.placa + ' · entró a las ' + cupo.dataset.hora;
        document.getElementById('plano-panel').classList.add('plano-panel--seleccion');

        // Solo un cupo libre se puede usar para registrar una entrada.
        const boton = document.getElementById('sel-entrada');
        boton.hidden = !libre;
        boton.href = panel.dataset.urlEntrada + '?cupoId=' + cupo.dataset.id;
    });
})();

// Página pública "¿Hay cupo?" y recuadro del login: piden /Disponibilidad/Estado cada 10 s y actualizan sin recargar.
// Esa respuesta solo trae { libresCarro, libresMoto, ocupados: [ids] }: nunca hay placas ni horas.
(function () {
    const INTERVALO = 10000;

    function pedirEstado(url) {
        return fetch(url, { cache: 'no-store' }).then(function (r) {
            if (!r.ok) throw new Error('estado ' + r.status);
            return r.json();
        });
    }

    // ----- Página /Disponibilidad -----
    const pagina = document.getElementById('disponibilidad');
    if (pagina) {
        const plano = document.getElementById('plano-publico');
        const etiquetaActualizado = document.getElementById('pub-actualizado');
        let seleccionado = null;
        let ultima = Date.now();

        function describir(cupo) {
            document.getElementById('pub-ayuda').hidden = true;
            document.getElementById('pub-seleccion').hidden = false;
            document.getElementById('pub-codigo').textContent = cupo.dataset.codigo;
            const ocupado = cupo.classList.contains('cupo-plano--ocupado');
            document.getElementById('pub-estado').textContent = ocupado ? 'Ocupado' : 'Libre';
            document.getElementById('pub-zona').textContent = cupo.dataset.zona;
            document.getElementById('pub-metros').textContent = 'A unos ' + cupo.dataset.metros + ' m de la portería';
        }

        plano.addEventListener('click', function (evento) {
            const cupo = evento.target.closest('.cupo-plano');
            if (!cupo) return;
            if (seleccionado) seleccionado.classList.remove('cupo-plano--seleccionado');
            seleccionado = cupo;
            cupo.classList.add('cupo-plano--seleccionado');
            plano.appendChild(cupo); // El último elemento se dibuja encima.
            describir(cupo);
        });

        function aplicar(estado) {
            const ocupados = new Set(estado.ocupados);
            plano.querySelectorAll('.cupo-plano').forEach(function (cupo) {
                cupo.classList.toggle('cupo-plano--ocupado', ocupados.has(parseInt(cupo.dataset.id, 10)));
            });

            document.getElementById('pub-libres-carro').textContent = estado.libresCarro;
            document.getElementById('pub-libres-moto').textContent = estado.libresMoto;
            document.getElementById('pub-titulo').textContent = estado.libresCarro > 0
                ? estado.libresCarro + (estado.libresCarro === 1 ? ' cupo libre' : ' cupos libres')
                : 'Lleno para carros';
            document.getElementById('pub-subtitulo').textContent = estado.libresCarro > 0
                ? 'Para carro · entra sin esperar'
                : 'Se actualiza solo: apenas salga un carro lo verás aquí';

            if (seleccionado) describir(seleccionado); // Si el cupo tocado cambió, la tarjeta también.
            ultima = Date.now();
        }

        function actualizar() {
            pedirEstado(pagina.dataset.urlEstado).then(aplicar).catch(function () { /* se reintenta en 10 s */ });
        }

        setInterval(actualizar, INTERVALO);
        setInterval(function () {
            etiquetaActualizado.textContent = 'Actualizado hace ' + Math.floor((Date.now() - ultima) / 1000) + ' s';
        }, 1000);
    }

    // ----- Recuadro "¿Hay cupo?" del login -----
    const login = document.getElementById('login-disponibilidad');
    if (login) {
        function actualizarLogin() {
            pedirEstado(login.dataset.urlEstado).then(function (estado) {
                document.getElementById('login-libres-carro').textContent = estado.libresCarro;
                document.getElementById('login-texto-carro').textContent =
                    (estado.libresCarro === 1 ? 'cupo libre' : 'cupos libres') + ' para carro';
                document.getElementById('login-libres-moto').textContent = estado.libresMoto;
                document.getElementById('login-texto-moto').textContent =
                    (estado.libresMoto === 1 ? 'cupo libre' : 'cupos libres') + ' para moto';
            }).catch(function () { /* se deja el "—" y se reintenta */ });
        }
        actualizarLogin();
        setInterval(actualizarLogin, INTERVALO);
    }
})();
