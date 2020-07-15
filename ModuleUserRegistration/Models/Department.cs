using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ModuleUserRegistration.Models
{
    public class Department
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Deparment Name")]
        public string DepartmentName { get; set; }
    }
}
