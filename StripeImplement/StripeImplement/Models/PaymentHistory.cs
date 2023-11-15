using System;
using System.Collections.Generic;

namespace StripeImplement.Models
{
    public class PaymentHistory
    {
        public int Id { get; set; }
        public string ChargeId { get; set; } // Use int? to represent nullable int
        public string PaidAmount { get; set; }
        public int? CustomerId { get; set; } // Use int? to represent nullable int
        public int? ButlerId { get; set; }   // Use int? to represent nullable int
        public int? AdminId { get; set; }    // Use int? to represent nullable int
        public int OrderId { get; set; }
        public string AmountReleased { get; set; }
        public string ReleasedStatus { get; set; }
    }
}
