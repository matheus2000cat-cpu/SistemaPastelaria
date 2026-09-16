using Microsoft.AspNetCore.Mvc;
using Supabase;
using SistemaPastelaria.Models;

namespace SistemaPastelaria.Controllers
{
    public class HomeController : Controller
    {
        private readonly Client _supabase;

        public HomeController(Client supabase)
        {
            _supabase = supabase;
        }

        public async Task<IActionResult> Index()
        {
            int? operarioId = HttpContext.Session.GetInt32("IdOperario");
            if (operarioId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.NomeOperario = HttpContext.Session.GetString("NomeOperario");
            ViewBag.NivelAcesso = HttpContext.Session.GetInt32("NivelAcesso") ?? 1;

            decimal vendasHoje = 0;
            int pedidosAtivos = 0;
            int pedidosPendentes = 0;

            try
            {
                var response = await _supabase.From<Pedido>().Get();
                var pedidos = response.Models;

                var hoje = DateTime.Today;

                foreach (var p in pedidos)
                {
                    // Soma as vendas de hoje (desconsiderando pedidos cancelados)
                    if (p.DataCriacao.Date == hoje && p.Status != "Cancelado")
                    {
                        vendasHoje += p.ValorTotal;
                    }

                    // Conta pedidos ativos (que não foram concluídos nem cancelados)
                    if (p.Status != "Concluído" && p.Status != "Cancelado")
                    {
                        pedidosAtivos++;
                        if (p.Status == "Pendente")
                        {
                            pedidosPendentes++;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Mantém 0 caso ocorra algum problema temporário de conexão
            }

            ViewBag.VendasHoje = vendasHoje;
            ViewBag.PedidosAtivos = pedidosAtivos;
            ViewBag.PedidosPendentes = pedidosPendentes;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}