using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Inmobiliaria.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly RepositorioUsuario repo;

        public UsuariosController(IConfiguration configuration)
        {
            repo = new RepositorioUsuario(configuration);
        }

        // =========================
        // LOGIN / LOGOUT
        // =========================
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string nombreUsuario, string clave)
        {
            if (!repo.VerificarLogin(nombreUsuario, clave, out var usuario) || usuario == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.NombreUsuario ?? ""),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Role, usuario.Rol ?? "Empleado"),
                new Claim("Avatar", usuario.Avatar ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );

            TempData["Mensaje"] = "Has iniciado sesión correctamente.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccesoDenegado() => View();

        // =========================
        // PERFIL DE USUARIO
        // =========================
        [Authorize]
        public IActionResult Perfil(int? id)
        {
            Usuario? usuario;

            if (User.IsInRole("Administrador") && id.HasValue)
            {
                usuario = repo.ObtenerPorId(id.Value);
            }
            else
            {
                usuario = repo.ObtenerPorUsuario(User.Identity!.Name!);
            }

            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(Usuario usuario, IFormFile? avatarFile)
        {
            if (usuario.Id <= 0) return NotFound();

            var usuarioExistente = repo.ObtenerPorId(usuario.Id);
            if (usuarioExistente == null) return NotFound();

            usuario.Rol = usuarioExistente.Rol;

            if (avatarFile != null && avatarFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(avatarFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }
                usuario.Avatar = "/avatars/" + fileName;
            }
            else
            {
                usuario.Avatar = usuarioExistente.Avatar;
            }

            repo.Modificacion(usuario);

            // 🔄 refrescar claims
            var claims = new List<Claim>
            {
                  new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario ?? ""),
                new Claim(ClaimTypes.Role, usuario.Rol ?? "Empleado"),
                new Claim("Avatar", usuario.Avatar ?? "")
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );

            TempData["Mensaje"] = "Perfil actualizado con éxito";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarAvatar(int id)
        {
            var usuario = repo.ObtenerPorId(id);
            if (usuario == null) return NotFound();

            usuario.Avatar = "/avatars/default.png";
            repo.Modificacion(usuario);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.NombreUsuario ?? ""),
                new Claim(ClaimTypes.Role, usuario.Rol ?? "Empleado"),
                new Claim("Avatar", usuario.Avatar ?? "")
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );

            TempData["Mensaje"] = "Avatar eliminado correctamente";
            return RedirectToAction("Index", "Home");
        }

       [Authorize]
[HttpGet]
public IActionResult CambiarClave(int id)
{
    var usuario = repo.ObtenerPorId(id);
    if (usuario == null) return NotFound();

    // Si no es admin, solo puede cambiar su propia clave
    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!User.IsInRole("Administrador") && currentUserId != usuario.Id.ToString())
    {
        return Forbid(); // Acceso denegado
    }

    return View(usuario);
}

[Authorize]
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult CambiarClave(int id, string claveActual, string nuevaClave, string confirmarClave)
{
    var usuario = repo.ObtenerPorId(id);
    if (usuario == null) return NotFound();

    // Validación de que solo pueda cambiar su propia clave si no es admin
    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (!User.IsInRole("Administrador") && currentUserId != usuario.Id.ToString())
    {
        return Forbid();
    }

    // Validaciones de nueva contraseña
    if (string.IsNullOrEmpty(nuevaClave) || string.IsNullOrEmpty(confirmarClave))
    {
        TempData["Error"] = "Debe ingresar y confirmar la nueva contraseña.";
        return RedirectToAction("CambiarClave", new { id });
    }

    if (nuevaClave != confirmarClave)
    {
        TempData["Error"] = "La nueva contraseña y la confirmación no coinciden.";
        return RedirectToAction("CambiarClave", new { id });
    }

    // Verificar la clave actual
    bool claveValida = false;
    try
    {
        if (!string.IsNullOrEmpty(usuario.Clave) && usuario.Clave.StartsWith("$2"))
        {
            claveValida = BCrypt.Net.BCrypt.Verify(claveActual, usuario.Clave);
        }
        else
        {
            claveValida = usuario.Clave == claveActual;
        }
    }
    catch
    {
        claveValida = false;
    }

    if (!claveValida)
    {
        TempData["Error"] = "La clave actual es incorrecta.";
        return RedirectToAction("CambiarClave", new { id });
    }

    // Guardar la nueva clave
    usuario.Clave = BCrypt.Net.BCrypt.HashPassword(nuevaClave);
    repo.CambiarClave(id, nuevaClave);

    TempData["Mensaje"] = "Contraseña modificada correctamente.";
    return RedirectToAction("Perfil", new { id });
}



        // =========================
        // CRUD DE USUARIOS (solo admin)
        // =========================
        [Authorize(Roles = "Administrador")]
        public IActionResult Index() => View(repo.ObtenerTodos());

        [Authorize(Roles = "Administrador")]
        public IActionResult Details(int id)
        {
            var usuario = repo.ObtenerPorId(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // 🔹 MODIFICADO: Create GET agrega ViewBag.Roles
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(new List<string> { "Empleado", "Administrador" }); // 🔹 MODIFICADO
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult Create(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                repo.Alta(usuario);
                return RedirectToAction(nameof(Index));
            }
            // 🔹 MODIFICADO: si hay error, recargar roles
            ViewBag.Roles = new SelectList(new List<string> { "Empleado", "Administrador" }); // 🔹 MODIFICADO
            return View(usuario);
        }

        // =========================
// ADMIN EDITA EMPLEADOS
// =========================
[Authorize(Roles = "Administrador")]
public IActionResult EditEmpleado(int id)
{
    var usuario = repo.ObtenerPorId(id);
    if (usuario == null) return NotFound();
    return View(usuario);
}

[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Administrador")]
public IActionResult EditEmpleado(int id, Usuario usuario, string? nuevaClave)
{
    if (id != usuario.Id) return NotFound();

    var usuarioExistente = repo.ObtenerPorId(id);
    if (usuarioExistente == null) return NotFound();

    // Mantener avatar y clave si no se modifican
    usuario.Avatar = usuarioExistente.Avatar;

    if (!string.IsNullOrWhiteSpace(nuevaClave))
    {
        usuario.Clave = BCrypt.Net.BCrypt.HashPassword(nuevaClave);
    }
    else
    {
        usuario.Clave = usuarioExistente.Clave;
    }

    if (ModelState.IsValid)
    {
        repo.Modificacion(usuario);
        TempData["Mensaje"] = "Empleado actualizado con éxito.";
        return RedirectToAction(nameof(Index));
    }

    return View(usuario);
}


        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var usuario = repo.ObtenerPorId(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(int id)
        {
            repo.Baja(id);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrador")]
[HttpGet]
public IActionResult EditarEmpleado(int id)
{
    var usuario = repo.ObtenerPorId(id);
    if (usuario == null) return NotFound();
    return View(usuario);
}

[Authorize(Roles = "Administrador")]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditarEmpleado(Usuario usuario, IFormFile? avatarFile, string? nuevaClave)
{
    var usuarioExistente = repo.ObtenerPorId(usuario.Id);
    if (usuarioExistente == null) return NotFound();

    // ✅ Mantener avatar si no se sube nuevo
    if (avatarFile != null && avatarFile.Length > 0)
    {
        var fileName = Guid.NewGuid() + Path.GetExtension(avatarFile.FileName);
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars", fileName);
        using (var stream = new FileStream(path, FileMode.Create))
        {
            await avatarFile.CopyToAsync(stream);
        }
        usuario.Avatar = "/avatars/" + fileName;
    }
    else
    {
        usuario.Avatar = usuarioExistente.Avatar;
    }

    // ✅ Contraseña: cifrar solo si el admin escribió algo
    if (!string.IsNullOrEmpty(nuevaClave))
    {
        usuario.Clave = BCrypt.Net.BCrypt.HashPassword(nuevaClave);
    }
    else
    {
        usuario.Clave = null; // Para que el repo no la modifique
    }

    repo.Modificacion(usuario);

    TempData["Mensaje"] = "Empleado actualizado correctamente.";
    return RedirectToAction(nameof(Index));
}

    }
}
