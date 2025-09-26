using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System;

namespace Inmobiliaria.Controllers
{
    // Todos los usuarios autenticados pueden acceder a esta clase
    [Authorize]
    public class InmueblesController : Controller
    {
        private readonly RepositorioInmueble repo;

        public InmueblesController(IConfiguration configuration)
        {
            // Se asume que RepositorioInmueble existe y RepositorioBase está configurado
            repo = new RepositorioInmueble(configuration);
        }

        // ======================================
        // MÉTODOS DE LECTURA (READ)
        // ======================================

        // Listado completo de inmuebles
        public IActionResult Index()
        {
            var lista = repo.ObtenerTodos();
            return View(lista);
        }

        // Listado solo de inmuebles disponibles (sin contrato activo)
        public IActionResult Disponibles()
        {
            var lista = repo.ObtenerDisponibles();
            // La segunda versión sugiere retornar a la vista "Index" para unificar la presentación
            return View("Index", lista);
        }

        // Listado de inmuebles no ocupados entre fechas (combinación de ObtenerNoOcupadosEntreFechas de la primera versión)
        [HttpPost] // Se usa POST para el formulario de filtrado por fechas
        public IActionResult NoOcupados(DateTime inicio, DateTime fin)
        {
            // Se usa el nombre del método del segundo bloque, pero con la lógica del filtro por POST del primero.
            var lista = repo.ObtenerNoOcupadosEntreFechas(inicio, fin);
            // La segunda versión sugiere retornar a la vista "Index" para unificar la presentación
            ViewBag.FiltroActivo = true; // Indicador para la vista si se desea mostrar info de fechas
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

        // Vista de formulario de alta (solo Administradores)
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        // Procesamiento del alta (solo Administradores)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
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

        // Vista de formulario de edición (solo Administradores)
        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id)
        {
            var inmueble = repo.ObtenerPorId(id);
            if (inmueble == null) return NotFound();
            return View(inmueble);
        }

        // Procesamiento de la edición (solo Administradores)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id, Inmueble inmueble)
        {
            if (id != inmueble.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Asegurar que el ID del inmueble en el modelo es el correcto
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

        // Vista de confirmación de baja (solo Administradores)
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var inmueble = repo.ObtenerPorId(id);
            if (inmueble == null) return NotFound();
            return View(inmueble);
        }

        // Procesamiento de la baja (solo Administradores)
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