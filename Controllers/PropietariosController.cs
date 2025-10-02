using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class PropietariosController : Controller
    {
        private readonly RepositorioPropietario repo;
        private readonly RepositorioInmueble repoInmueble;


        public PropietariosController(IConfiguration configuration)
        {
            repo = new RepositorioPropietario(configuration);
            repoInmueble = new RepositorioInmueble(configuration);

        }

        public IActionResult Index()
        {
            var lista = repo.ObtenerTodos();
            return View(lista);
        }

        public IActionResult Details(int id)
        {
            var p = repo.ObtenerPorId(id);
            if (p == null) return NotFound();
            return View(p);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Propietario p)
        {
            if (ModelState.IsValid)
            {
                repo.Alta(p);
                return RedirectToAction(nameof(Index));
            }
            return View(p);
        }

        public IActionResult Edit(int id)
        {
            var p = repo.ObtenerPorId(id);
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Propietario p)
        {
            if (id != p.Id) return NotFound();
            if (ModelState.IsValid)
            {
                repo.Modificacion(p);
                return RedirectToAction(nameof(Index));
            }
            return View(p);
        }
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var p = repo.ObtenerPorId(id);
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            repo.Baja(id);
            return RedirectToAction(nameof(Index));
        }
        

        [Authorize(Roles = "Administrador")]
public IActionResult Baja(int id)
{
    var propietario = repo.ObtenerPorId(id);
    if (propietario == null) return NotFound();
    return View(propietario);
}

        [HttpPost, ActionName("Baja")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult BajaConfirmada(int id)
        {
            repo.Baja(id);
            TempData["Mensaje"] = "Propietario dado de baja y sus inmuebles marcados como Indisponibles.";
            return RedirectToAction(nameof(Index));
        }
[Authorize(Roles = "Administrador")]
public IActionResult CambiarEstado(int id)
{
    var propietario = repo.ObtenerPorId(id);
    if (propietario == null) return NotFound();

    // Cambiar estado
    string nuevoEstado = propietario.Estado == "Activo" ? "Inactivo" : "Activo";
    repo.CambiarEstado(id, nuevoEstado);

    // Si se da de baja -> marcar inmuebles como indisponibles
    if (nuevoEstado == "Inactivo")
    {
        repoInmueble.MarcarInmueblesComoIndisponibles(id);
    }

    TempData["Mensaje"] = $"Propietario {nuevoEstado} correctamente.";
    return RedirectToAction(nameof(Index));
}


    }
}
