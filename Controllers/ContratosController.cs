using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using System;
using Microsoft.Extensions.Configuration; // Necesario para IConfiguration
using System.Linq; // Necesario para .ToList() si RepositorioInmueble retorna IEnumerable

namespace Inmobiliaria.Controllers
{
    // Solo usuarios autenticados pueden acceder a esta clase
    [Authorize]
    public class ContratosController : Controller
    {
        private readonly RepositorioContrato repo;
        private readonly RepositorioInmueble repoInmueble; // Usamos el nombre más descriptivo de la primera versión

        public ContratosController(IConfiguration configuration)
        {
            repo = new RepositorioContrato(configuration);
            repoInmueble = new RepositorioInmueble(configuration);
        }

        // ======================================
        // LECTURA (READ)
        // ======================================

        public IActionResult Index()
        {
            var lista = repo.ObtenerTodos();
            return View(lista);
        }

        public IActionResult Details(int id)
        {
            var contrato = repo.ObtenerPorId(id);
            // Se asume que el método ObtenerPorId de RepositorioContrato ya carga Inquilino e Inmueble
            // o que la vista maneja la carga de datos relacionados.
            if (contrato == null) return NotFound();
            return View(contrato);
        }

        // ======================================
        // CREACIÓN (CREATE)
        // ======================================

      
        public IActionResult Create()
        {
            // Se usa la lógica de carga de inmuebles de la primera versión
            ViewBag.Inmuebles = repoInmueble.ObtenerDisponibles();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public IActionResult Create(Contrato contrato)
        {
            // La lógica de validación de la segunda versión es mejor, pero la gestión de errores
            // con try-catch de la primera versión es más robusta.
            try
            {
                // Se asigna el usuario creador antes de la validación del modelo
                contrato.UsuarioCreador = User.Identity.Name;

                if (ModelState.IsValid)
                {
                    // Lógica de validación de superposición de la segunda versión
                    // Si el repositorio ya lanza una excepción por superposición (como se vio antes),
                    // el bloque catch la gestionará.
                    repo.Alta(contrato);
                    
                    TempData["Mensaje"] = "Contrato creado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                // Manejar la excepción de superposición de fechas o cualquier otra de la persistencia
                ModelState.AddModelError("", ex.Message);
            }

            // Recargar listas necesarias y retornar a la vista con el modelo
            ViewBag.Inmuebles = repoInmueble.ObtenerDisponibles();
            return View(contrato);
        }

        // ======================================
        // EDICIÓN (UPDATE)
        // ======================================

        
        public IActionResult Edit(int id)
        {
            var contrato = repo.ObtenerPorId(id);
            if (contrato == null) return NotFound();
            
            // La primera versión carga todos los inmuebles, lo que permite cambiarlo a uno ocupado (por si el contrato es el que lo ocupa)
            ViewBag.Inmuebles = repoInmueble.ObtenerTodos();
            return View(contrato);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
 
        public IActionResult Edit(int id, Contrato contrato)
        {
            try
            {
                // Se asigna el usuario terminador antes de la validación
                contrato.UsuarioTerminador = User.Identity.Name;

                if (id != contrato.Id) return NotFound();
                
                if (ModelState.IsValid)
                {
                    // Lógica de validación de superposición de la segunda versión
                    // El repositorio debe validar con el id excluido, si lanza excepción, el catch la maneja.
                    repo.Modificacion(contrato);
                    
                    TempData["Mensaje"] = "Contrato modificado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                // Manejar la excepción de superposición de fechas o cualquier otra de la persistencia
                ModelState.AddModelError("", ex.Message);
            }

            // Recargar listas necesarias y retornar a la vista con el modelo
            ViewBag.Inmuebles = repoInmueble.ObtenerTodos();
            return View(contrato);
        }

        // ======================================
        // ELIMINACIÓN (DELETE)
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