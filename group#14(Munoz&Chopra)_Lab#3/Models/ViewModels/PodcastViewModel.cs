namespace group_14_Munoz_Chopra__Lab_3.Models.ViewModels
{
    public class PodcastViewModel
    {
        public int PodcastID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatorUsername { get; set; } = "Unknown";
        public int SubscriberCount { get; set; }
    }
}
