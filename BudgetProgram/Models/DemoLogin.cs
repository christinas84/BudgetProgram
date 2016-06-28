using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BudgetProgram.Models
{
    public class DemoLogin
    {
        [Required]
        [EmailAddress]
        public string GuestEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string GuestPassword { get; set; }
    }
}