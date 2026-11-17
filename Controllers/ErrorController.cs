using Microsoft.AspNetCore.Mvc;
using SistemaPastelaria.Models;

namespace SistemaPastelaria.Controllers;

public class ErrorController : Controller
{
    [AcceptVerbs("GET", "POST", "PUT", "DELETE")]
    [Route("/Home/Error")]
    public IActionResult Index()
    {
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
        {
            RequestId = HttpContext.TraceIdentifier
        });
    }
}
