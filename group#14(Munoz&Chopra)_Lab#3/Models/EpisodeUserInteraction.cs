using System.ComponentModel.DataAnnotations;

namespace group_14_Munoz_Chopra__Lab_3.Models
{
    public class EpisodeUserInteraction
    {
        [Key]
        public int InteractionID { get; set; }

        [Required]
        public int EpisodeID { get; set; }

        [Required]
        public int UserID { get; set; }

        public bool Viewed { get; set; } = false;

        public bool Played { get; set; } = false;
    }
}
