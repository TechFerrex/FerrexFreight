namespace FerrexWeb.Models
{
    public class StopDto
    {
        public string Address { get; set; } = string.Empty;
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }

        public StopDto() { }

        public StopDto(string address, decimal lat, decimal lng)
        {
            Address = address;
            Lat = lat;
            Lng = lng;
        }
    }

}
