namespace StripeImplement.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderTitle { get; set; }
        public string OrderDescription { get; set; }
        public long OrderPrice { get; set; }
        public string ChargeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string paidBy { get; set; }
        public int? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? BuyerId { get; set; }
        public int? SellerId { get; set; }


    }
}
