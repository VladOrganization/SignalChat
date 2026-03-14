namespace SignalChat.Backend.Database.Entities
{
    public class Images
    {
        public Guid id { get; set; }
        public string ImageUrl {  get; set; }

        public Guid MessageId { get; set; }
    }
}
