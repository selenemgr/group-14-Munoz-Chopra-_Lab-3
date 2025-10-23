namespace group_14_Munoz_Chopra__Lab_3.Models.ViewModels
{
    public class EpisodeViewModel
    {
        public int EpisodeID { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public int DurationMinutes { get; set; }
        public int PlayCount { get; set; }
        public int NumberOfViews { get; set; }
        public string AudioFileURL { get; set; } = string.Empty;
    }

    public class PodcastDetailsViewModel
    {
        public int PodcastID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatorUsername { get; set; } = "Unknown";
        public DateTime CreatedDate { get; set; }
        public int SubscriberCount { get; set; }
        public bool IsSubscribed { get; set; } = false;

        public List<EpisodeViewModel> Episodes { get; set; } = new();
    }
}
