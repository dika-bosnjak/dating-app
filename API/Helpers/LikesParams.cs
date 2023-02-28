namespace API.Helpers
{
    //like params extend the pagination params
    public class LikesParams : PaginationParams
    {
        //set the user id that is requested
        public int UserId { get; set; }
        //set the predicate (liked, liked by)
        public string Predicate { get; set; }
    }
}