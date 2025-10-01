using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Microsoft.Extensions.Configuration;
using System;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class ContratosController : Controller
    {
        private readonly RepositorioContrato repo;
        private readonly RepositorioInmueble repoInmueble;
        private readonly RepositorioInquilino repoInquilino;
        private readonly RepositorioUsuario repoUsuario;
        private readonly IConfiguration _configuration;


        public ContratosController(IConfiguration configuration)
        {
             _configuration = configuration;
            repo = new RepositorioContrato(configuration);
            repoInmueble = new RepositorioInmueble(configuration);
            repoInquilino = new RepositorioInquilino(configuration);
            repoUsuario = new RepositorioUsuario(configuration);
        }

        // ======================================
        // LISTAR CONTRATOS
        // ======================================
        public IActionResult Index()
        {
            var contratos = repo.ObtenerTodos();

            // Determinar rol del usuario
            bool esAdmin = HttpContext.User.IsInRole("Administrador");
            ViewBag.EsAdmin = esAdmin;

            return View(contratos);
        }



        public IActionResult Details(int id)
        {
            var contrato = repo.ObtenerPorId(id);
            if (contrato == null) return NotFound();
            return View(contrato);
        }

        // ======================================
        // CREAR CONTRATO
        // ======================================
        public IActionResult Create()
        {
            // 🔹 Mostrar todos los inmuebles, no solo "disponibles".
            // La validación se hace por fechas en RepositorioContrato.
            ViewBag.Inmuebles = repoInmueble.ObtenerTodos();
            ViewBag.Inquilinos = repoInquilino.ObtenerTodos();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Contrato contrato)
        {
            try
            {
                // Asignar el usuario logueado como creador
                var usuario = repoUsuario.ObtenerPorUsuario(User.Identity!.Name!);
                contrato.UsuarioCreadorId = usuario?.Id ?? 0;

                if (ModelState.IsValid)
                {
                    repo.Alta(contrato);
                    TempData["Mensaje"] = "Contrato creado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                // ⚠️ Si se produce superposición, mostrarlo en la vista
                ModelState.AddModelError("", ex.Message);
            }

            // Recargar listas
            ViewBag.Inmuebles = repoInmueble.ObtenerTodos();
            ViewBag.Inquilinos = repoInquilino.ObtenerTodos();
            return View(contrato);
        }

        // ======================================
        // EDITAR CONTRATO
        // ======================================
        public IActionResult Edit(int id)
        {
            var contrato = repo.ObtenerPorId(id);
            if (contrato == null) return NotFound();

            ViewBag.Inmuebles = repoInmueble.ObtenerTodos();
            ViewBag.Inquilinos = repoInquilino.ObtenerTodos();
            return View(contrato);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Contrato contrato)
        {
            try
            {
                if (id != contrato.Id) return NotFound();

                // Si finaliza el contrato, asignamos al usuario logueado
                var usuario = repoUsuario.ObtenerPorUsuario(User.Identity!.Name!);
                contrato.UsuarioFinalizadorId = usuario?.Id;

                if (ModelState.IsValid)
                {
                    repo.Modificacion(contrato);
                    TempData["Mensaje"] = "Contrato modificado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                // ⚠️ Mostrar error si hay solapamiento de fechas
                ModelState.AddModelError("", ex.Message);
            }

            ViewBag.Inmuebles = repoInmueble.ObtenerTodos();
            ViewBag.Inquilinos = repoInquilino.ObtenerTodos();
            return View(contrato);
        }

        // ======================================
        // ELIMINAR CONTRATO
        // ======================================
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var contrato = repo.ObtenerPorId(id);
            if (contrato == null) return NotFound();
            return View(contrato);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(int id)
        {
            repo.Baja(id);
            TempData["Mensaje"] = "Contrato eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }


        // ======================================
        // FINALIZAR CONTRATO (solo guarda UsuarioFinalizadorId)
        // ======================================
        [Authorize]
        public IActionResult Finalizar(int id)
        {
            var contrato = repo.ObtenerPorId(id);
            if (contrato == null) return NotFound();

            // 🔒 Verificar si ya está finalizado
            if (contrato.UsuarioFinalizador != null)
            {
                TempData["Error"] = "El contrato ya fue finalizado anteriormente.";
                return RedirectToAction(nameof(Index));
            }

            return View(contrato); // mostramos datos del contrato
        }

        [HttpPost]
[ValidateAntiForgeryToken]
[Authorize]
public IActionResult Finalizar(int id, Contrato contrato)
{
    var contratoDb = repo.ObtenerPorId(id);
    if (contratoDb == null) return NotFound();

    if (contratoDb.UsuarioFinalizador != null)
    {
        TempData["Error"] = "El contrato ya está finalizado.";
        return RedirectToAction(nameof(Index));
    }

    var usuario = repoUsuario.ObtenerPorUsuario(User.Identity!.Name!);

    try
    {
        // 🔹 Verificamos si corresponde marcarlo como finalizado normal
        if (DateTime.Today >= contratoDb.FechaFin)
        {
            repo.FinalizarContrato(contratoDb.Id, usuario?.Id ?? 0);
            TempData["Mensaje"] = "Contrato finalizado correctamente (cumplió su plazo).";
        }
        else
        {
            TempData["Error"] = "El contrato aún no llegó a su fecha de fin. Debe usar Finalizar Anticipado.";
        }

        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        TempData["Error"] = ex.Message;
        return RedirectToAction(nameof(Details), new { id });
    }
}


public IActionResult FinalizarAnticipado(int id)
{
    var contrato = repo.ObtenerPorId(id);
    if (contrato == null) return NotFound();

    if (contrato.EstadoContrato != "Vigente")
    {
        TempData["Error"] = "El contrato ya fue finalizado.";
        return RedirectToAction(nameof(Index));
    }

    // Calcular multa y deuda para mostrar en la vista
    double mesesTotales = ((contrato.FechaFin - contrato.FechaInicio).Days) / 30.0;
    double mesesCumplidos = ((DateTime.Today - contrato.FechaInicio).Days) / 30.0;
    contrato.MultaCalculada = mesesCumplidos < mesesTotales / 2 ? contrato.MontoMensual * 2 : contrato.MontoMensual;
    int mesesRestantes = (int)Math.Ceiling(mesesTotales - mesesCumplidos);
    ViewBag.Deuda = mesesRestantes * contrato.MontoMensual;
    ViewBag.Total = ViewBag.Deuda + contrato.MultaCalculada;

    return View(contrato);
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult FinalizarAnticipado(int id, bool pagarAhora)
{
    var contrato = repo.ObtenerPorId(id);
    if (contrato == null) return NotFound();

    var usuario = repoUsuario.ObtenerPorUsuario(User.Identity!.Name!);

    // 🔹 Recalcular multa
    double mesesTotales = ((contrato.FechaFin - contrato.FechaInicio).Days) / 30.0;
    double mesesCumplidos = ((DateTime.Today - contrato.FechaInicio).Days) / 30.0;
    contrato.MultaCalculada = mesesCumplidos < mesesTotales / 2
        ? contrato.MontoMensual * 2
        : contrato.MontoMensual;

    // 🔹 Calcular deuda pendiente
    int mesesRestantes = (int)Math.Ceiling(mesesTotales - mesesCumplidos);
    decimal deuda = mesesRestantes * contrato.MontoMensual;

    // 🔹 Calcular total (deuda + multa)
    decimal total = deuda + (contrato.MultaCalculada ?? 0);

    // 🔹 Guardar finalización anticipada
    repo.FinalizarAnticipado(id, DateTime.Today, usuario?.Id ?? 0);

    if (pagarAhora)
    {
        // Registrar el pago con el TOTAL (deuda + multa)
        var pagoRepo = new RepositorioPago(_configuration);
        pagoRepo.Alta(new Pago
        {
            ContratoId = contrato.Id,
            Importe = total, // 👈 ahora se guarda el importe correcto
            Concepto = $"Finalización anticipada - Alquiler pendiente + Multa",
            FechaPago = DateTime.Today,
            UsuarioCreadorId = usuario?.Id ?? 0,
            Estado = "Pagado"
        });
    }

    TempData["Mensaje"] = "Contrato finalizado anticipadamente.";
    return RedirectToAction(nameof(Index));
}



    }
}
