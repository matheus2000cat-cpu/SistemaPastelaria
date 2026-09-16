using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace SistemaPastelaria.Models
{
    [Table("item_pedido")]
    public class ItemPedido : BaseModel
    {
        [PrimaryKey("id_item", false)]
        public int IdItem { get; set; }

        [Column("id_pedido")]
        public int IdPedido { get; set; }

        [Column("id_produto")]
        public int IdProduto { get; set; }

        [Column("quantidade")]
        public int Quantidade { get; set; }

        [Column("preco_unitario")]
        public decimal PrecoUnitario { get; set; }

        [Column("observacao")]
        public string Observacao { get; set; } = string.Empty;
    }
}