using BookCatalog.Shared;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace BookCatalog.Server.Services
{
    public class BookService
    {
        private readonly ConcurrentDictionary<int, Book> _books = new();
        private int _nextId = 1;

        // Инициализация с тестовыми данными
        public BookService()
        {
            // Добавляем несколько книг для тестирования
            AddBook(new Book { Title = "1984", Author = "Джордж Оруэлл", Genre = "Антиутопия", Description = "Роман-антиутопия Джорджа Оруэлла, изданный в 1949 году.", PublicationYear = 1949, ISBN = "978-5-17-080115-9" });
            AddBook(new Book { Title = "Преступление и наказание", Author = "Фёдор Достоевский", Genre = "Роман", Description = "Социально-психологический и социально-философский роман Фёдора Михайловича Достоевского.", PublicationYear = 1866, ISBN = "978-5-17-085554-1" });
            AddBook(new Book { Title = "Мастер и Маргарита", Author = "Михаил Булгаков", Genre = "Фантастика", Description = "Роман Михаила Афанасьевича Булгакова, работа над которым началась в конце 1920-х годов и продолжалась вплоть до смерти писателя.", PublicationYear = 1967, ISBN = "978-5-17-103233-8" });
            AddBook(new Book { Title = "Гарри Поттер и философский камень", Author = "Джоан Роулинг", Genre = "Фэнтези", Description = "Первый роман в серии книг про юного волшебника Гарри Поттера, написанный Дж. К. Роулинг.", PublicationYear = 1997, ISBN = "978-5-389-07435-4" });
            AddBook(new Book { Title = "Война и мир", Author = "Лев Толстой", Genre = "Роман-эпопея", Description = "Роман-эпопея Льва Николаевича Толстого, описывающий русское общество в эпоху войн против Наполеона в 1805—1812 годах.", PublicationYear = 1869, ISBN = "978-5-389-06556-7" });
        }

        // Получить все книги с пагинацией и фильтрацией
        public PagedResult<Book> GetBooks(BookQuery query)
        {
            var result = new PagedResult<Book>
            {
                Page = query.Page,
                PageSize = query.PageSize
            };

            // Применяем фильтрацию
            var filteredBooks = _books.Values.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchTerm = query.SearchTerm.ToLower();
                filteredBooks = filteredBooks.Where(b => 
                    b.Title.ToLower().Contains(searchTerm) || 
                    b.Author.ToLower().Contains(searchTerm) || 
                    b.Description.ToLower().Contains(searchTerm) ||
                    b.ISBN.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(query.Author))
            {
                var author = query.Author.ToLower();
                filteredBooks = filteredBooks.Where(b => b.Author.ToLower().Contains(author));
            }

            if (!string.IsNullOrWhiteSpace(query.Genre))
            {
                var genre = query.Genre.ToLower();
                filteredBooks = filteredBooks.Where(b => b.Genre.ToLower().Contains(genre));
            }

            // Получаем общее количество записей после фильтрации
            result.TotalCount = filteredBooks.Count();

            // Применяем сортировку
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                switch (query.SortBy.ToLower())
                {
                    case "title":
                        filteredBooks = query.SortAscending 
                            ? filteredBooks.OrderBy(b => b.Title) 
                            : filteredBooks.OrderByDescending(b => b.Title);
                        break;
                    case "author":
                        filteredBooks = query.SortAscending 
                            ? filteredBooks.OrderBy(b => b.Author) 
                            : filteredBooks.OrderByDescending(b => b.Author);
                        break;
                    case "genre":
                        filteredBooks = query.SortAscending 
                            ? filteredBooks.OrderBy(b => b.Genre) 
                            : filteredBooks.OrderByDescending(b => b.Genre);
                        break;
                    case "year":
                        filteredBooks = query.SortAscending 
                            ? filteredBooks.OrderBy(b => b.PublicationYear) 
                            : filteredBooks.OrderByDescending(b => b.PublicationYear);
                        break;
                    case "createdat":
                        filteredBooks = query.SortAscending 
                            ? filteredBooks.OrderBy(b => b.CreatedAt) 
                            : filteredBooks.OrderByDescending(b => b.CreatedAt);
                        break;
                    default:
                        filteredBooks = filteredBooks.OrderBy(b => b.Title);
                        break;
                }
            }

            // Применяем пагинацию
            result.Items = filteredBooks
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            return result;
        }

        // Получить книгу по ID
        public Book? GetBookById(int id)
        {
            _books.TryGetValue(id, out var book);
            return book;
        }

        // Добавить новую книгу
        public Book AddBook(Book book)
        {
            book.Id = _nextId++;
            book.CreatedAt = DateTime.Now;
            _books[book.Id] = book;
            return book;
        }

        // Обновить существующую книгу
        public bool UpdateBook(int id, Book updatedBook)
        {
            if (!_books.TryGetValue(id, out var existingBook))
            {
                return false;
            }

            updatedBook.Id = id;
            updatedBook.CreatedAt = existingBook.CreatedAt;
            updatedBook.UpdatedAt = DateTime.Now;

            return _books.TryUpdate(id, updatedBook, existingBook);
        }

        // Удалить книгу
        public bool DeleteBook(int id)
        {
            return _books.TryRemove(id, out _);
        }

        // Импорт книг из CSV
        public (int Success, int Failed) ImportBooksFromCsvAsync(Stream csvStream)
        {
            int successCount = 0;
            int failedCount = 0;

            try
            {
                using var reader = new StreamReader(csvStream);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    HasHeaderRecord = true,
                    MissingFieldFound = null
                });

                var records = csv.GetRecords<BookCsvModel>();

                foreach (var record in records)
                {
                    try
                    {
                        var book = new Book
                        {
                            Title = record.Title,
                            Author = record.Author,
                            Genre = record.Genre,
                            Description = record.Description ?? string.Empty,
                            ISBN = record.ISBN ?? string.Empty,
                            PublicationYear = record.PublicationYear
                        };

                        AddBook(book);
                        successCount++;
                    }
                    catch
                    {
                        failedCount++;
                    }
                }
            }
            catch (Exception)
            {
                // Логирование ошибки
            }

            return (successCount, failedCount);
        }

        // Получить уникальные жанры
        public List<string> GetUniqueGenres()
        {
            return _books.Values
                .Select(b => b.Genre)
                .Distinct()
                .OrderBy(g => g)
                .ToList();
        }

        // Получить уникальных авторов
        public List<string> GetUniqueAuthors()
        {
            return _books.Values
                .Select(b => b.Author)
                .Distinct()
                .OrderBy(a => a)
                .ToList();
        }
    }

    // Модель для импорта из CSV
    public class BookCsvModel
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? PublicationYear { get; set; }
        public string? ISBN { get; set; }
    }
} 