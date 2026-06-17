namespace ContractManagement.API.DTOs
{
    public class LoginDto
    {
        public string username { get; set; } = string.Empty;  // lowercase to match JSON
        public string password { get; set; } = string.Empty;  // lowercase to match JSON
    }
}