using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SistemaPastelaria.Models
{
    [Table("produto")]
    public class Produto : BaseModel
    {
        [PrimaryKey("id_produto", false)]
        public int IdProduto { get; set; }

        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        [Column("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [Column("preco")]
        public decimal Preco { get; set; }

        [Column("ativo")]
        public bool Ativo { get; set; } = true;
    }
}