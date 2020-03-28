using DemoPaging.Entities;
using System.Collections.Generic;

namespace DemoPaging.Models
{
    public class PagingOffsetData
    {
        public List<Item> Items { get; set; } = new List<Item>();
        public int Total { get; set; }
    }
}
