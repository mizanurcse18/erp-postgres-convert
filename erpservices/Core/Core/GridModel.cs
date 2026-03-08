using System.Collections;

namespace Core
{
    public class GridModel
    {
        public IEnumerable Rows { get; set; }
        public int Total { get; set; }
        public bool IsSubmittedFromPopup { get; set; }
    }
}