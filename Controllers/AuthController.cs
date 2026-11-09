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
            return View(await CriarLoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await CriarLoginViewModel(model);
                return View(model);
            }

            var response = await _supabase.From<Operario>()
                .Where(x => x.IdOperario == model.IdOperario && x.Ativo)
                .Get();

            var operario = response.Models.FirstOrDefault();

            if (operario is null || !string.Equals(operario.Pin, model.SenhaPin, StringComparison.Ordinal))
            {
                ModelState.AddModelError(string.Empty, "Funcionário ou PIN inválido.");
                model = await CriarLoginViewModel(model);
                return View(model);
            }

            HttpContext.Session.Clear();
            HttpContext.Session.SetInt32("IdOperario", operario.IdOperario);
            HttpContext.Session.SetString("NomeOperario", operario.Nome);
            HttpContext.Session.SetInt32("NivelAcesso", operario.NivelAcesso);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private async Task<LoginViewModel> CriarLoginViewModel(LoginViewModel? model = null)
        {
            var response = await _supabase.From<Operario>()
                .Where(operario => operario.Ativo)
                .Get();

            return new LoginViewModel
            {
                Funcionarios = response.Models
                    .Select(operario => new FuncionarioOpcaoViewModel
                    {
                        IdOperario = operario.IdOperario,
                        Nome = operario.Nome,
                        Cargo = operario.Cargo
                    }).ToList(),
                IdOperario = model?.IdOperario ?? 0
            };
        }
    }
}