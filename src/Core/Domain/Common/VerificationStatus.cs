namespace UNIFIEDDASHBOARD.WebApi.Domain.Common;
public enum VerificationStatus : byte
{
    Initial = 0,
    Submitted = 1,
    Error = 2,
    Verified = 100
}