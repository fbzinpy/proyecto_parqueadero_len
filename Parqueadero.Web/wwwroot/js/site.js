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

        // Solo un cupo libre se puede usar para registrar una entrada.
        const boton = document.getElementById('sel-entrada');
        boton.hidden = !libre;
        boton.href = panel.dataset.urlEntrada + '?cupoId=' + cupo.dataset.id;
    });
})();
