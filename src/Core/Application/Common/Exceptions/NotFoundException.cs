using System.Net;

namespace UNIFIEDDASHBOARD.WebApi.Application.Common.Exceptions;

public class NotFoundException : CustomException
{
    public NotFoundException(string message)
        : base(message, null, HttpStatusCode.NotFound)
    {
    }
}