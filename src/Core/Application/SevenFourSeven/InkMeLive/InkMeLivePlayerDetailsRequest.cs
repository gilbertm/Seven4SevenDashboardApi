using Microsoft.AspNetCore.Http;
using UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.InkMeLive;

namespace UNIFIEDDASHBOARD.WebApi.Application.SevenFourSeven.InkMeLive;

public class InkMeLiveMedicalListModel
{
    public bool I_am_not_Pregnant_or_Breast_feeder { get; set; }
    public bool I_am_not_a_Diabetic_patient { get; set; }
    public bool I_do_not_have_Psoriasis_or_any_chronic_skin_disease { get; set; }
    public bool I_do_not_have_any_Blood_Disorders { get; set; }
    public bool I_am_not_under_any_treatment_or_medications { get; set; }
}

public class InkMeLiveIDProof
{
    public byte[]? IDProofFrontSide { get; set; }
    public byte[]? IDProofBackSide { get; set; }
}

public class InkMeLivePlayerDetailsRequest
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string MobileNo { get; set; } = default!;
    public string PlayerUserName { get; set; } = default!;
    public string SocialCode { get; set; } = default!;
    public int Age { get; set; }
    public string FBprofilelink { get; set; } = default!;
    public InkMeLiveIDProof IdProofs { get; set; } = default!;
    public byte[]? DigitalSignature { get; set; }
    public InkMeLiveMedicalListModel ConfirmList { get; set; } = default!;

    public bool ResetPlayerStatusToNew { get; set; }
}

public class InkMeLivePlayerAgreementRequest
{
    public string PlayerUserName { get; set; } = default!;

    public string AgreementFileExtension { get; set; } = default!;

    public byte[] Agreement { get; set; } = default!;
}

public class InkMeLivePlayerAttachmentsRequest
{
    public string PlayerUserName { get; set; } = default!;

    public FileType FilesType { get; set; } = default!;

    public IReadOnlyList<IFormFile> Attachments { get; set; } = new List<IFormFile>();
}

public class InkMeLivePlayerSubmitAttachmentsRequest
{
    public string PlayerUserName { get; set; } = default!;

    public int StatusId { get; set; }
}

public class PlayerDetailsRequestValidator : CustomValidator<InkMeLivePlayerDetailsRequest>
{
    public PlayerDetailsRequestValidator(IStringLocalizer<PlayerDetailsRequestValidator> T)
    {
        RuleFor(a => a.FullName).Cascade(CascadeMode.Stop)
            .NotEmpty();
        RuleFor(a => a.Email).Cascade(CascadeMode.Stop)
            .NotEmpty();
        RuleFor(a => a.MobileNo).Cascade(CascadeMode.Stop)
            .NotEmpty();
        RuleFor(a => a.PlayerUserName).Cascade(CascadeMode.Stop)
            .NotEmpty();
        RuleFor(a => a.SocialCode).Cascade(CascadeMode.Stop)
            .NotEmpty();
        RuleFor(a => a.Age).Cascade(CascadeMode.Stop)
            .NotEmpty();
        RuleFor(a => a.IdProofs).Cascade(CascadeMode.Stop)
            .NotNull();
        RuleFor(a => a.ConfirmList).Cascade(CascadeMode.Stop)
            .NotNull();
        RuleFor(a => a.FBprofilelink).Cascade(CascadeMode.Stop)
            .NotNull();
    }
}