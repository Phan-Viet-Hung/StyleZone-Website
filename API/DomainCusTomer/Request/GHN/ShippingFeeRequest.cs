namespace API.DomainCusTomer.Request.GHN
{
    public class ShippingFeeRequest
    {
        public int service_id { get; set; } = 53320;
        public int insurance_value { get; set; }
        public int from_district_id { get; set; } = 1482;
        public int to_district_id { get; set; }
        public string to_ward_code { get; set; }
        public int height { get; set; } = 5;    
        public int length { get; set; } = 30;   
        public int weight { get; set; } = 300;  
        public int width { get; set; } = 25;   
    }
}
