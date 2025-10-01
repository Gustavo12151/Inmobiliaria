using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Inmobiliaria.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class PagosController : Controller
    {
        private readonly RepositorioPago repoPago;
        private readonly RepositorioContrato repoContrato;
        private readonly RepositorioUsuario repoUsuario;

        public PagosController(IConfiguration configuration)
        {
            repoPago = new RepositorioPago(configuration);
            repoContrato = new RepositorioContrato(configuration);
            repoUsuario = new RepositorioUsuario(configuration);
        }

        // INDEX
        public IActionResult Index(int? contratoId)
        {
            var lista = contratoId.HasValue
                ? repoPago.ObtenerTodosPorContrato(contratoId.Value)
                : repoPago.ObtenerTodos();

            ViewBag.Contrato = contratoId.HasValue ? repoContrato.ObtenerPorId(contratoId.Value) : null;
            return View(lista);
        }

        public IActionResult Details(int id)
        {
            var pago = repoPago.ObtenerPorId(id);
            if (pago == null) return NotFound();
            return View(pago);
        }

        // CREATE
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Create()
        {
            var contratosVigentes = repoContrato.ObtenerVigentes();

            // Armamos la lista para el select
            var contratosParaSelect = contratosVigentes.Select(c => new
            {
                Id = c.Id,
                Descripcion = $"Contrato {c.Id} - Inmueble: {c.Inmueble?.Direccion ?? "Sin info"} - " +
                              $"Inquilino: {c.Inquilino?.Nombre ?? "Sin info"} {c.Inquilino?.Apellido ?? ""} - " +
                              $"{c.FechaInicio:dd/MM/yyyy} a {c.FechaFin:dd/MM/yyyy}"
            }).ToList();

            ViewBag.Contratos = new SelectList(contratosParaSelect, "Id", "Descripcion");

            return View(new Pago());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Create(Pago pago)
        {
            if (!ModelState.IsValid)
            {
                var contratosVigentes = repoContrato.ObtenerVigentes();
                var contratosParaSelect = contratosVigentes.Select(c => new
                {
                    Id = c.Id,
                    Descripcion = $"Contrato {c.Id} - Inmueble: {c.Inmueble?.Direccion ?? "Sin info"} - " +
                                  $"Inquilino: {c.Inquilino?.Nombre ?? "Sin info"} {c.Inquilino?.Apellido ?? ""} - " +
                                  $"{c.FechaInicio:dd/MM/yyyy} a {c.FechaFin:dd/MM/yyyy}"
                }).ToList();

                ViewBag.Contratos = new SelectList(contratosParaSelect, "Id", "Descripcion", pago.ContratoId);
                return View(pago);
            }

            var usuario = repoUsuario.ObtenerPorUsuario(User.Identity!.Name!);
            if (usuario == null)
            {
                ModelState.AddModelError("", "No se pudo determinar el usuario actual.");
                return View(pago);
            }

            pago.UsuarioCreadorId = usuario.Id;
            pago.FechaPago = DateTime.Now;
            repoPago.Alta(pago);

            TempData["Mensaje"] = "Pago registrado correctamente.";
            return RedirectToAction(nameof(Index), new { contratoId = pago.ContratoId });
        }

        // EDIT
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Edit(int id)
        {
            var pago = repoPago.ObtenerPorId(id);
            if (pago == null || pago.Estado == "Anulado") return NotFound();
            return View(pago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Edit(int id, Pago pago)
        {
            if (id != pago.Id) return NotFound();
            if (!ModelState.IsValid) return View(pago);

            var pagoOriginal = repoPago.ObtenerPorId(id);
            if (pagoOriginal == null || pagoOriginal.Estado == "Anulado") return NotFound();

            // Solo se puede modificar el concepto
            pagoOriginal.Concepto = pago.Concepto;
            repoPago.Modificacion(pagoOriginal);

            TempData["Mensaje"] = "Concepto modificado correctamente.";
            return RedirectToAction(nameof(Index), new { contratoId = pagoOriginal.ContratoId });
        }

        // DELETE
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var pago = repoPago.ObtenerPorId(id);
            if (pago == null) return NotFound();
            return View(pago);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(int id)
        {
            var usuario = repoUsuario.ObtenerPorUsuario(User.Identity!.Name!);
            if (usuario == null)
            {
                TempData["Error"] = "No se pudo determinar el usuario logueado.";
                return RedirectToAction(nameof(Index));
            }

            repoPago.Anular(id, usuario.Id);
            TempData["Mensaje"] = "Pago anulado correctamente.";

            var pago = repoPago.ObtenerPorId(id);
            return RedirectToAction(nameof(Index), new { contratoId = pago?.ContratoId });
        }
    }
}
