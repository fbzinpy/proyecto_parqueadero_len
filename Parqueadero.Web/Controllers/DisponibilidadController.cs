using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parqueadero.Core.Entidades;
using Parqueadero.Core.Servicios;
using Parqueadero.Web.Models;

namespace Parqueadero.Web.Controllers;

// Página abierta (sin iniciar sesión) para ver si hay cupo.
// Privacidad: nunca muestra placas, horas ni datos de usuarios; solo qué cupos están libres u ocupados.
[AllowAnonymous]
public class DisponibilidadController : Controller
{
    private readonly ServicioParqueadero _servicio;

    public DisponibilidadController(ServicioParqueadero servicio)
    {
        _servicio = servicio;
    }

    public IActionResult Index()
    {
        EstadoParqueadero estado = _servicio.CargarEstado();
        Dictionary<string, PosicionCupo> posiciones = PlanoParqueadero.ObtenerPosiciones().ToDictionary(p => p.Codigo);

        DisponibilidadViewModel modelo = new DisponibilidadViewModel();
        foreach (Cupo cupo in _servicio.ObtenerCupos())
        {
            PosicionCupo posicion = posiciones[cupo.Codigo];
            bool ocupado = estado.EstaOcupado(cupo.Id);

            modelo.Cupos.Add(new CupoPublico
            {
                Id = cupo.Id,
                Codigo = cupo.Codigo,
                Zona = posicion.Zona,
                Metros = PlanoParqueadero.MetrosAPorteria(posicion),
                Ocupado = ocupado
            });

            if (cupo.Tipo == TipoVehiculo.Carro)
            {
                modelo.TotalCarros++;
                if (!ocupado) modelo.LibresCarro++;
            }
            else
            {
                modelo.TotalMotos++;
                if (!ocupado) modelo.LibresMoto++;
            }
        }

        return View(modelo);
    }

    // JSON mínimo que consulta el JavaScript cada 10 s: { libresCarro, libresMoto, ocupados: [ids] }.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Estado()
    {
        EstadoParqueadero estado = _servicio.CargarEstado();
        int libresCarro = 0;
        int libresMoto = 0;
        List<int> ocupados = new List<int>();

        foreach (Cupo cupo in _servicio.ObtenerCupos())
        {
            if (estado.EstaOcupado(cupo.Id))
            {
                ocupados.Add(cupo.Id);
            }
            else if (cupo.Tipo == TipoVehiculo.Carro)
            {
                libresCarro++;
            }
            else
            {
                libresMoto++;
            }
        }

        return Json(new { libresCarro, libresMoto, ocupados });
    }
}
