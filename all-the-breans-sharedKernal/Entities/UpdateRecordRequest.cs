using System.ComponentModel.DataAnnotations;

namespace all_the_breans_sharedKernal.Entities
{
    public class UpdateRecordRequest
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(50)]
        public string? Cost { get; set; }

        public string? Image { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
