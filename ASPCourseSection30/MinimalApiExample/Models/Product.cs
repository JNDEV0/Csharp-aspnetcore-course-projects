using System.ComponentModel.DataAnnotations;

namespace MinimalApiExample.Models
{
    public class Product
    {
        [Required(ErrorMessage = "Id cant be blank")]
        [Range(0, int.MaxValue, ErrorMessage = "Id must be positive value int")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name cant be blank")]
        public string Name { get; set; }

        public Product(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return $"ProductId: {Id}, ProductName: {Name}";
        }
    }
}
