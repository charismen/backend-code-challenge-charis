namespace ShipManagement.API.Auth
{
    public class AuthUserOptions
    {
        public IList<AuthUser> Users { get; set; } = new List<AuthUser>();
    }

    public class AuthUser
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
