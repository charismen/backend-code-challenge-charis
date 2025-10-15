namespace ShipManagement.API.Models
{
    public class CrewMember
    {
        public string CrewMemberId { get; set; } = string.Empty;
        public string ShipCode { get; set; } = string.Empty;
        public string RankName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime SignOnDate { get; set; }
        public DateTime? SignOffDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CrewListRequest
    {
        public string ShipCode { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortColumn { get; set; } = "RankName";
        public bool SortDescending { get; set; } = false;
        public string? StatusFilter { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
