using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RoleBasedAuthenticationUsingIdentity.Models
{
    public class ProductTables
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public string Price { get; set; }

        public string ProductType { get; set; }

        public string Quantity { get; set; }
    }
}
