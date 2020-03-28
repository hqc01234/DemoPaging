using System.Text.Json;

namespace DemoPaging.Models
{
    public class PagingCursorArgs
    {
        public long? PageCursor { get; set; }

        public int PageSize { get; set; }
    }
}