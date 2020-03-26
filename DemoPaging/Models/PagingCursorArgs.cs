using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoPaging.Models
{
    public class PagingCursorArgs
    {
        public int PageCursor { get; set; }

        public int PageSize { get; set; }
    }
}