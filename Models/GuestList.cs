using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Models
{
    public class GuestList
    {
        [Key]
        public int GuestListId { get; set; }

        public int UserId { get; set; }

        public int WeddingId { get; set; }

        public User WeddingGuest { get; set; }

        public Wedding Event { get; set; }
    }
}