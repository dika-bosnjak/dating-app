namespace API.DTOs
{
    //LikeDTO is used to get user data from likes table
    public class LikeDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int Age { get; set; }
        public string KnownAs { get; set; }
        public string PhotoUrl { get; set; }
        public string City { get; set; }
        public string Gender { get; set; }
    }
}