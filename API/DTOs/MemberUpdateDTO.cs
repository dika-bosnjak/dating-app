namespace API.DTOs
{
    //MemberUpdateDTO is used to store info that is updated through UI
    public class MemberUpdateDTO
    {
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}