using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SaaS_Database_Architecture.Models
{
    public class NewField
    {
        
        public  string tablename { get; set; }
        [Required]
        public string fieldname { get; set; }
        [Required]
        public string fieldtypes { get; set; }

        public string fieldsize { get; set; }

        public string defaultvalue { get; set; }

        public bool allownull { get; set; }
    }
}