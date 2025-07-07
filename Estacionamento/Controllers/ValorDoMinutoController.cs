using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Estacionamento.Models;
using Microsoft.AspNetCore.Mvc;

namespace Estacionamento.Controllers
{
    [Route("/valores")]
    public class ValorDoMinutoController : Controller
    {
        private readonly IDbConnection _connection;
        public ValorDoMinutoController(IDbConnection connection)
        {
            _connection = connection;
        }
        public IActionResult Index()
        {
            var valores = _connection.Query<ValorDoMinuto>("SELECT * FROM Valores");
            return View(valores);
        }
    }
}