namespace Fintech.Api.DTOs
{
    public class LogoutRequest
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpireDate { get; set; } = DateTime.Now;
    }
}