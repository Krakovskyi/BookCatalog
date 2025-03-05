using System;
using System.ComponentModel.DataAnnotations;

namespace BookCatalog.Shared
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название книги обязательно")]
        [StringLength(100, ErrorMessage = "Название книги не должно превышать 100 символов")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Имя автора обязательно")]
        [StringLength(100, ErrorMessage = "Имя автора не должно превышать 100 символов")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Жанр обязателен")]
        [StringLength(50, ErrorMessage = "Жанр не должен превышать 50 символов")]
        public string Genre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string Description { get; set; } = string.Empty;

        [Range(1000, 2100, ErrorMessage = "Год публикации должен быть между 1000 и 2100")]
        public int? PublicationYear { get; set; }

        [StringLength(20, ErrorMessage = "ISBN не должен превышать 20 символов")]
        public string ISBN { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
} 