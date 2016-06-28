using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BudgetProgram.Models
{
    
    public class Invite
    {
        public int Id { get; set; }

        [Display(Name = "Household Code for invite")]
        public string InviteCode { get; set; }

        [Required]
        public string InvitedUser { get; set; }

        [Required(ErrorMessage = "Please provide a valid email address for the person you want to invite.")]
        [EmailAddress]
        public string Email { get; set; }

        public string InviteSentBy { get; set; }

        public DateTimeOffset InviteDate { get; set; }

        public int HouseHoldId { get; set; }

        public virtual HouseHolds HouseHold { get; set; }


    }
}