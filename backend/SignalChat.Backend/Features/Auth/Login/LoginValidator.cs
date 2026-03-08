using FluentValidation;

namespace SignalChat.Backend.Features.Auth.Login;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6)
            .WithMessage("Код должен содержать 6 символов");
    }
}
