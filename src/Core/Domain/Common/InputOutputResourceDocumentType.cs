namespace RAFFLE.WebApi.Domain.Common;

public enum InputOutputResourceDocumentType : sbyte
{
    None = 0,

    Passport = 1,
    PassportBack = 10,

    NationalId = 2,
    NationalIdBack = 20,

    GovernmentId = 3,
    GovernmentIdBack = 30,

    SelfieWithAtLeastOneCard = 4
}