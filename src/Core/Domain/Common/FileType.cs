using System.ComponentModel;

namespace RAFFLE.WebApi.Domain.Common;

public enum FileType
{
    [Description(".jpg,.png,.jpeg")]
    Image
}