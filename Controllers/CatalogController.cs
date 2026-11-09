using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SistemaPastelaria.Models;
using Supabase;

namespace SistemaPastelaria.Controllers;

public class CatalogController : Controller
{
    private readonly Client _supabase;

    public CatalogController(Client supabase)
    {
        _supabase = supabase;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var response = await _supabase.From<Produto>()
            .Where(produto => produto.Ativo)
            .Get();

        return View(response.Models.OrderBy(produto => produto.Categoria)
            .ThenBy(produto => produto.Nome).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizarPedido(PedidoFormViewModel form)
    {
        var tipoValido = form.TipoAtendimento is "Retirada" or "Entrega";
        if (!tipoValido)
        {
            ModelState.AddModelError(nameof(form.TipoAtendimento), "Escolha uma forma de atendimento válida.");
        }

        if (form.TipoAtendimento == "Entrega" && string.IsNullOrWhiteSpace(form.EnderecoEntrega))
        {
            ModelState.AddModelError(nameof(form.EnderecoEntrega), "Informe o endereço para entrega.");
        }

        List<ItemCarrinhoInput>? itens;
        try
        {
            itens = JsonSerializer.Deserialize<List<ItemCarrinhoInput>>(form.ItensJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            itens = null;
        }

        if (itens is null || itens.Count == 0 || itens.Any(item => item.IdProduto <= 0 || item.Quantidade is < 1 or > 50))
        {
            ModelState.AddModelError(nameof(form.ItensJson), "O carrinho está vazio ou contém itens inválidos.");
        }

        var produtosResponse = await _supabase.From<Produto>()
            .Where(produto => produto.Ativo)
            .Get();
        var produtos = produtosResponse.Models.ToDictionary(produto => produto.IdProduto);

        if (itens is not null)
        {
            foreach (var item in itens)
            {
                if (!produtos.ContainsKey(item.IdProduto))
                {
                    ModelState.AddModelError(nameof(form.ItensJson), "Um dos produtos não está mais disponível. Atualize o cardápio e tente novamente.");
                    break;
                }
            }
        }

        if (!ModelState.IsValid || itens is null)
        {
            var catalogo = await _supabase.From<Produto>()
                .Where(produto => produto.Ativo)
                .Get();
            return View("Index", catalogo.Models.OrderBy(produto => produto.Categoria)
                .ThenBy(produto => produto.Nome).ToList());
        }

        var quantidades = itens
            .GroupBy(item => item.IdProduto)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(item => item.Quantidade));

        if (quantidades.Values.Any(quantidade => quantidade > 50))
        {
            ModelState.AddModelError(nameof(form.ItensJson), "Cada produto pode ter no máximo 50 unidades por pedido.");
            var catalogo = await _supabase.From<Produto>().Where(produto => produto.Ativo).Get();
            return View("Index", catalogo.Models.OrderBy(produto => produto.Categoria)
                .ThenBy(produto => produto.Nome).ToList());
        }

        var linhasPedido = quantidades.Select(par =>
        {
            var produto = produtos[par.Key];
            return new PedidoItem
            {
                IdProduto = produto.IdProduto,
                NomeProduto = produto.Nome,
                Quantidade = par.Value,
                PrecoUnitario = produto.Preco,
                Subtotal = produto.Preco * par.Value
            };
        }).ToList();

        var pedido = new Pedido
        {
            ClienteNome = form.ClienteNome.Trim(),
            ClienteTelefone = form.ClienteTelefone.Trim(),
            TipoAtendimento = form.TipoAtendimento == "Entrega" ? "Delivery" : "Balcão",
            EnderecoEntrega = form.TipoAtendimento == "Entrega" ? form.EnderecoEntrega?.Trim() : null,
            Itens = linhasPedido,
            Status = "Pendente",
            ValorTotal = linhasPedido.Sum(item => item.Subtotal),
            FormaPagamento = form.TipoAtendimento == "Entrega" ? "Na entrega" : "Na retirada",
            DataCriacao = DateTime.UtcNow
        };

        var response = await _supabase.From<Pedido>().Insert(pedido);
        var pedidoCriado = response.Models.FirstOrDefault()
            ?? throw new InvalidOperationException("O Supabase não retornou o pedido criado.");

        foreach (var item in linhasPedido)
        {
            await _supabase.From<ItemPedido>().Insert(new ItemPedido
            {
                IdPedido = pedidoCriado.IdPedido,
                IdProduto = item.IdProduto,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario,
                Observacao = string.Empty
            });
        }

        return View("Confirmacao", pedidoCriado);
    }

    private sealed class ItemCarrinhoInput
    {
        public int IdProduto { get; set; }
        public int Quantidade { get; set; }
    }
}
