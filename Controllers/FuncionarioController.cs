using Microsoft.AspNetCore.Mvc;
using Supabase;
using SistemaPastelaria.Models;

namespace SistemaPastelaria.Controllers
{
    public class FuncionarioController : Controller
    {
        private readonly Client _supabase;

        public FuncionarioController(Client supabase)
        {
            _supabase = supabase;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("NivelAcesso") < 4)
                return RedirectToAction("Index", "Home");

            var response = await _supabase.From<Operario>().Get();
            return View(response.Models.OrderBy(o => o.IdOperario).ToList());
        }

        [HttpGet]
        public IActionResult Cadastrar()
        {
            if (HttpContext.Session.GetInt32("NivelAcesso") < 4)
                return RedirectToAction("Index", "Home");

            return View(new Operario());
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar(Operario operario)
        {
            if (HttpContext.Session.GetInt32("NivelAcesso") < 4)
                return RedirectToAction("Index", "Home");

            try
            {
                await _supabase.From<Operario>().Insert(operario);
                TempData["Sucesso"] = "Funcionário cadastrado com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = "Erro ao cadastrar: " + ex.Message;
                return View(operario);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            if (HttpContext.Session.GetInt32("NivelAcesso") < 4)
                return RedirectToAction("Index", "Home");

            var response = await _supabase.From<Operario>().Where(x => x.IdOperario == id).Get();
            var operario = response.Models.FirstOrDefault();

            if (operario == null) return NotFound();

            return View(operario);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Operario operario)
        {
            if (HttpContext.Session.GetInt32("NivelAcesso") < 4)
                return RedirectToAction("Index", "Home");

            try
            {
                await _supabase.From<Operario>().Update(operario);
                TempData["Sucesso"] = "Funcionário atualizado com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = "Erro ao atualizar: " + ex.Message;
                return View(operario);
            }
        }
    }
}