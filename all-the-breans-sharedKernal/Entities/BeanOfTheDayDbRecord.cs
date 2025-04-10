using System.ComponentModel.DataAnnotations;

namespace all_the_breans_sharedKernal.Entities
{
    public class BeanOfTheDayDbRecord
    {
        [Key]
        public required Guid _id { get; set; }

        public int BeanIndex { get; set; }

        public DateTime Date { get; set; }
    }
}
