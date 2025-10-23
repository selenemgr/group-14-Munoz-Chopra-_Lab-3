using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace group_14_Munoz_Chopra__Lab_3.Models
{
    public class Subscription
    {
        [Key]
        public int SubscriptionID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }
        public User User { get; set; }

        [ForeignKey("Podcast")]
        public int PodcastID { get; set; }
        public Podcast Podcast { get; set; }

        public DateTime SubscribedDate { get; set; }
    }
}
