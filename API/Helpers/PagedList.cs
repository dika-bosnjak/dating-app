using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
    //PagedList class is used for displaying list of items with pagination
    public class PagedList<T> : List<T>
    {
        //constructor - items, count, page number and page size
        public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize); //number of all elements divided by the number of elements on one page
            PageSize = pageSize;
            TotalCount = count;
            AddRange(items); //add items in the list
        }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        //create list is used to create a pagedlist from the source for the specific page and exact number of elements
        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

    }
}