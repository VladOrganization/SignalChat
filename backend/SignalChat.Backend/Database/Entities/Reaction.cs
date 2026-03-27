using SignalChat.Backend.Database.Entities.Enums;

namespace SignalChat.Backend.Database.Entities
{
    public class Reaction
    {
        public Guid Id { get; set; }

        public ReactionEnum Emoji {  get; set; }

        public Message Message { get; set; }

        public Guid MessageId { get; set; }
        
        public User User { get; set; }
        public Guid UserId { get; set; }

    }
}
