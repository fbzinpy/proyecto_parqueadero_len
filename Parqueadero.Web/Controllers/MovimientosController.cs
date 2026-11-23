using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parqueadero.Core.Entidades;
using Parqueadero.Core.Servicios;
using Parqueadero.Web.Models;

namespace Parqueadero.Web.Controllers;

// Entradas, salidas e historial de vehículos.
// Pueden entrar los dos roles.
[Authorize(Roles = "Administrador,Operador")]
public class MovimientosController : Controller
{
    private readonly ServicioParqueadero _servicio;

    public MovimientosController(ServicioParqueadero servicio)
    {
        _servicio = servicio;
    }

    // ---------- Entrada ----------

    [HttpGet]
    public IActionResult Entrada(int? cupoId)
    {
        EntradaViewModel modelo = CrearModeloEntrada(new EntradaViewModel());

        // Viene del plano ("Registrar entrada aquí"): deja ese cupo elegido y el tipo que le corresponde.
        // Si el cupo no existe o ya está ocupado, no está en CuposLibres y se ignora.
        Cupo? elegido = modelo.CuposLibres.FirstOrDefault(c => c.Id == cupoId);
        if (elegido != null)
        {
            modelo.CupoId = elegido.Id;
            modelo.Tipo = elegido.Tipo;
        }

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Entrada(string? placa, TipoVehiculo tipo, int? cupoId)
    {
        var (ticket, error) = _servicio.RegistrarEntrada(placa, tipo, cupoId);

        if (ticket == null)
        {
            // Se vuelve a mostrar el formulario con lo que escribió el usuario y el error.
            EntradaViewModel modelo = new EntradaViewModel { Placa = placa, Tipo = tipo, CupoId = cupoId, Error = error };
            return View(CrearModeloEntrada(modelo));
        }

        // TempData sobrevive a la redirección: el layout muestra este mensaje en el panel.
        TempData["Mensaje"] = $"Entrada registrada: {ticket.Placa} en el cupo {ticket.Cupo!.Codigo} a las {ticket.HoraEntrada:HH:mm}.";
        return RedirectToAction("Index", "Home");
    }

    private EntradaViewModel CrearModeloEntrada(EntradaViewModel modelo)
    {
        EstadoParqueadero estado = _servicio.CargarEstado();
        modelo.CuposLibres = _servicio.ObtenerCupos().Where(c => !estado.EstaOcupado(c.Id)).ToList();
        return modelo;
    }

    // ---------- Salida ----------

    // Sin placa: solo el buscador. Con placa: muestra el ticket y cuánto debe pagar si sale ahora.
    [HttpGet]
    public IActionResult Salida(string? placa)
    {
        SalidaViewModel modelo = new SalidaViewModel { Placa = placa };

        if (string.IsNullOrWhiteSpace(placa))
        {
            return View(modelo);
        }

        Ticket? ticket = _servicio.BuscarTicketAbierto(placa);
        if (ticket == null)
        {
            modelo.Error = $"No hay ningún vehículo adentro con la placa {ServicioParqueadero.NormalizarPlaca(placa)}.";
            return View(modelo);
        }

        modelo.Ticket = ticket;
        modelo.Placa = ticket.Placa;
        modelo.HoraSalida = DateTime.Now;
        modelo.ValorHora = _servicio.ValorHora(ticket.Tipo);
        modelo.HorasCobradas = CalculadoraCobro.HorasCobradas(ticket.HoraEntrada, modelo.HoraSalida);
        modelo.Total = modelo.HorasCobradas * modelo.ValorHora;
        return View(modelo);
    }

    // Botón "Cobrar y dar salida": cierra el ticket y libera el cupo.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Salida")]
    public IActionResult ConfirmarSalida(string? placa)
    {
        Ticket? ticket = _servicio.RegistrarSalida(placa);

        if (ticket == null)
        {
            TempData["Mensaje"] = $"No hay ningún vehículo adentro con la placa {ServicioParqueadero.NormalizarPlaca(placa)}.";
            return RedirectToAction("Index", "Home");
        }

        TempData["Mensaje"] = $"Salida registrada: {ticket.Placa} pagó {Formato.Dinero(ticket.Total ?? 0)}. El cupo {ticket.Cupo!.Codigo} quedó libre.";
        return RedirectToAction("Index", "Home");
    }

    // ---------- Historial ----------

    public IActionResult Historial()
    {
        return View(_servicio.ObtenerHistorial());
    }
}
