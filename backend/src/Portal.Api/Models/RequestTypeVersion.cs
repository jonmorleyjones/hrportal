namespace Portal.Api.Models;

public class RequestTypeVersion
{
    public Guid Id { get; set; }
    public Guid RequestTypeId { get; set; }
    public int VersionNumber { get; set; }
    public string FormJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public RequestType RequestType { get; set; } = null!;
    public ICollection<RequestResponse> Responses { get; set; } = new List<RequestResponse>();
}
