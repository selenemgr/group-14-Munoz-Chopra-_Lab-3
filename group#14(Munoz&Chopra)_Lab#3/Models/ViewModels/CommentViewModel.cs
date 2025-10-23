namespace group_14_Munoz_Chopra__Lab_3.Models.ViewModels
{
    public class CommentViewModel
    {
        public int CommentID { get; set; }
        public int EpisodeID { get; set; }
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
