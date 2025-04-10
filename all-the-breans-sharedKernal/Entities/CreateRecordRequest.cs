namespace all_the_breans_sharedKernal.Entities
{
    public class CreateRecordRequest
    {
        public required string Name { get; set; }
        
        public required string Country { get; set; }
        
        public required string Cost { get; set; }

        public string Image { get; set; }

        public string Color { get; set; }

        public string Description { get; set; }
    }
}
