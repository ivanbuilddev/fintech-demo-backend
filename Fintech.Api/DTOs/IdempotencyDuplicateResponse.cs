namespace Fintech.Api.DTOs;
public class IdempotencyDuplicateResponse
{
    public int StatusCode { get; set; }
    public string ReponseBody { get; set; } = "";
}