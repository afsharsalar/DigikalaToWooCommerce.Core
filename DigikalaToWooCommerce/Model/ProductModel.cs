using System.Collections.Generic;

namespace DigikalaToWooCommerce.Core.Model
{
    public class ProductModel
    {
        public int Dkp { get; set; }

        public string Title { get; set; }

        public List<VariantModel> Data { get; set; }

    }


    public class VariantModel
    {
        public int id { get; set; }
        public bool active { get; set; }
        public bool active_digistyle { get; set; }
        public string title { get; set; }
        public string site { get; set; }
        public Warranty warranty { get; set; }

        public Size size { get; set; }

        public Color color { get; set; }
        public MarketplaceSeller marketplace_seller { get; set; }
        public int leadTime { get; set; }
        public List<object> gifts { get; set; }
        public List<object> gift_product_ids { get; set; }
        public bool scheduled_stock { get; set; }
        public object promotion_price_id { get; set; }
        public bool is_digikala_owner { get; set; }
        public double? rank { get; set; }
        public object sr { get; set; }
        public bool fast_shopping_badge { get; set; }
        public bool fast_shopping_confirm { get; set; }
        public bool is_multi_warehouse { get; set; }
        public object stats { get; set; }
        public bool available_on_website { get; set; }
        public PriceList price_list { get; set; }
        public string addToCartUrl { get; set; }
        public string addToYaldaCartUrl { get; set; }
        public int dcPoint { get; set; }
    }

    public class PriceList
    {
        public int id { get; set; }
        public object discount_percent { get; set; }
        public int rrp_price { get; set; }
        public int selling_price { get; set; }
        public bool is_incredible_offer { get; set; }
        public bool is_sponsored_offer { get; set; }
        public object promotion_id { get; set; }
        public object timer { get; set; }
        public bool pre_sell { get; set; }
        public int variant_id { get; set; }
        public int orderLimit { get; set; }
        public object initial_limit { get; set; }
        public object tags { get; set; }
        public int discount_amount { get; set; }
        public object discount { get; set; }
        public bool show_discount_badge { get; set; }
    }

    public class MarketplaceSeller
    {
        public int id { get; set; }
        public string name { get; set; }
        public int rate { get; set; }
        public int rateCount { get; set; }
        public Rating rating { get; set; }
        public double stars { get; set; }
        public bool is_trusted { get; set; }
        public bool is_official_seller { get; set; }
        public string url { get; set; }
        public string registerTimeAgo { get; set; }
    }

    public class Rating
    {
        public double cancel_percentage { get; set; }
        public string cancel_summarize { get; set; }
        public double return_percentage { get; set; }
        public string return_summarize { get; set; }
        public double ship_on_time_percentage { get; set; }
        public string ship_on_time_summarize { get; set; }
        public double final_score { get; set; }
        public double final_percentage { get; set; }
    }

    public class Color
    {
        public int id { get; set; }
        public string title { get; set; }
        public string code { get; set; }
        public string hexCode { get; set; }
        public string hex_code { get; set; }
    }
    public class Warranty
    {
        public int id { get; set; }
        public string title { get; set; }
        public object description { get; set; }
    }

    public class Size
    {
        public int id { get; set; }
        public string title { get; set; }
        public int sort { get; set; }
    }
}
