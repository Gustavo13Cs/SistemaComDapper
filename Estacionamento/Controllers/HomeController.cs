using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ui.Controllers;

public class HomeController : Controller
{ 
    public IActionResult Index()
    {
        return View();
    }
}
