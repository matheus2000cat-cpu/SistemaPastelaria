using Microsoft.AspNetCore.Mvc;
using Supabase;
using SistemaPastelaria.Models;

namespace SistemaPastelaria.Controllers
{
    public class AuthController : Controller
    {
        private readonly Client _supabase;

        public AuthController(Client supabase)
        {
            _supabase = supabase;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            HttpContext.Session.Clear(); 

            try
            {
                var response = await _supabase.From<Operario>().Get();
                
                // Manda o texto bruto da API do Supabase pra tela pra gente ver o que tá vindo!
                ViewBag.RawJson = response.Content;

                var operarios = response.Models?.Where(o => o != null && !string.IsNullOrEmpty(o.Nome)).ToList() ?? new List<Operario>();

                if (!operarios.Any())
                {
                    ViewBag.Erro = "O C# não encontrou operadores. Veja a caixa amarela abaixo para descobrir o que o banco devolveu.";
                }

                return View(operarios);
            }
            catch (Exception ex)
            {
                ViewBag.Erro = "Erro fatal de conexão: " + ex.Message;
                return View(new List<Operario>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(int idOperario, string pin)
        {
            try
            {
                var response = await _supabase.From<Operario>().Where(x => x.IdOperario == idOperario).Get();
                var operario = response.Models.FirstOrDefault();

                if (operario != null && operario.Pin == pin)
                {
                    HttpContext.Session.SetInt32("IdOperario", operario.IdOperario);
                    HttpContext.Session.SetString("NomeOperario", operario.Nome);
                    HttpContext.Session.SetInt32("NivelAcesso", operario.NivelAcesso);
                    return RedirectToAction("Index", "Home");
                }

                TempData["Erro"] = "PIN incorreto.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = "Erro no login: " + ex.Message;
                return RedirectToAction("Login");
            }
        }
    }
}