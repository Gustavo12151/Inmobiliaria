using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using System.Linq;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class PagosController : Controller
    {
        private readonly RepositorioPago repo;
        private readonly RepositorioContrato repoContrato;
        private readonly RepositorioUsuario repoUsuario;

        public PagosController(IConfiguration configuration)
        {
            repo = new RepositorioPago(configuration);
            repoContrato = new RepositorioContrato(configuration);
            repoUsuario = new RepositorioUsuario(configuration);
        }

        // ================================
        // LISTADO
        // ================================
        public IActionResult Index()
        {
            var lista = repo.ObtenerTodos();
            return View(lista);
        }

        public IActionResult Details(int id)
        {
            var pago = repo.ObtenerPorId(id);
            if (pago == null) return NotFound();
            return View(pago);
        }

        // ================================
        // CREATE
        // ================================
        public IActionResult Create()
        {
            var usuario = repoUsuario.ObtenerPorUsuario(User.Identity!.Name!);
            ViewBag.UsuarioActual = usuario?.NombreUsuario;

            var contratos = repoContrato.ObtenerTodos()
                .Select(c => new
                {
                    Id = c.Id,
                    Descripcion = $"{c.Inmueble.Direccion} - {c.Inquilino.Apellido}, {c.Inquilino.Nombre}"
                }).ToList();

            ViewBag.Contratos = contratos;

            return View();
        }

        [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Create(Pago pago)
{
    if (!ModelState.IsValid)
    {
        // Recargar combos si falla la validación
        var contratos = repoContrato.ObtenerTodos()
            .Select(c => new {
                Id = c.Id,
                Descripcion = $"{c.Inmueble.Direccion} - {c.Inquilino.Apellido}, {c.Inquilino.Nombre}"
            }).ToList();

        ViewBag.Contratos = contratos;
        return View(pago);
    }

    // Recuperar el usuario actual por su nombre
    var usuario = repoUsuario.ObtenerPorUsuario(User.Identity!.Name!);
    if (usuario == null)
    {
        ModelState.AddModelError("", "No se pudo determinar el usuario actual.");
        return View(pago);
    }

    pago.UsuarioCreadorId = usuario.Id;

    repo.Alta(pago);
    TempData["Mensaje"] = "Pago registrado correctamente.";
    return RedirectToAction(nameof(Index));
}


        // ================================
        // EDIT
        // ================================
        public IActionResult Edit(int id)
        {
            var pago = repo.ObtenerPorId(id);
            if (pago == null) return NotFound();

            var contratos = repoContrato.ObtenerTodos()
                .Select(c => new
                {
                    Id = c.Id,
                    Descripcion = $"{c.Inmueble.Direccion} - {c.Inquilino.Apellido}, {c.Inquilino.Nombre}"
                }).ToList();

            ViewBag.Contratos = contratos;

            return View(pago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Pago pago)
        {
            if (id != pago.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Contratos = repoContrato.ObtenerTodos()
                    .Select(c => new
                    {
                        Id = c.Id,
                        Descripcion = $"{c.Inmueble.Direccion} - {c.Inquilino.Apellido}, {c.Inquilino.Nombre}"
                    }).ToList();

                return View(pago);
            }

            repo.Modificacion(pago);
            TempData["Mensaje"] = "Pago modificado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ================================
        // DELETE
        // ================================
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var pago = repo.ObtenerPorId(id);
            if (pago == null) return NotFound();
            return View(pago);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(int id)
        {
            int usuarioId = int.Parse(User.Claims.First(c => c.Type == "Id").Value);
            repo.Anular(id, usuarioId);
            TempData["Mensaje"] = "Pago anulado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
