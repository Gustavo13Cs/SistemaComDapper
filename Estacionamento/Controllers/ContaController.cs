using Estacionamento.Models;
using Estacionamento.Services;
using Microsoft.AspNetCore.Mvc;

namespace Estacionamento.Controllers
{
    public class ContaController : Controller
    {
        private readonly AuthService _authService;

        public ContaController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Se já estiver logado, vai direto pra Home
            if (HttpContext.Session.GetInt32("UsuarioId") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string senha, bool manterConectado)
        {
            var usuario = _authService.Login(email, senha);

            if (usuario != null)
            {
                HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                HttpContext.Session.SetString("UsuarioNome", usuario.NomeCompleto);
                HttpContext.Session.SetString("UsuarioPerfil", usuario.Perfil);

                TempData["Sucesso"] = $"Bem-vindo, {usuario.NomeCompleto}!";

                if (manterConectado)
                {
                    Response.Cookies.Append("UsuarioEmail", email, new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    });
                }

                return RedirectToAction("Index", "Home");
            }

            TempData["Erro"] = "E-mail ou senha inválidos.";
            return View();
        }

        [HttpGet]
        public IActionResult Registrar() => View();

        [HttpPost]
        public IActionResult Registrar(Usuario usuario)
        {
            if (_authService.Registrar(usuario))
                return RedirectToAction("Login");

            TempData["Erro"] = "Erro ao registrar usuário.";
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("UsuarioEmail");
            return RedirectToAction("Login");
        }
    }
}
