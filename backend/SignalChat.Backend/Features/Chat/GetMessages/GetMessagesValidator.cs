using FluentValidation;

namespace SignalChat.Backend.Features.Chat.GetMessages;

public class GetMessagesValidator : AbstractValidator<GetMessagesQuery>
{
    public GetMessagesValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.DialogId).NotEmpty();
        RuleFor(x => x.CurrentUserIde).NotEmpty();
    }
}
