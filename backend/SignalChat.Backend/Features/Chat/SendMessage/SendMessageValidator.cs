using FluentValidation;

namespace SignalChat.Backend.Features.Chat.SendMessage;

public class SendMessageValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
    }
}
