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
            var lista = repo.ObtenerTodos();
            return View(lista);
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
            ViewBag.Inmuebles = repoInmueble.ObtenerDisponibles();
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
                ModelState.AddModelError("", ex.Message);
            }

            // Recargar listas
            ViewBag.Inmuebles = repoInmueble.ObtenerDisponibles();
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
    }
}
    