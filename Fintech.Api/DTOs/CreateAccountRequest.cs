namespace Fintech.Api.DTOs;
public class CreateAccountRequest
{
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = "EUR";
}
