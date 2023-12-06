namespace MsgExtProductSupportSSOCSharp.Models
{
    public class Product
    {
        public string Title { get; set; }
        public string RetailCategory { get; set; }
        public Link Specguide { get; set; }
        public string PhotoSubmission { get; set; }
        public double CustomerRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Id { get; set; }
        public string ContentType { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
    }
    public class Link
    {
        public string Description { get; set; }
        public string Url { get; set; }
    }
}
