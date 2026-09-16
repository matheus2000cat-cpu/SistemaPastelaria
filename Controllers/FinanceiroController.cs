using Microsoft.AspNetCore.Mvc;
using Supabase;
using SistemaPastelaria.Models;

namespace SistemaPastelaria.Controllers
{
    public class FinanceiroController : Controller
    {
        private readonly Client _supabase;

        public FinanceiroController(Client supabase)
        {
            _supabase = supabase;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? operarioId = HttpContext.Session.GetInt32("IdOperario");
            int? nivel = HttpContext.Session.GetInt32("NivelAcesso");
            
            if (operarioId == null || nivel < 3) // Apenas Gerente (3) ou Admin (4)
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Busca movimentações (Sangrias e Suprimentos)
                var movResponse = await _supabase.From<MovimentacaoCaixa>().Get();
                
                // Busca operários para exibir o nome de quem fez a movimentação
                var opResponse = await _supabase.From<Operario>().Get();
                var operariosDict = opResponse.Models.ToDictionary(o => o.IdOperario, o => o);

                var listaVm = new List<FinanceiroViewModel>();
                decimal totalSangrias = 0;
                decimal totalSuprimentos = 0;

                foreach (var m in movResponse.Models.OrderByDescending(x => x.DataMovimento))
                {
                    operariosDict.TryGetValue(m.IdOperario, out var op);
                    
                    if (m.Tipo == "Sangria") totalSangrias += m.Valor;
                    if (m.Tipo == "Suprimento") totalSuprimentos += m.Valor;

                    listaVm.Add(new FinanceiroViewModel
                    {
                        IdMovimentacao = m.IdMovimentacao,
                        NomeOperario = op?.Nome ?? "Desconhecido",
                        Tipo = m.Tipo,
                        Valor = m.Valor,
                        Observacao = m.Observacao,
                        DataMovimento = m.DataMovimento
                    });
                }

                ViewBag.TotalSangrias = totalSangrias;
                ViewBag.TotalSuprimentos = totalSuprimentos;
                ViewBag.NomeOperario = HttpContext.Session.GetString("NomeOperario");

                return View(listaVm);
            }
            catch (Exception ex)
            {
                ViewBag.Erro = "Erro ao carregar dados financeiros: " + ex.Message;
                return View(new List<FinanceiroViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarMovimentacao(string tipo, decimal valor, string observacao)
        {
            int? operarioId = HttpContext.Session.GetInt32("IdOperario");
            if (operarioId == null) return RedirectToAction("Login", "Auth");

            try
            {
                var mov = new MovimentacaoCaixa
                {
                    IdOperario = operarioId.Value,
                    Tipo = tipo,
                    Valor = valor,
                    Observacao = observacao ?? string.Empty,
                    DataMovimento = DateTime.Now
                };

                await _supabase.From<MovimentacaoCaixa>().Insert(mov);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = "Erro ao registrar: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }

    public class FinanceiroViewModel
    {
        public int IdMovimentacao { get; set; }
        public string NomeOperario { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Observacao { get; set; } = string.Empty;
        public DateTime DataMovimento { get; set; }
    }
}