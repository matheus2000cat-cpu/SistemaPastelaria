using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SistemaPastelaria.Models
{
    [Table("operario")]
    public class Operario : BaseModel
    {
        [PrimaryKey("id_operario", false)]
        [JsonProperty("id_operario")]
        public int IdOperario { get; set; }

        [Column("nome")]
        [JsonProperty("nome")]
        public string Nome { get; set; } = string.Empty;

        [Column("pin")]
        [JsonProperty("pin")]
        public string Pin { get; set; } = string.Empty;

        [Column("cargo")]
        [JsonProperty("cargo")]
        public string Cargo { get; set; } = string.Empty;

        [Column("nivel_acesso")]
        [JsonProperty("nivel_acesso")]
        public int NivelAcesso { get; set; }

        [Column("ativo")]
        [JsonProperty("ativo")]
        public bool Ativo { get; set; }
    }
}