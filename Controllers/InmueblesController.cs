using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Inmobiliaria.Controllers
{
    [Authorize] // Todos los usuarios autenticados pueden acceder
    public class InmueblesController : Controller
    {
        private readonly RepositorioInmueble repo;

        public InmueblesController(IConfiguration configuration)
        {
            repo = new RepositorioInmueble(configuration);
        }

        // ======================================
        // MÉTODOS DE LECTURA (READ)
        // ======================================

        public IActionResult Index()
        {
            var lista = repo.ObtenerTodos();
            return View(lista);
        }

        // Listado solo de inmuebles disponibles (sin contrato activo)
        public IActionResult Disponibles()
        {
            var lista = repo.ObtenerDisponibles();
            return View("Index", lista);
        }

        // Listado de inmuebles no ocupados entre fechas
        [HttpPost]
        public IActionResult NoOcupados(DateTime inicio, DateTime fin)
        {
            var lista = repo.ObtenerNoOcupadosEntreFechas(inicio, fin);
            ViewBag.FiltroActivo = true;
            ViewBag.FechaInicio = inicio.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fin.ToString("yyyy-MM-dd");
            return View("Index", lista);
        }

        // Detalle de un inmueble
        public IActionResult Details(int id)
        {
            var inmueble = repo.ObtenerPorId(id);
            if (inmueble == null) return NotFound();
            return View(inmueble);
        }

        // ======================================
        // MÉTODOS DE CREACIÓN (CREATE)
        // ======================================

        [Authorize]
        public IActionResult Create()
        {
            ViewBag.Propietarios = new SelectList(repo.ObtenerPropietarios(), "Id", "NombreCompleto"); // Id y NombreCompleto
    ViewBag.Tipos = new SelectList(repo.ObtenerTipos(), "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Create(Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                repo.Alta(inmueble);
                TempData["Mensaje"] = "Inmueble creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(inmueble);
        }

        // ======================================
        // MÉTODOS DE EDICIÓN (UPDATE)
        // ======================================

        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id)
        {
            var inmueble = repo.ObtenerPorId(id);
            if (inmueble == null) return NotFound();
            return View(inmueble);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id, Inmueble inmueble)
        {
            if (id != inmueble.Id) return NotFound();

            if (ModelState.IsValid)
            {
                inmueble.Id = id;
                repo.Modificacion(inmueble);
                TempData["Mensaje"] = "Inmueble modificado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(inmueble);
        }

        // ======================================
        // MÉTODOS DE ELIMINACIÓN (DELETE)
        // ======================================

        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var inmueble = repo.ObtenerPorId(id);
            if (inmueble == null) return NotFound();
            return View(inmueble);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(int id)
        {
            repo.Baja(id);
            TempData["Mensaje"] = "Inmueble eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
