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

        public ContratosController(IConfiguration configuration)
        {
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

    // 🔒 Bloqueo doble finalización
    if (contratoDb.UsuarioFinalizador != null)
    {
        TempData["Error"] = "El contrato ya está finalizado.";
        return RedirectToAction(nameof(Index));
    }

    // Guardamos quién finalizó el contrato
    var usuario = repoUsuario.ObtenerPorUsuario(User.Identity!.Name!);
    contratoDb.UsuarioFinalizadorId = usuario?.Id;

    try
    {
        repo.Modificacion(contratoDb);
        TempData["Mensaje"] = "Contrato finalizado correctamente.";
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        TempData["Error"] = ex.Message;
        return RedirectToAction(nameof(Details), new { id });
    }
}


    }
}
