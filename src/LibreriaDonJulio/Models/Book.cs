using System.ComponentModel.DataAnnotations;

namespace LibreriaDonJulio.Models
{
    public class Book
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        [Required]
        public string Isbn { get; set; } = string.Empty;
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price can't not be negative.")]
        public decimal Price { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock can't not be negative")]
        public int Stock { get; set; }
    }
}
