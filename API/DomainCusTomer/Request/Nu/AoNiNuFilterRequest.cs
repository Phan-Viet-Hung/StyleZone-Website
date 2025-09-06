namespace API.DomainCusTomer.Request.Nu
{
    public class AoNiNuFilterRequest
    {
        public List<string>? Product { get; set; }
        public List<string>? Colors { get; set; }
        public List<string>? Sizes { get; set; }
        public List<string>? Genders { get; set; }
        public string SortBy { get; set; } = "createdat";
        public string SortOrder { get; set; } = "desc";

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
