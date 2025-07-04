using System.Net;

namespace AuthService.Domain.Exceptions;

public class ErrorResponse
{
    public string Title { get; set; }
    public int Status { get; set; }
    public string Details { get; set; }
    public IEnumerable<Exception> Errors { get; set; }
}