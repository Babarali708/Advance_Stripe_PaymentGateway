namespace StripeImplement.Models
{
    public class Packages
    {
        public int Id { get; set; }
        public string PakageType { get; set; }
        public int? SellerId { get; set;}
        public int? BuyerId { get; set;}
        public int? PackagesCount { get; set;}
        public int? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public User Buyer { get; set; }


    }
}
