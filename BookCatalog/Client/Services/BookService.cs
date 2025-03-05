using BookCatalog.Shared;
using System.Net.Http.Json;

namespace BookCatalog.Client.Services
{
    public class BookService
    {
        private readonly HttpClient _http;

        public BookService(HttpClient http)
        {
            _http = http;
        }

        // Получить список книг с пагинацией и фильтрацией
        public async Task<PagedResult<Book>> GetBooksAsync(BookQuery query)
        {
            try
            {
                // Формируем URL с параметрами запроса
                var url = $"api/books?page={query.Page}&pageSize={query.PageSize}";
                
                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                    url += $"&searchTerm={Uri.EscapeDataString(query.SearchTerm)}";
                
                if (!string.IsNullOrWhiteSpace(query.Author))
                    url += $"&author={Uri.EscapeDataString(query.Author)}";
                
                if (!string.IsNullOrWhiteSpace(query.Genre))
                    url += $"&genre={Uri.EscapeDataString(query.Genre)}";
                
                if (!string.IsNullOrWhiteSpace(query.SortBy))
                    url += $"&sortBy={Uri.EscapeDataString(query.SortBy)}&sortAscending={query.SortAscending}";

                var result = await _http.GetFromJsonAsync<PagedResult<Book>>(url);
                return result ?? new PagedResult<Book>();
            }
            catch
            {
                // В случае ошибки возвращаем пустой результат
                return new PagedResult<Book>();
            }
        }

        // Получить книгу по ID
        public async Task<Book?> GetBookByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<Book>($"api/books/{id}");
            }
            catch
            {
                return null;
            }
        }

        // Добавить новую книгу
        public async Task<Book?> AddBookAsync(Book book)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/books", book);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Book>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        // Обновить существующую книгу
        public async Task<bool> UpdateBookAsync(int id, Book book)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/books/{id}", book);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Удалить книгу
        public async Task<bool> DeleteBookAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/books/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Импорт книг из CSV файла
        public async Task<string> ImportBooksFromCsvAsync(Stream fileStream, string fileName)
        {
            try
            {
                var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(fileStream);
                content.Add(streamContent, "file", fileName);

                var response = await _http.PostAsync("api/books/import", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ImportResult>();
                    return result?.Message ?? "Import completed successfully";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return $"Error: {error}";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Получить уникальные жанры
        public async Task<List<string>> GetGenresAsync()
        {
            try
            {
                var genres = await _http.GetFromJsonAsync<List<string>>("api/books/genres");
                return genres ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        // Получить уникальных авторов
        public async Task<List<string>> GetAuthorsAsync()
        {
            try
            {
                var authors = await _http.GetFromJsonAsync<List<string>>("api/books/authors");
                return authors ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }

    // Вспомогательный класс для результата импорта
    public class ImportResult
    {
        public string Message { get; set; } = string.Empty;
    }
} 