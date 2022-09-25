using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Models.ModelView
{
    public class ProductViewModel
    {
        public Product Product { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> CoverTypes{ get; set; }
        [DisplayName("Image")]
        public IFormFile FormFile { get; set; }


        public static explicit operator ProductViewModel(Product product)
        {
            return new ProductViewModel
            {
                Product = product,
            };
        }
    }
}
