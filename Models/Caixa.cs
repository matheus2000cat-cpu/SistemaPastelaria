using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SistemaPastelaria.Models;

[Table("caixa")]
public class Caixa : BaseModel
{
    [PrimaryKey("id_caixa", false)]
    public int IdCaixa { get; set; }

    [Column("id_operario_abertura")]
    public int IdOperarioAbertura { get; set; }

    [Column("abertura_em")]
    public DateTime AberturaEm { get; set; } = DateTime.Now;

    [Column("valor_inicial")]
    public decimal ValorInicial { get; set; }

    [Column("fechamento_em")]
    public DateTime? FechamentoEm { get; set; }

    [Column("valor_informado")]
    public decimal? ValorInformado { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Aberto";

    [Column("observacao_fechamento")]
    public string ObservacaoFechamento { get; set; } = string.Empty;
}
