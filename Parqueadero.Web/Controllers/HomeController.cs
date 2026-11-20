using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parqueadero.Core.Entidades;
using Parqueadero.Core.Servicios;
using Parqueadero.Web.Models;

namespace Parqueadero.Web.Controllers;

public class HomeController : Controller
{
    private readonly ServicioParqueadero _servicio;

    public HomeController(ServicioParqueadero servicio)
    {
        _servicio = servicio;
    }

    // Panel del operador: ocupación, vehículos dentro y mapa de cupos. Pueden entrar los dos roles.
    // tipo y buscar llegan por la URL (ej: /?tipo=Moto&buscar=ABC) desde los filtros y el buscador.
    [Authorize(Roles = "Administrador,Operador")]
    public IActionResult Index(string? tipo, string? buscar)
    {
        EstadoParqueadero estado = _servicio.CargarEstado();
        List<Cupo> cupos = _servicio.ObtenerCupos();
        List<Ticket> adentro = _servicio.ObtenerTicketsAbiertos();

        // Filtro por tipo de vehículo (botones Todos / Carro / Moto).
        // IsDefined evita que un número cualquiera (ej: ?tipo=5) pase como tipo válido.
        if (Enum.TryParse(tipo, out TipoVehiculo tipoFiltro) && Enum.IsDefined(tipoFiltro))
        {
            adentro = adentro.Where(t => t.Tipo == tipoFiltro).ToList();
        }
        else
        {
            tipo = null;
        }

        // Buscador: muestra las placas que contienen el texto escrito.
        string textoBuscado = ServicioParqueadero.NormalizarPlaca(buscar);
        if (textoBuscado.Length > 0)
        {
            adentro = adentro.Where(t => t.Placa.Contains(textoBuscado)).ToList();
        }

        PanelViewModel modelo = new PanelViewModel
        {
            TotalCupos = estado.TotalCupos,
            CuposOcupados = estado.CuposOcupados,
            Cupos = cupos,
            IdsOcupados = cupos.Where(c => estado.EstaOcupado(c.Id)).Select(c => c.Id).ToHashSet(),
            VehiculosAdentro = adentro,
            FiltroTipo = tipo,
            Buscar = buscar
        };

        return View(modelo);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
