using System.Diagnostics;
using Estacionamento.Models;
using Estacionamento.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace ui.Controllers;

public class HomeController : Controller
{ 
    private readonly IRepositorio<Vaga> _vagaRepo;

    public HomeController(IRepositorio<Vaga> vagaRepo)
    {
        _vagaRepo = vagaRepo;
    }
    public IActionResult Index()
    {
        List<Vaga> vagas = _vagaRepo.ObterTodos().ToList();
        var total = vagas.Count;
        var ocupadas = vagas.Count(v => v.Ocupada);

        double porcentagem = total > 0 ? (ocupadas * 100.0 / total) : 0;
        ViewBag.PorcentagemOcupada = porcentagem;

        return View();

    }
}
