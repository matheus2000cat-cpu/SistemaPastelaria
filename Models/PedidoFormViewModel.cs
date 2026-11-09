using System.ComponentModel.DataAnnotations;

namespace SistemaPastelaria.Models;

public class PedidoFormViewModel
{
    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(100)]
    public string ClienteNome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu telefone.")]
    [StringLength(30)]
    public string ClienteTelefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escolha entrega ou retirada.")]
    public string TipoAtendimento { get; set; } = string.Empty;

    [StringLength(300)]
    public string? EnderecoEntrega { get; set; }

    [Required(ErrorMessage = "Adicione pelo menos um item ao pedido.")]
    public string ItensJson { get; set; } = string.Empty;
}
