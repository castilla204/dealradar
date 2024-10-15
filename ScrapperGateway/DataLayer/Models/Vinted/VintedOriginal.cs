
namespace ScrapperGateway.Models.Vinted
{
    public class Exposure
    {
        public string test_id { get; set; }
        public string test_name { get; set; }
        public string variant { get; set; }
        public string test_anon_id { get; set; }
        public string test_user_id { get; set; }
        public string country_code { get; set; }
    }

    public class Extra
    {
    }

    public class HighResolution
    {
        public string id { get; set; }
        public int timestamp { get; set; }
        public object orientation { get; set; }
    }

    public class ItemBox
    {
        public string first_line { get; set; }
        public string second_line { get; set; }
        public Exposure exposure { get; set; }
    }

    public class Photo
    {
        public int id { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public object temp_uuid { get; set; }
        public string url { get; set; }
        public string dominant_color { get; set; }
        public string dominant_color_opaque { get; set; }
        public List<Thumbnail> thumbnails { get; set; }
        public bool is_suspicious { get; set; }
        public object orientation { get; set; }
        public HighResolution high_resolution { get; set; }
        public string full_size_url { get; set; }
        public bool is_hidden { get; set; }
        public Extra extra { get; set; }
        public int image_no { get; set; }
        public bool is_main { get; set; }
    }

    public class Root
    {
        public long id { get; set; }
        public string title { get; set; }
        public string price { get; set; }
        public bool is_visible { get; set; }
        public object discount { get; set; }
        public string currency { get; set; }
        public string brand_title { get; set; }
        public User user { get; set; }
        public object conversion { get; set; }
        public string url { get; set; }
        public bool promoted { get; set; }
        public Photo photo { get; set; }
        public int favourite_count { get; set; }
        public bool is_favourite { get; set; }
        public object badge { get; set; }
        public string service_fee { get; set; }
        public string total_item_price { get; set; }
        public int view_count { get; set; }
        public string size_title { get; set; }
        public string content_source { get; set; }
        public string status { get; set; }
        public List<object> icon_badges { get; set; }
        public ItemBox item_box { get; set; }
        public SearchTrackingParams search_tracking_params { get; set; }
    }

    public class SearchTrackingParams
    {
        public double score { get; set; }
        public object matched_queries { get; set; }
    }

    public class Thumbnail
    {
        public string type { get; set; }
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public object original_size { get; set; }
    }

    public class User
    {
        public int id { get; set; }
        public string login { get; set; }
        public string profile_url { get; set; }
        public Photo photo { get; set; }
        public bool business { get; set; }
    }
}