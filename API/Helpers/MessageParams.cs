namespace API.Helpers
{
    //messageparams extend pagination parameters
    public class MessageParams : PaginationParams
    {
        //set the requested username
        public string Username { get; set; }
        //set the requested messages container (unread, inbox, outbox)
        public string Container { get; set; } = "Unread";

    }
}