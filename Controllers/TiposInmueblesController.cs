using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    [Authorize] // ✅ Todos los logueados acceden, el rol se controla en cada acción
    public class TiposInmueblesController : Controller
    {
        private readonly RepositorioTipoInmueble repo;

        public TiposInmueblesController(IConfiguration configuration)
        {
            repo = new RepositorioTipoInmueble(configuration);
        }

        // ✅ Puede ver Admin y Empleado
        public IActionResult Index() => View(repo.ObtenerTodos());

        // ✅ Puede ver Admin y Empleado
        public IActionResult Details(int id)
        {
            var tipo = repo.ObtenerPorId(id);
            if (tipo == null) return NotFound();
            return View(tipo);
        }

        // 🔒 Solo Admin puede crear
        [Authorize(Roles = "Administrador")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult Create(TipoInmueble tipo)
        {
            if (ModelState.IsValid)
            {
                repo.Alta(tipo);
                TempData["Mensaje"] = "Tipo de inmueble creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(tipo);
        }

        // 🔒 Solo Admin puede editar
        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id)
        {
            var tipo = repo.ObtenerPorId(id);
            if (tipo == null) return NotFound();
            return View(tipo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id, TipoInmueble tipo)
        {
            if (id != tipo.Id) return NotFound();
            if (ModelState.IsValid)
            {
                repo.Modificacion(tipo);
                TempData["Mensaje"] = "Tipo de inmueble modificado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(tipo);
        }

        // 🔒 Solo Admin puede eliminar
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var tipo = repo.ObtenerPorId(id);
            if (tipo == null) return NotFound();
            return View(tipo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(int id)
        {
            repo.Baja(id);
            TempData["Mensaje"] = "Tipo de inmueble eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
