namespace group_14_Munoz_Chopra__Lab_3.Models
{
    public class Podcast
    {
        public int PodcastID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CreatorID { get; set; }
        public DateTime CreatedDate { get; set; }

        public ICollection<Episode> Episodes { get; set; }
    }
}
