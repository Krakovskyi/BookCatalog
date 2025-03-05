using System;

namespace BookCatalog.Shared
{
    public class BookQuery
    {
        // Параметры пагинации
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Параметры фильтрации
        public string? SearchTerm { get; set; }
        public string? Author { get; set; }
        public string? Genre { get; set; }

        // Параметры сортировки
        public string? SortBy { get; set; } = "Title";
        public bool SortAscending { get; set; } = true;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageCount => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
} 