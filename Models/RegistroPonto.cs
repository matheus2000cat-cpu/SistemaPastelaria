using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SistemaPastelaria.Models
{
    [Table("registro_ponto")]
    public class RegistroPonto : BaseModel
    {
        [PrimaryKey("id_registro", false)]
        public int IdRegistro { get; set; }

        [Column("id_operario")]
        public int IdOperario { get; set; }

        [Column("data_entrada")]
        public DateTime DataEntrada { get; set; } = DateTime.Now;

        [Column("data_saida")]
        public DateTime? DataSaida { get; set; }
    }
}