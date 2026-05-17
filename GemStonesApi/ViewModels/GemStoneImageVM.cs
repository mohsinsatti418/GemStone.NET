namespace GemStonesApi.ViewModels
{
    public class GemStoneImageVM
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string ImageName { get; set; }
        public bool IsThumbnail { get; set; }
        public int DisplayOrder { get; set; }
    }
}