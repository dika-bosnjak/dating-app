namespace API.DTOs
{
    //CreateMessageDTO is used when the message is created on clients side
    public class CreateMessageDTO
    {
        public string RecipientUsername { get; set; }
        public string Content { get; set; }
    }
}