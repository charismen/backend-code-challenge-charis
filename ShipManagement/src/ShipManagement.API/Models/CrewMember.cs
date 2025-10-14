namespace ShipManagement.API.Models
{
    public class CrewMember
    {
        public int Id { get; set; }
        public int ShipId { get; set; }
        public int RankId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime? SignOffDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RankName { get; set; } = string.Empty;
        public string ShipName { get; set; } = string.Empty;
    }

    public class CrewListRequest
    {
        public int ShipId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
        public string? StatusFilter { get; set; }
        public string? NameFilter { get; set; }
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