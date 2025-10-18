namespace ShipManagement.API.Models
{
    public class Ship
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FiscalYear { get; set; } = string.Empty;
        public bool Status { get; set; } = true;
    }
}
