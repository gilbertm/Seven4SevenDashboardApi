using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.InkMeLive;
public class PlayersModel
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? PlayerEmail { get; set; }
    public string? PlayerMobile { get; set; }
    public string? PlayerUserName { get; set; }
    public string? SocialCode { get; set; }
    public int Age { get; set; }
    public string? FBprofilelink { get; set; }
    public bool IsEmailVerify { get; set; }
    public bool IsMobileVerify { get; set; }
    public int PlayerStatusId { get; set; }
    public string? PlayerStatus { get; set; }
    public string? Note { get; set; }
    public string? EncryptKey { get; set; }
    public string? IDProofFrontSide { get; set; }
    public string? IDProofFrontSidePath { get; set; }
    public string? IDProofBackSide { get; set; }
    public string? IDProofBackSidePath { get; set; }
    public string? DigitalSignature { get; set; }
    public string? DigitalSignaturePath { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime ModifiedOn { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime ActionTakenOn { get; set; }
    public int ActionTakenBy { get; set; }
    public PlayerConfirm? ObjPlayerConfirm { get; set; }
}

public class PlayerConfirm
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public bool IsPregnantOrBreastFeeder { get; set; }
    public bool IsDiabeticPatient { get; set; }
    public bool IsPsoriasisOrAnyChronicSkinDisease { get; set; }
    public bool IsBloodDisorders { get; set; }
    public bool IsTreatmentOrMedications { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedOn { get; set; }
}