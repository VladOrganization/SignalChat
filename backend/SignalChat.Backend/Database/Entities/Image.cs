namespace SignalChat.Backend.Database.Entities
{
    public class Image
    {
        public Guid Id { get; set; }
        public string ImageUrl {  get; set; }
        public Guid MessageId { get; set; }
        public Message Message { get; set; }
    }
}
