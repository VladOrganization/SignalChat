using FluentValidation;

namespace SignalChat.Backend.Features.Chat.CreateDirectDialog;

public class CreateDirectDialogValidator : AbstractValidator<CreateDirectDialogCommand>
{
    public CreateDirectDialogValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty();
        RuleFor(x => x.PartnerId).NotEmpty();
        RuleFor(x => x.PartnerId)
            .NotEqual(x => x.CurrentUserId)
            .WithMessage("Нельзя создать личный диалог с самим собой");
    }
}
