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

        public PagosController(IConfiguration configuration)
        {
            repo = new RepositorioPago(configuration);
        }

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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pago pago)
        {
            if (!ModelState.IsValid)
                return View(pago);

            int usuarioId;
            try
            {
                usuarioId = int.Parse(User.Claims.First(c => c.Type == "Id").Value);
            }
            catch
            {
                ModelState.AddModelError("", "No se pudo determinar el Id del usuario.");
                return View(pago);
            }

            pago.UsuarioCreadorId = usuarioId;
            repo.Alta(pago);
            TempData["Mensaje"] = "Pago registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var pago = repo.ObtenerPorId(id);
            if (pago == null) return NotFound();
            return View(pago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Pago pago)
        {
            if (id != pago.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(pago);

            repo.Modificacion(pago);
            TempData["Mensaje"] = "Pago modificado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET Delete: solo para confirmación si quisieras
        public IActionResult Delete(int id)
        {
            var pago = repo.ObtenerPorId(id);
            if (pago == null) return NotFound();
            return View(pago);
        }

        // POST Delete -> Anular pago
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            int usuarioId = int.Parse(User.Claims.First(c => c.Type == "Id").Value);
            repo.Anular(id, usuarioId);
            TempData["Mensaje"] = "Pago anulado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
