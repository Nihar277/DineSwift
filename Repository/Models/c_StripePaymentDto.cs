namespace Repository.Model
{
    
    public class StripePaymentDto
    {
        public long Amount { get; set; } // in paise (₹100 = 10000)
        public string Currency { get; set; } = "inr";

        public int c_customerid { get; set; }
        public int c_restaurantid { get; set; }

        public string c_housenumber { get; set; }
        public string c_societyname { get; set; }
        public string c_landmark { get; set; }
        public string c_city { get; set; }
        public string c_state { get; set; }

        public int c_quantity { get; set; }
        public string c_dishname { get; set; }
        public string c_foodimage { get; set; }

        public int c_fooditemid { get; set; }
    }
}