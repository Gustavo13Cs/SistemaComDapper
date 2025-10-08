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
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string senha)
        {
            var usuario = _authService.Login(email, senha);

            if (usuario != null)
            {
                TempData["Sucesso"] = $"Bem-vindo, {usuario.NomeCompleto}!";
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
    }
}
