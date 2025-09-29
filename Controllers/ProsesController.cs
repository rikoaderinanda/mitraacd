using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using mitraacd.Models;

namespace mitraacd.Controllers;

public class ProsesController : Controller
{
    private readonly ILogger<TaskController> _logger;

    public ProsesController(ILogger<TaskController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
