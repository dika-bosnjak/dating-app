namespace API.DTOs
{
    //PhotoDTO is ued to store main info about photo (used in displaying photos)
    public class PhotoDTO
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
    }
}