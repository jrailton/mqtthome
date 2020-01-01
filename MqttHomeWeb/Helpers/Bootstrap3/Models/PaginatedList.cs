using System;
using System.Collections.Generic;
using System.Linq;

namespace Softwarehouse.Bootstrap3
{
    public class PaginatedList<T> : List<T>
    {
        /// <summary>
        /// The page number being requested. Default is 1.
        /// </summary>
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }

        /// <summary>
        /// The number of listings per page. Default is 200. Maximum is 200.
        /// </summary>
        public int PerPage { get; set; } = 200;
        public int TotalListings { get; set; }

        public PaginatedList()
        {
        }

        public PaginatedList(IQueryable<T> items, int page = 1, int pageSize = 200)
        {
            if (page < 1)
                page = 1;

            if (pageSize > 200)
                pageSize = 200;

            Page = page;
            TotalListings = items.Count();
            PerPage = pageSize;
            TotalPages = (int)Math.Ceiling((decimal)TotalListings / PerPage);

            AddRange(items.Skip((page - 1) * pageSize).Take(pageSize));
        }

        public bool HasPreviousPage => (Page > 1);

        public bool HasNextPage => (Page < TotalPages);
    }
}
