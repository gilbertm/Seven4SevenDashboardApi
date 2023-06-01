namespace UNIFIEDDASHBOARD.WebApi.Domain.Common;

public enum InputOutputResourceStatusType : sbyte
{
    Disabled = 0, // temporarily uploaded
    Enabled = 1, // enabled
    EnabledAndVerified = 2, // enabled and verified / checked by system keepers
    SoftDeleted = 5, // deleted or subject for removal / denied uses this also
    HardDeleted = 6 // additional reinforcement to the multinenancy no delete. This will be used to completely remove physically the reosurce from the system.
}