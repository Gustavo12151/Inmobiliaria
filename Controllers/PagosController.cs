using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

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

        // GET: Pagos
        public IActionResult Index()
        {
            var lista = repo.ObtenerTodos();
            bool esAdmin = User.IsInRole("Administrador");
            ViewBag.EsAdmin = esAdmin;
            return View(lista);
        }

        // GET: Pagos/Details/5
        public IActionResult Details(int id)
        {
            var pago = repo.ObtenerPorId(id);
            if (pago == null) return NotFound();
            return View(pago);
        }

        // GET: Pagos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pago pago)
        {
            if (!ModelState.IsValid) return View(pago);

            // Obtenemos el Id del usuario logueado desde los claims
            var claimId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;

            if (string.IsNullOrEmpty(claimId))
            {
                ModelState.AddModelError("", "No se pudo determinar el Id del usuario logueado.");
                return View(pago);
            }

            pago.UsuarioCreadorId = int.Parse(claimId);

            repo.Alta(pago);
            return RedirectToAction(nameof(Index));
        }

        // GET: Pagos/Edit/5
        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id)
        {
            var pago = repo.ObtenerPorId(id);
            if (pago == null) return NotFound();
            return View(pago);
        }

        // POST: Pagos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id, Pago pago)
        {
            if (id != pago.Id) return NotFound();
            if (!ModelState.IsValid) return View(pago);

            repo.Modificacion(pago);
            return RedirectToAction(nameof(Index));
        }

        // GET: Pagos/Delete/5  → Anular pago
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var pago = repo.ObtenerPorId(id);
            if (pago == null) return NotFound();
            return View(pago);
        }

        // POST: Pagos/Delete/5 → Anular pago
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(int id)
        {
            // Registramos quién anuló el pago
            var pago = repo.ObtenerPorId(id);
            if (pago != null)
            {
                var claimId = User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
                if (!string.IsNullOrEmpty(claimId))
                {
                    pago.UsuarioAnuladorId = int.Parse(claimId);
                    repo.Modificacion(pago); // Guardamos el usuario que anuló
                }

                // Opción: también podrías borrar si tu lógica lo permite
                // repo.Baja(id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
