using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SistemaPastelaria.Models;

[Table("pedido")]
public class Pedido : BaseModel
{
    [PrimaryKey("id_pedido", false)]
    public int IdPedido { get; set; }

    [Column("id_operario")]
    public int? IdOperario { get; set; }

    [Column("cliente_nome")]
    public string ClienteNome { get; set; } = string.Empty;

    [Column("cliente_telefone")]
    public string ClienteTelefone { get; set; } = string.Empty;

    [Column("tipo_atendimento")]
    public string TipoAtendimento { get; set; } = string.Empty;

    [Column("endereco_entrega")]
    public string? EnderecoEntrega { get; set; }

    [Column("itens")]
    public List<PedidoItem> Itens { get; set; } = [];

    [Column("status")]
    public string Status { get; set; } = "Pendente";

    [Column("valor_total")]
    public decimal ValorTotal { get; set; }

    [Column("forma_pagamento")]
    public string FormaPagamento { get; set; } = string.Empty;

    [Column("data_criacao")]
    public DateTime DataCriacao { get; set; }

    [Column("data_conclusao")]
    public DateTime? DataConclusao { get; set; }
}
