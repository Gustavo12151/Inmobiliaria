using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Inmobiliaria.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class PagosController : Controller
    {
        private readonly RepositorioPago repoPago;
        private readonly RepositorioContrato repoContrato;
        private readonly RepositorioUsuario repoUsuario;

        public PagosController(IConfiguration configuration)
        {
            // Usar el IConfiguration que te provee el framework
            repoPago = new RepositorioPago(configuration);
            repoContrato = new RepositorioContrato(configuration);
            repoUsuario = new RepositorioUsuario(configuration);
        }

        // INDEX: muestra todos o por contrato si se pasa contratoId
        public IActionResult Index(int? contratoId)
        {
            var lista = contratoId.HasValue
                ? repoPago.ObtenerTodosPorContrato(contratoId.Value)   // requiere implementar en el repo
                : repoPago.ObtenerTodos();

            ViewBag.Contrato = contratoId.HasValue ? repoContrato.ObtenerPorId(contratoId.Value) : null;
            return View(lista);
        }

        public IActionResult Details(int id)
        {
            var pago = repoPago.ObtenerPorId(id);
            if (pago == null) return NotFound();
            return View(pago);
        }

        // CREATE
        [Authorize(Roles = "Administrador,Empleado")]
public IActionResult Create()
{
    // Obtener solo contratos vigentes
    var contratosVigentes = repoContrato.ObtenerVigentes();

    // Crear SelectList para la vista
    ViewBag.Contratos = new SelectList(
        contratosVigentes, 
        "Id",       // valor de la opción
        "Id"        // texto que se muestra, puedes poner otra propiedad como "Numero" o "Inquilino.Nombre"
    );

    return View(new Pago());
}

[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Administrador,Empleado")]
public IActionResult Create(Pago pago)
{
    if (!ModelState.IsValid)
    {
        // Volver a llenar la lista si hay error
        var contratosVigentes = repoContrato.ObtenerVigentes();
        ViewBag.Contratos = new SelectList(contratosVigentes, "Id", "Id", pago.ContratoId);
        return View(pago);
    }

    var usuario = repoUsuario.ObtenerPorUsuario(User.Identity!.Name!);
    if (usuario == null)
    {
        ModelState.AddModelError("", "No se pudo determinar el usuario actual.");
        var contratosVigentes = repoContrato.ObtenerVigentes();
        ViewBag.Contratos = new SelectList(contratosVigentes, "Id", "Id", pago.ContratoId);
        return View(pago);
    }

    pago.UsuarioCreadorId = usuario.Id;
    pago.FechaPago = DateTime.Now;
    repoPago.Alta(pago); // el repo calcula NumeroPago automáticamente

    TempData["Mensaje"] = "Pago registrado correctamente.";
    return RedirectToAction(nameof(Index), new { contratoId = pago.ContratoId });
}



        // EDIT (solo Admin y Empleado)
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Edit(int id)
        {
            var pago = repoPago.ObtenerPorId(id);
            if (pago == null || pago.Estado == "Anulado") return NotFound();
            return View(pago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Edit(int id, Pago pago)
        {
            if (id != pago.Id) return NotFound();
            if (!ModelState.IsValid) return View(pago);

            var original = repoPago.ObtenerPorId(id);
            if (original == null || original.Estado == "Anulado") return NotFound();

            // SOLO modificar Concepto
            original.Concepto = pago.Concepto;
            repoPago.Modificacion(original);

            TempData["Mensaje"] = "Pago modificado correctamente.";
            return RedirectToAction(nameof(Index), new { contratoId = original.ContratoId });
        }

        // DELETE (solo Admin puede anular)
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var pago = repoPago.ObtenerPorId(id);
            if (pago == null) return NotFound();
            return View(pago);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(int id)
        {
            int usuarioId = int.Parse(User.Claims.First(c => c.Type == "Id").Value);
            repoPago.Anular(id, usuarioId);
            var pago = repoPago.ObtenerPorId(id);
            return RedirectToAction(nameof(Index), new { contratoId = pago?.ContratoId });
        }
    }
}
