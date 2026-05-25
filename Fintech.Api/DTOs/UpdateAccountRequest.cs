namespace Fintech.Api.DTOs;
public class UpdateAccountRequest
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
    public decimal? Balance { get; set; }

}