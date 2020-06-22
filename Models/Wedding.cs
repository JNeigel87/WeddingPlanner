using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Models
{
    public class Wedding
    {
        [Key]
        public int WeddingId { get; set; }

        [Required (ErrorMessage = "Name is required")]
        [MinLength(2,ErrorMessage="Name must be at least 2 characters")]
        public string WedderOne { get; set; }

        [Required (ErrorMessage = "Name is required")]
        [MinLength(2,ErrorMessage="Name must be at least 2 characters")]
        public string WedderTwo { get; set; }

        [Required (ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        [FutureDate]
        public DateTime Date { get; set; }

        [Required (ErrorMessage = "Location is required")]
        public string Location { get; set; }

        [Required (ErrorMessage = "Address is required")]
        public string Address { get; set; }

        public int UserId { get; set; }

        public User Planner { get; set; }

        public List<GuestList> Guests { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public class FutureDateAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if ((DateTime) value < DateTime.Now)
                    return new ValidationResult("Date must be in the future!");
                return ValidationResult.Success;
            }
        }
    }
}