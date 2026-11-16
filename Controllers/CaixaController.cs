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
                var caixa = await CaixaAberto();
                if (caixa is null)
                {
                    return RedirectToAction(nameof(Abertura));
                }

                var response = await _supabase.From<Produto>().Get();
                var pedidosResponse = await _supabase.From<Pedido>()
                    .Where(pedido => pedido.IdCaixa == caixa.IdCaixa)
                    .Get();
                ViewBag.NomeOperario = HttpContext.Session.GetString("NomeOperario");
                ViewBag.NivelAcesso = HttpContext.Session.GetInt32("NivelAcesso") ?? 1;
                ViewBag.Caixa = caixa;
                ViewBag.VendasCaixa = pedidosResponse.Models
                    .Where(pedido => pedido.Status != "Cancelado")
                    .Sum(pedido => pedido.ValorTotal);
                
                var produtos = response.Models.Where(produto => produto.Ativo).ToList();
                return View(produtos);
            }
            catch (Exception ex)
            {
                TempData["Erro"] = "Não foi possível carregar o caixa: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Abertura()
        {
            if (HttpContext.Session.GetInt32("IdOperario") is null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var caixa = await CaixaAberto();
            if (caixa is not null)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.NomeOperario = HttpContext.Session.GetString("NomeOperario");
            return View();
        }

        [HttpPost("/Caixa/Abrir")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Abrir(decimal valorInicial)
        {
            var operarioId = HttpContext.Session.GetInt32("IdOperario");
            if (operarioId is null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (valorInicial < 0)
            {
                ModelState.AddModelError(nameof(valorInicial), "O valor inicial não pode ser negativo.");
                return View("Abertura");
            }

            if (await CaixaAberto() is not null)
            {
                return RedirectToAction(nameof(Index));
            }

            await _supabase.From<Caixa>().Insert(new Caixa
            {
                IdOperarioAbertura = operarioId.Value,
                ValorInicial = valorInicial,
                AberturaEm = DateTime.Now,
                Status = "Aberto"
            });

            TempData["Mensagem"] = "Caixa aberto com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/Caixa/Fechar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fechar(decimal valorInformado, string? observacao)
        {
            if (HttpContext.Session.GetInt32("IdOperario") is null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var caixa = await CaixaAberto();
            if (caixa is null)
            {
                return RedirectToAction(nameof(Abertura));
            }

            if (valorInformado < 0)
            {
                TempData["Erro"] = "O valor informado não pode ser negativo.";
                return RedirectToAction(nameof(Index));
            }

            caixa.ValorInformado = valorInformado;
            caixa.ObservacaoFechamento = (observacao ?? string.Empty).Trim();
            caixa.FechamentoEm = DateTime.Now;
            caixa.Status = "Fechado";
            await _supabase.From<Caixa>().Update(caixa);

            TempData["Mensagem"] = "Caixa fechado com sucesso.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SalvarPedido([FromBody] PedidoRequestDto dto)
        {
            int? operarioId = HttpContext.Session.GetInt32("IdOperario");
            if (operarioId == null)
            {
                return Json(new { sucesso = false, mensagem = "Sessão expirada. Faça login novamente." });
            }

            try
            {
                var caixa = await CaixaAberto();
                if (caixa is null)
                {
                    return Json(new { sucesso = false, mensagem = "Abra o caixa antes de registrar pedidos." });
                }

                if (dto.Itens.Count == 0)
                {
                    return Json(new { sucesso = false, mensagem = "Adicione pelo menos um produto ao pedido." });
                }

                var ids = dto.Itens.Select(item => item.IdProduto).Distinct().ToList();
                var produtos = (await _supabase.From<Produto>().Get()).Models
                    .Where(produto => produto.Ativo && ids.Contains(produto.IdProduto))
                    .ToDictionary(produto => produto.IdProduto);

                if (produtos.Count != ids.Count)
                {
                    return Json(new { sucesso = false, mensagem = "Um dos produtos não está mais disponível." });
                }

                var totalCalculado = dto.Itens.Sum(item => produtos[item.IdProduto].Preco * item.Quantidade);
                if (dto.Itens.Any(item => item.Quantidade <= 0))
                {
                    return Json(new { sucesso = false, mensagem = "A quantidade dos itens deve ser maior que zero." });
                }

                // 1. Cria o Pedido principal
                var novoPedido = new Pedido
                {
                    IdOperario = operarioId.Value,
                    IdCaixa = caixa.IdCaixa,
                    TipoAtendimento = dto.TipoAtendimento,
                    FormaPagamento = dto.FormaPagamento,
                    ValorTotal = totalCalculado,
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
                        PrecoUnitario = produtos[itemDto.IdProduto].Preco
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

        private async Task<Caixa?> CaixaAberto()
        {
            var response = await _supabase.From<Caixa>()
                .Where(caixa => caixa.Status == "Aberto")
                .Get();
            return response.Models.OrderByDescending(caixa => caixa.AberturaEm).FirstOrDefault();
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