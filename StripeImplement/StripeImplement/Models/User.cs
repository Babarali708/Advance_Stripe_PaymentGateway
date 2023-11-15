using StripeImplement.Models;

namespace StripeImplement.Models
{
    public class User
    {
      
        public int Id { get; set; }
        public string Name { get; set; }
        public string StripeId { get; set; }
        public string Email { get; set; }
        public Nullable<int> Role { get; set; }
        public Nullable<int> IsActive { get; set; }
        public Nullable<System.DateTime> CreatedAt { get; set; }


    }
}



