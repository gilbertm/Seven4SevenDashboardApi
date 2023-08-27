using System.ComponentModel;

namespace UNIFIEDDASHBOARD.WebApi.Domain.Common;

public enum FileType
{
    [Description(".jpg,.png,.jpeg")]
    Image,

    [Description(".avi, .mov, .hevc")]
    Video = 2,

    [Description(".txt,.pdf,.docx")]
    File = 3
}