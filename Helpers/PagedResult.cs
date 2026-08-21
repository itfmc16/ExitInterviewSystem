namespace ExitInterviewSystem.Helpers
{
    /// <summary>
    /// Simple paged result wrapper used by all list screens.
    /// </summary>
    public class PagedResult<T> : IPagerInfo
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }

        public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
        public int FromRecord => TotalCount == 0 ? 0 : ((Page - 1) * PageSize) + 1;
        public int ToRecord => Math.Min(Page * PageSize, TotalCount);

        public static async Task<PagedResult<T>> CreateAsync(
            IQueryable<T> query, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 200) pageSize = 200;

            var total = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .CountAsync(query);

            var items = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .ToListAsync(query.Skip((page - 1) * pageSize).Take(pageSize));

            return new PagedResult<T>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        /// <summary>In-memory paging (e.g. AD search results already loaded).</summary>
        public static PagedResult<T> FromList(IList<T> source, int page, int pageSize, int? totalOverride = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            var total = totalOverride ?? source.Count;
            var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new PagedResult<T>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }
    }
}
