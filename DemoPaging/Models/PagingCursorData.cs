using DemoPaging.Entities;
using System.Collections.Generic;

namespace DemoPaging.Models
{
    public class PagingCursorData
    {
        public List<Item> Items { get; set; } = new List<Item>();
        public int Total { get; set; }
        public long? PreviousPage { get; set; }
        public long? NextPage { get; set; }
    }
}
