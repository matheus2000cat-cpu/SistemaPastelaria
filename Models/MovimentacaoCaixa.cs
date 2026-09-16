using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SistemaPastelaria.Models
{
    [Table("movimentacao_caixa")]
    public class MovimentacaoCaixa : BaseModel
    {
        [PrimaryKey("id_movimentacao", false)]
        public int IdMovimentacao { get; set; }

        [Column("id_operario")]
        public int IdOperario { get; set; }

        [Column("tipo")]
        public string Tipo { get; set; } = string.Empty; // 'Sangria' ou 'Suprimento'

        [Column("valor")]
        public decimal Valor { get; set; }

        [Column("observacao")]
        public string Observacao { get; set; } = string.Empty;

        [Column("data_movimento")]
        public DateTime DataMovimento { get; set; } = DateTime.Now;
    }
}