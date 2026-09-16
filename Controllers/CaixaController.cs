using Microsoft.AspNetCore.Mvc;
using Supabase;
using SistemaPastelaria.Models;

namespace SistemaPastelaria.Controllers
{
    public class CaixaController : Controller
    {
        private readonly Client _supabase;

        public CaixaController(Client supabase)
        {
            _supabase = supabase;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? operarioId = HttpContext.Session.GetInt32("IdOperario");
            if (operarioId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var response = await _supabase.From<Produto>().Get();
                ViewBag.NomeOperario = HttpContext.Session.GetString("NomeOperario");
                ViewBag.NivelAcesso = HttpContext.Session.GetInt32("NivelAcesso") ?? 1;
                
                var produtos = response.Models;
                return View(produtos);
            }
            catch (Exception ex)
            {
                ViewBag.Erro = "Erro ao carregar PDV: " + ex.Message;
                return View(new List<Produto>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SalvarPedido([FromBody] PedidoRequestDto dto)
        {
            int? operarioId = HttpContext.Session.GetInt32("IdOperario");
            if (operarioId == null)
            {
                return Json(new { sucesso = false, mensagem = "Sessão expirada. Faça login novamente." });
            }

            try
            {
                // 1. Cria o Pedido principal
                var novoPedido = new Pedido
                {
                    IdOperario = operarioId.Value,
                    TipoAtendimento = dto.TipoAtendimento,
                    FormaPagamento = dto.FormaPagamento,
                    ValorTotal = dto.ValorTotal,
                    Status = "Pendente",
                    DataCriacao = DateTime.Now
                };

                var pedidoResponse = await _supabase.From<Pedido>().Insert(novoPedido);
                var pedidoSalvo = pedidoResponse.Models.FirstOrDefault();

                if (pedidoSalvo == null)
                {
                    return Json(new { sucesso = false, mensagem = "Não foi possível gravar o pedido principal no banco." });
                }

                // 2. Insere os Itens do Pedido
                foreach (var itemDto in dto.Itens)
                {
                    var novoItem = new ItemPedido
                    {
                        IdPedido = pedidoSalvo.IdPedido,
                        IdProduto = itemDto.IdProduto,
                        Quantidade = itemDto.Quantidade,
                        PrecoUnitario = itemDto.PrecoUnitario
                    };

                    await _supabase.From<ItemPedido>().Insert(novoItem);
                }

                return Json(new { sucesso = true });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = ex.Message });
            }
        }
    }

    // DTOs para receber o JSON do JavaScript do PDV
    public class PedidoRequestDto
    {
        public string TipoAtendimento { get; set; } = string.Empty;
        public string FormaPagamento { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public List<ItemPedidoDto> Itens { get; set; } = new();
    }

    public class ItemPedidoDto
    {
        public int IdProduto { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }
}