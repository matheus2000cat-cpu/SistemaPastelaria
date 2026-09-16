using Microsoft.AspNetCore.Mvc;
using Supabase;
using SistemaPastelaria.Models;

namespace SistemaPastelaria.Controllers
{
    public class PontoController : Controller
    {
        private readonly Client _supabase;

        public PontoController(Client supabase)
        {
            _supabase = supabase;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? nivel = HttpContext.Session.GetInt32("NivelAcesso");
            if (nivel == null || nivel < 4) // Restrito exclusivamente ao Administrador / Dono (Nível 4)
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var pontoResponse = await _supabase.From<RegistroPonto>().Get();
                var operarioResponse = await _supabase.From<Operario>().Get();

                var operariosDict = operarioResponse.Models.ToDictionary(o => o.IdOperario, o => o);
                var listaViewModel = new List<PontoViewModel>();

                foreach (var p in pontoResponse.Models.OrderByDescending(x => x.DataEntrada))
                {
                    operariosDict.TryGetValue(p.IdOperario, out var op);
                    listaViewModel.Add(new PontoViewModel
                    {
                        IdRegistro = p.IdRegistro,
                        NomeOperario = op?.Nome ?? "Funcionário Desconhecido",
                        Cargo = op?.Cargo ?? "-",
                        DataEntrada = p.DataEntrada,
                        DataSaida = p.DataSaida
                    });
                }

                ViewBag.NomeOperario = HttpContext.Session.GetString("NomeOperario");
                return View(listaViewModel);
            }
            catch (Exception ex)
            {
                ViewBag.Erro = "Erro ao carregar registros de ponto: " + ex.Message;
                return View(new List<PontoViewModel>());
            }
        }
    }

    public class PontoViewModel
    {
        public int IdRegistro { get; set; }
        public string NomeOperario { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public DateTime DataEntrada { get; set; }
        public DateTime? DataSaida { get; set; }
    }
}