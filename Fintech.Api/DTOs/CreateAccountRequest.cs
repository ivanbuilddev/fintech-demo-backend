namespace Fintech.Api.DTOs
{
    public class CreateAccountRequest
    {
        public string Name { get; set; } = string.Empty;
        public Guid UserId{ get; set; }
        public string Currency { get; set; } = "EUR";
    }
}