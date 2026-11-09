namespace SistemaPastelaria.Models;

public class AdminDashboardViewModel
{
    public IReadOnlyList<Produto> Produtos { get; init; } = [];
    public IReadOnlyList<Pedido> Pedidos { get; init; } = [];
    public Produto NovoProduto { get; init; } = new();
}
