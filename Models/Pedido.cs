using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SistemaPastelaria.Models
{
    [Table("pedido")]
    public class Pedido : BaseModel
    {
        [PrimaryKey("id_pedido", false)]
        public int IdPedido { get; set; }

        [Column("id_operario")]
        public int IdOperario { get; set; }

        [Column("tipo_atendimento")]
        public string TipoAtendimento { get; set; } = string.Empty;

        [Column("status")]
        public string Status { get; set; } = "Pendente";

        [Column("valor_total")]
        public decimal ValorTotal { get; set; }

        [Column("forma_pagamento")]
        public string FormaPagamento { get; set; } = string.Empty;

        [Column("data_criacao")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Column("data_conclusao")]
        public DateTime? DataConclusao { get; set; }
    }
}