using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Webapi.Core
{
    /// <summary>
    /// Paged list interface
    /// </summary>
    public interface IPagedList<T>
    {
        /// <summary>
        /// Page index
        /// </summary>
        int PageIndex { get; }
        /// <summary>
        /// Page size
        /// </summary>
        int PageSize { get; }
        /// <summary>
        /// Total count
        /// </summary>
        int TotalCount { get; }
        /// <summary>
        /// Total pages
        /// </summary>
        int TotalPages { get; }
        /// <summary>
        /// Has previous page
        /// </summary>
        bool HasPreviousPage { get; }
        /// <summary>
        /// Has next age
        /// </summary>
        bool HasNextPage { get; }

        IList<T> Data { get; }
    }

    /// <summary>
    /// Paged list
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    [Serializable]
    public class PagedList<T> : IPagedList<T>
    {
        public PagedList() { }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        public PagedList(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var total = source.Count();
            this.TotalCount = total;
            this.TotalPages = total / pageSize;

            if (total % pageSize > 0)
                TotalPages++;

            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.Data = source.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }
        public IList<T> Data { get; } = new List<T>();
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        public PagedList(IList<T> source, int pageIndex, int pageSize)
        {
            TotalCount = source.Count();
            TotalPages = TotalCount / pageSize;

            if (TotalCount % pageSize > 0)
                TotalPages++;

            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.Data = source.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalCount">Total count</param>
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            TotalCount = totalCount;
            TotalPages = TotalCount / pageSize;

            if (TotalCount % pageSize > 0)
                TotalPages++;

            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.Data = source.ToList();
        }

        /// <summary>
        /// Page index
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// Total pages
        /// </summary>
        public int TotalPages { get; set; }
        /// <summary>
        /// Has previous page
        /// </summary>
        public bool HasPreviousPage
        {
            get { return (PageIndex > 0); }
        }
        /// <summary>
        /// Has next page
        /// </summary>
        public bool HasNextPage
        {
            get { return (PageIndex + 1 < TotalPages); }
        }
    }
}
