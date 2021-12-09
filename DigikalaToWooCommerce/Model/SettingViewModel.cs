namespace DigikalaToWooCommerce.Core.Model
{
    public class SettingViewModel
    {
        public int  SellerId { get; set; }

        public int Discount { get; set; }

        public string WooCommerceApiKey { get; set; }
        public string WooCommerceApiPassword { get; set; }
        public string WooCommerceApiUrl { get; set; }
    }
}
