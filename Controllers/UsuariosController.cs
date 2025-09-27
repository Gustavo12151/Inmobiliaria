using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;

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
            var usuario = repo.ObtenerPorUsuario(nombreUsuario);

            if (usuario == null || usuario.Clave != clave)
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

    // Mantener el rol
    usuario.Rol = usuarioExistente.Rol;

    // ✅ Si se sube un avatar, reemplazar
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
        // ✅ Si no se sube avatar, conservar el existente
        usuario.Avatar = usuarioExistente.Avatar;
    }

    repo.Modificacion(usuario);

    // Refrescar claims
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

    usuario.Avatar = "/avatars/default.png"; // 🔹 Avatar por defecto
    repo.Modificacion(usuario);

    // Refrescar claims
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
            return View(usuario);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarClave(int id, string claveActual, string nuevaClave)
        {
            var usuario = repo.ObtenerPorId(id);
            if (usuario == null) return NotFound();

            if (usuario.Clave != claveActual)
            {
                TempData["Error"] = "La clave actual es incorrecta";
                return RedirectToAction("CambiarClave", new { id });
            }

            usuario.Clave = nuevaClave;
            repo.Modificacion(usuario);

            TempData["Mensaje"] = "Contraseña cambiada con éxito.";
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

        [Authorize(Roles = "Administrador")]
        public IActionResult Create() => View();

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
            return View(usuario);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id)
        {
            var usuario = repo.ObtenerPorId(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id, Usuario usuario)
        {
            if (id != usuario.Id) return NotFound();
            if (ModelState.IsValid)
            {
                repo.Modificacion(usuario);
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
    }
}
