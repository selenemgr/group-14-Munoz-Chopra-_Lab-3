namespace group_14_Munoz_Chopra__Lab_3.Models.ViewModels
{
    public class EpisodeDetailsViewModel
    {
        public int EpisodeID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AudioFileURL { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public int DurationMinutes { get; set; }
        public int PlayCount { get; set; }
        public int NumberOfViews { get; set; }


        // Podcast reference for back button
        public int PodcastID { get; set; }
        public string PodcastTitle { get; set; } = string.Empty;

        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
    }
}
