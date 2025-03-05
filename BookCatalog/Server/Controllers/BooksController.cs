using BookCatalog.Server.Services;
using BookCatalog.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BookCatalog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BookService _bookService;
        private readonly ILogger<BooksController> _logger;
        private static readonly SemaphoreSlim _rateLimiter = new(10, 10); // Максимум 10 одновременных запросов

        public BooksController(BookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        // Получить список книг с пагинацией и фильтрацией
        [HttpGet]
        public async Task<ActionResult<PagedResult<Book>>> GetBooks([FromQuery] BookQuery query)
        {
            try
            {
                // Применяем ограничение скорости запросов
                if (!await _rateLimiter.WaitAsync(TimeSpan.FromSeconds(2)))
                {
                    return StatusCode((int)HttpStatusCode.TooManyRequests, "Rate limit exceeded. Please try again later.");
                }

                try
                {
                    var result = _bookService.GetBooks(query);
                    return Ok(result);
                }
                finally
                {
                    _rateLimiter.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting books");
                return StatusCode(500, "Internal server error");
            }
        }

        // Получить книгу по ID
        [HttpGet("{id}")]
        public ActionResult<Book> GetBook(int id)
        {
            var book = _bookService.GetBookById(id);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }

        // Добавить новую книгу
        [HttpPost]
        public ActionResult<Book> AddBook(Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addedBook = _bookService.AddBook(book);
            return CreatedAtAction(nameof(GetBook), new { id = addedBook.Id }, addedBook);
        }

        // Обновить существующую книгу
        [HttpPut("{id}")]
        public IActionResult UpdateBook(int id, Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = _bookService.UpdateBook(id, book);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // Удалить книгу
        [HttpDelete("{id}")]
        public IActionResult DeleteBook(int id)
        {
            var success = _bookService.DeleteBook(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // Импорт книг из CSV файла
        [HttpPost("import")]
        public IActionResult ImportBooks(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only CSV files are supported");
            }

            try
            {
                using var stream = file.OpenReadStream();
                var result = _bookService.ImportBooksFromCsvAsync(stream);
                
                return Ok(new { 
                    Message = $"Import completed. {result.Success} books imported successfully, {result.Failed} failed." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing books from CSV");
                return StatusCode(500, "Error importing books");
            }
        }

        // Получить уникальные жанры
        [HttpGet("genres")]
        public ActionResult<List<string>> GetGenres()
        {
            return Ok(_bookService.GetUniqueGenres());
        }

        // Получить уникальных авторов
        [HttpGet("authors")]
        public ActionResult<List<string>> GetAuthors()
        {
            return Ok(_bookService.GetUniqueAuthors());
        }
    }
} 