using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models.ViewModels
{
    public class CartVM
    {
        public IEnumerable<Cart> ListCart { get; set; }
        public OrderHeader OrderHeader { get; set; }   
    }
}
