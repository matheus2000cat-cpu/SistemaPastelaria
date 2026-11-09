using Microsoft.AspNetCore.Mvc;
using SistemaPastelaria.Models;
using Supabase;

namespace SistemaPastelaria.Controllers;

public class AdminController : Controller
{
    private static readonly string[] StatusPedido =
    [
        "Pendente",
        "Em Preparo",
        "Pronto",
        "Concluído",
        "Cancelado"
    ];

    private readonly Client _supabase;

    public AdminController(Client supabase)
    {
        _supabase = supabase;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!FuncionarioAutenticado())
        {
            return RedirectToAction("Login", "Auth");
        }

        var produtos = await _supabase.From<Produto>().Get();
        var pedidos = await _supabase.From<Pedido>().Get();
        var model = new AdminDashboardViewModel
        {
            Produtos = produtos.Models.OrderBy(produto => produto.Nome).ToList(),
            Pedidos = pedidos.Models.OrderByDescending(pedido => pedido.DataCriacao).Take(50).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarProduto(Produto produto)
    {
        if (!FuncionarioAutenticado())
        {
            return RedirectToAction("Login", "Auth");
        }

        ValidarProduto(produto);
        if (!ModelState.IsValid)
        {
            return await IndexComErros(produto);
        }

        produto.Nome = produto.Nome.Trim();
        produto.Categoria = produto.Categoria.Trim();
        produto.Descricao = (produto.Descricao ?? string.Empty).Trim();
        produto.ImagemUrl = string.IsNullOrWhiteSpace(produto.ImagemUrl) ? null : produto.ImagemUrl.Trim();
        produto.Ativo = true;
        await _supabase.From<Produto>().Insert(produto);

        TempData["Mensagem"] = "Produto cadastrado no cardápio.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarProduto(Produto produto)
    {
        if (!FuncionarioAutenticado())
        {
            return RedirectToAction("Login", "Auth");
        }

        ValidarProduto(produto);
        if (produto.IdProduto <= 0)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return await Editar(produto.IdProduto, produto);
        }

        var existente = await BuscarProduto(produto.IdProduto);
        if (existente is null)
        {
            return NotFound();
        }

        existente.Nome = produto.Nome.Trim();
        existente.Categoria = produto.Categoria.Trim();
        existente.Descricao = (produto.Descricao ?? string.Empty).Trim();
        existente.Preco = produto.Preco;
        existente.ImagemUrl = string.IsNullOrWhiteSpace(produto.ImagemUrl) ? null : produto.ImagemUrl.Trim();
        await _supabase.From<Produto>().Update(existente);

        TempData["Mensagem"] = "Produto atualizado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id, Produto? produto = null)
    {
        if (!FuncionarioAutenticado())
        {
            return RedirectToAction("Login", "Auth");
        }

        if (produto is not null && !ModelState.IsValid)
        {
            return View(produto);
        }

        var encontrado = await BuscarProduto(id);
        return encontrado is null ? NotFound() : View(encontrado);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesativarProduto(int id)
    {
        if (!FuncionarioAutenticado())
        {
            return RedirectToAction("Login", "Auth");
        }

        var produto = await BuscarProduto(id);
        if (produto is null)
        {
            return NotFound();
        }

        produto.Ativo = false;
        await _supabase.From<Produto>().Update(produto);
        TempData["Mensagem"] = "Produto removido do cardápio.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtivarProduto(int id)
    {
        if (!FuncionarioAutenticado())
        {
            return RedirectToAction("Login", "Auth");
        }

        var produto = await BuscarProduto(id);
        if (produto is null)
        {
            return NotFound();
        }

        produto.Ativo = true;
        await _supabase.From<Produto>().Update(produto);
        TempData["Mensagem"] = "Produto voltou ao cardápio.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarStatus(int pedidoId, string status)
    {
        if (!FuncionarioAutenticado())
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!StatusPedido.Contains(status))
        {
            return BadRequest();
        }

        var response = await _supabase.From<Pedido>()
            .Where(pedido => pedido.IdPedido == pedidoId)
            .Get();
        var pedido = response.Models.FirstOrDefault();
        if (pedido is null)
        {
            return NotFound();
        }

        pedido.Status = status;
        await _supabase.From<Pedido>().Update(pedido);
        TempData["Mensagem"] = $"Pedido #{pedido.IdPedido} atualizado.";
        return RedirectToAction(nameof(Index));
    }

    private bool FuncionarioAutenticado() => HttpContext.Session.GetInt32("IdOperario") is not null;

    private async Task<Produto?> BuscarProduto(int id)
    {
        var response = await _supabase.From<Produto>()
            .Where(produto => produto.IdProduto == id)
            .Get();
        return response.Models.FirstOrDefault();
    }

    private async Task<IActionResult> IndexComErros(Produto produto)
    {
        var produtos = await _supabase.From<Produto>().Get();
        var pedidos = await _supabase.From<Pedido>().Get();
        return View("Index", new AdminDashboardViewModel
        {
            Produtos = produtos.Models.OrderBy(item => item.Nome).ToList(),
            Pedidos = pedidos.Models.OrderByDescending(item => item.DataCriacao).Take(50).ToList(),
            NovoProduto = produto
        });
    }

    private void ValidarProduto(Produto produto)
    {
        if (string.IsNullOrWhiteSpace(produto.Nome) || produto.Nome.Length > 100)
        {
            ModelState.AddModelError(nameof(produto.Nome), "Informe um nome com até 100 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(produto.Categoria) || produto.Categoria.Length > 60)
        {
            ModelState.AddModelError(nameof(produto.Categoria), "Informe uma categoria com até 60 caracteres.");
        }

        if ((produto.Descricao ?? string.Empty).Length > 500)
        {
            ModelState.AddModelError(nameof(produto.Descricao), "A descrição deve ter até 500 caracteres.");
        }

        if (produto.Preco <= 0)
        {
            ModelState.AddModelError(nameof(produto.Preco), "O preço deve ser maior que zero.");
        }

        if (produto.ImagemUrl is { Length: > 500 })
        {
            ModelState.AddModelError(nameof(produto.ImagemUrl), "O endereço da imagem deve ter até 500 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(produto.ImagemUrl) &&
            (!Uri.TryCreate(produto.ImagemUrl, UriKind.Absolute, out var imagemUri) ||
             imagemUri.Scheme is not ("http" or "https")))
        {
            ModelState.AddModelError(nameof(produto.ImagemUrl), "Use um endereço de imagem HTTP ou HTTPS válido.");
        }
    }
}
