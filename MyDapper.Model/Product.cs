using System.ComponentModel.DataAnnotations;

namespace MyDapper.Model
{
    public class Product
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }
    }
}