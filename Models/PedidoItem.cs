namespace SistemaPastelaria.Models;

public class PedidoItem
{
    public int IdProduto { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
