using System.Collections.Generic;

namespace Ceramista.Dnn.Ceramista.Dnn.HelloWorld.Models
{
    public class ProductViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public List<string> CategoryIds { get; set; }
        public bool IsFeatured { get; set; }

        public ProductViewModel()
        {
            CategoryIds = new List<string>();
        }
    }
}
