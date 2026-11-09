using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SistemaPastelaria.Models
{
    [Table("operario")]
    public class Operario : BaseModel
    {
        [PrimaryKey("id_operario", false)]
        public int IdOperario { get; set; }

        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        [Column("cargo")]
        public string Cargo { get; set; } = string.Empty;

        [Column("nivel_acesso")]
        public int NivelAcesso { get; set; }

        [Column("pin")]
        public string Pin { get; set; } = string.Empty;

        [Column("ativo")]
        public bool Ativo { get; set; }
    }
}