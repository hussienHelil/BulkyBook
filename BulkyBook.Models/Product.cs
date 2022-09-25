using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string  Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        [ValidateNever]
        public string ImageUrl { get; set; }

        [ValidateNever]
        public virtual Category Category { get; set;}
        [Required]
        public int CategoryId { get; set; }
        [ValidateNever]
        public virtual CoverType CoverType { get; set;}
        [Required]
        public int CoverTypeId { get; set; }



    }
}
