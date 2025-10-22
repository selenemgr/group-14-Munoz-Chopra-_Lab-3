namespace group_14_Munoz_Chopra__Lab_3.Models
{
    public class Episode
    {
        public int EpisodeID { get; set; }
        public int PodcastID { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int DurationMinutes { get; set; }
        public int PlayCount { get; set; }
        public string AudioFileURL { get; set; }
        public int NumberOfViews { get; set; }
    }
}
