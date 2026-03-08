using FluentValidation;

namespace SignalChat.Backend.Features.Auth.Register;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .Matches(@"^[A-Za-z]{5,10}$")
            .WithMessage("Имя должно содержать от 5 до 10 букв");
    }
}
