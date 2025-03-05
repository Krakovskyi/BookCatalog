using System;
using System.ComponentModel.DataAnnotations;

namespace BookCatalog.Shared
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Book title is required")]
        [StringLength(100, ErrorMessage = "Book title must not exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author name is required")]
        [StringLength(100, ErrorMessage = "Author name must not exceed 100 characters")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Genre is required")]
        [StringLength(50, ErrorMessage = "Genre must not exceed 50 characters")]
        public string Genre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Range(1000, 2100, ErrorMessage = "Publication year must be between 1000 and 2100")]
        public int? PublicationYear { get; set; }

        [StringLength(20, ErrorMessage = "ISBN must not exceed 20 characters")]
        public string ISBN { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
} 