using FluentValidation;

namespace SignalChat.Backend.Features.Auth.Refresh;

public class RefreshValidator : AbstractValidator<RefreshCommand>
{
    public RefreshValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}
