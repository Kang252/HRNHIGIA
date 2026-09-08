using NHIGIA.Core.Domain.TypedEntities;
using System.Collections.Generic;

namespace NHIGIA.Core.Helper
{
    public class PagingData
    {
        public PagingData() : this(0, 0, null, null)
        {
        }

        public PagingData(int pageIndex, int pageSize) : this(pageIndex, pageSize, null, null)
        {
        }

        public PagingData(int pageIndex, int pageSize, List<TypeFilterDescriptor> filterColumns) : this(pageIndex, pageSize, filterColumns, null)
        {
        }

        public PagingData(int pageIndex, int pageSize, List<TypeSortDescriptor> sortColumns) : this(pageIndex, pageSize, null, sortColumns)
        {
        }

        public PagingData(int pageIndex, int pageSize, List<TypeFilterDescriptor> filterColumns, List<TypeSortDescriptor> sortColumns)
        {
            PageIndex = pageIndex;
            Offset = (pageIndex - 1) * pageSize;
            Length = pageSize;
            FilterColumns = filterColumns ?? new List<TypeFilterDescriptor>();
            SortColumns = sortColumns ?? new List<TypeSortDescriptor>();
        }

        public int PageIndex { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public List<TypeFilterDescriptor> FilterColumns { get; set; }
        public List<TypeSortDescriptor> SortColumns { get; set; }
    }

    public class PagingData<T> : PagingData
    {
        public PagingData() : this(0, 0, default(T), null, null)
        {
        }
        public PagingData(int pageIndex, int pageSize) : this(pageIndex, pageSize, default(T), null, null)
        {
        }
        public PagingData(int pageIndex, int pageSize, T searchCriteria) : this(pageIndex, pageSize, searchCriteria, null, null)
        {
        }

        public PagingData(int pageIndex, int pageSize, T searchCriteria, List<TypeFilterDescriptor> filterColumns) : this(pageIndex, pageSize, searchCriteria, filterColumns, null)
        {
        }

        public PagingData(int pageIndex, int pageSize, T searchCriteria, List<TypeSortDescriptor> sortColumns) : this(pageIndex, pageSize, searchCriteria, null, sortColumns)
        {
        }

        public PagingData(int pageIndex, int pageSize, T searchCriteria, List<TypeFilterDescriptor> filterColumns, List<TypeSortDescriptor> sortColumns) : base(pageIndex, pageSize, filterColumns, sortColumns)
        {
            SearchCriteria = searchCriteria;
        }

        public T SearchCriteria { get; set; }
    }
}
