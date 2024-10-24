namespace ClientScrapperMilanuncios.Models
{
    public class CashPrice
    {
        public int value { get; set; }
        public bool includeTaxes { get; set; }
    }

    public class Category
    {
        public int id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public class CategoryTree
    {
        public int id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public class City
    {
        public int id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public class Location
    {
        public City city { get; set; }
        public Province province { get; set; }
        public Region region { get; set; }
    }

    public class Origin
    {
        public string name { get; set; }
        public string provider { get; set; }
    }

    public class Price
    {
        public CashPrice cashPrice { get; set; }
    }

    public class Province
    {
        public int id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public class Region
    {
        public int id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public class Root
    {
        public Category category { get; set; }
        public int categoryId { get; set; }
        public List<CategoryTree> categoryTree { get; set; }
        public City city { get; set; }
        public bool contactable { get; set; }
        public string description { get; set; }
        public bool highlighted { get; set; }
        public string id { get; set; }
        public List<string> images { get; set; }
        public bool isNew { get; set; }
        public string isReserved { get; set; }
        public Location location { get; set; }
        public Origin origin { get; set; }
        public Price price { get; set; }
        public Province province { get; set; }
        public DateTime publishDate { get; set; }
        public SearchLink searchLink { get; set; }
        public string sellerType { get; set; }
        public string sellType { get; set; }
        public string seoTitle { get; set; }
        public List<Tag> tags { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public int userId { get; set; }
        public DateTime sortDate { get; set; }
        public DateTime updateDate { get; set; }
        public int Adscore { get; set; }
        public int finalScore { get; set; }
        public string goodThings { get; set; }
        public string badThings { get; set; }
        public DateTime ScrappedDate { get; set;  }
    }

    public class SearchLink
    {
        public string label { get; set; }
        public string url { get; set; }
    }

    public class Tag
    {
        public string type { get; set; }
        public string text { get; set; }
    }

}
