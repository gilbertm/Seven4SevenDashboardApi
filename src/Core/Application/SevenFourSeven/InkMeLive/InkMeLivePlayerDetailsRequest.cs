using RAFFLE.WebApi.Application.SevenFourSeven.InkMeLive;

namespace RAFFLE.WebApi.Application.SevenFourSeven.InkMeLive;

public class CustomListModel
{
    public bool I_am_not_Pregnant_or_Breast_feeder { get; set; }
    public bool I_am_not_a_Diabetic_patient { get; set; }
    public bool I_do_not_have_Psoriasis_or_any_chronic_skin_disease { get; set; }
    public bool I_do_not_have_any_Blood_Disorders { get; set; }
    public bool I_am_not_under_any_treatment_or_medications { get; set; }
}

public class IDProof
{
    public string IdProofFrontSide { get; set; } = default!;
    public string IdProofBackSide { get; set; } = default!;
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
    public IDProof IdProofs { get; set; } = default!;
    public string DigitalSignature { get; set; } = default!;
    public CustomListModel ConfirmList { get; set; } = default!;
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
    }
}