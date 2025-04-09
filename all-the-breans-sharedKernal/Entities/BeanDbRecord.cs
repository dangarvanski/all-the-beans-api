using System.ComponentModel.DataAnnotations;

namespace all_the_breans_sharedKernal.Entities
{
    public class BeanDbRecord
    {
        [Key]
        public required string _id { get; set; }

        public int index { get; set; }

        public bool IsBOTD { get; set; }

        public string Cost { get; set; }
        
        public string Image { get; set; }
        
        public string Color { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public string Country { get; set; }
    }
}
