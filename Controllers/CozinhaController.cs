using Microsoft.AspNetCore.Mvc;
using Supabase;
using SistemaPastelaria.Models;

namespace SistemaPastelaria.Controllers
{
    public class CozinhaController : Controller
    {
        private readonly Client _supabase;

        public CozinhaController(Client supabase)
        {
            _supabase = supabase;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("IdOperario") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var response = await _supabase.From<Pedido>()
                    .Where(x => x.Status != "Concluído" && x.Status != "Cancelado")
                    .Get();

                ViewBag.NomeOperario = HttpContext.Session.GetString("NomeOperario");
                return View(response.Models);
            }
            catch (Exception ex)
            {
                ViewBag.Erro = "Erro ao carregar pedidos da cozinha: " + ex.Message;
                return View(new List<Pedido>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarStatus([FromBody] AtualizarStatusDto dto)
        {
            if (HttpContext.Session.GetInt32("IdOperario") == null)
            {
                return Json(new { sucesso = false, mensagem = "Sessão expirada." });
            }

            try
            {
                var response = await _supabase.From<Pedido>()
                    .Where(x => x.IdPedido == dto.IdPedido)
                    .Get();

                var pedido = response.Models.FirstOrDefault();
                if (pedido != null)
                {
                    pedido.Status = dto.NovoStatus;

                    // Se o pedido foi concluído, grava o horário exato de fechamento
                    if (dto.NovoStatus == "Concluído")
                    {
                        pedido.DataConclusao = DateTime.Now;
                    }

                    await _supabase.From<Pedido>().Update(pedido);
                    return Json(new { sucesso = true });
                }

                return Json(new { sucesso = false, mensagem = "Pedido não encontrado." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Erro ao atualizar status: " + ex.Message });
            }
        }
    }

    public class AtualizarStatusDto
    {
        public int IdPedido { get; set; }
        public string NovoStatus { get; set; } = string.Empty;
    }
}