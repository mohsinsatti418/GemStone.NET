namespace GemStonesApi.Models
{
    public class GemStoneImage
    {
        public int Id { get; set; }
        public int GemStoneId { get; set; }
        public string ImageUrl { get; set; }
        public string ImageName { get; set; }
        public bool IsThumbnail { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}