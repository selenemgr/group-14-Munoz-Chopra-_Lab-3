using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace group_14_Munoz_Chopra__Lab_3.Models
{
    public class Podcast
    {
        [Key]
        public int PodcastID { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [ForeignKey("Creator")]
        public int CreatorID { get; set; }
        public User Creator { get; set; }

        public DateTime CreatedDate { get; set; }


        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
    }
}
