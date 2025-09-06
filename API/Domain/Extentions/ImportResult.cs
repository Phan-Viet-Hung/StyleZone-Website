namespace API.Domain.Extentions
{
    public class ImportResult<T>
    {
        public int AddedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<T> AddedItems { get; set; } = new();
        public List<T> SkippedItems { get; set; } = new();
    }

}
