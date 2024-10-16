
namespace ScrapperGateway.Models.CochesNet
{
    public class Location
    {
        public List<int> provinceIds { get; set; }
        public string mainProvince { get; set; }
        public int mainProvinceId { get; set; }
    }

    public class OfferType
    {
        public int id { get; set; }
        public string literal { get; set; }
    }

    public class Price
    {
        public int amount { get; set; }
        public int taxTypeId { get; set; }
        public bool hasTaxes { get; set; }
    }

    public class Resource
    {
        public string type { get; set; }
        public string url { get; set; }
    }

    public class Root
    {
        public string id { get; set; }
        public DateTime creationDate { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public Price price { get; set; }
        public Seller seller { get; set; }
        public int km { get; set; }
        public int year { get; set; }
        public List<int> provinceIds { get; set; }
        public string mainProvince { get; set; }
        public Location location { get; set; }
        public List<Resource> resources { get; set; }
        public int makeId { get; set; }
        public int modelId { get; set; }
        public int fuelTypeId { get; set; }
        public string fuelType { get; set; }
        public bool isFinanced { get; set; }
        public bool isCertified { get; set; }
        public bool isProfessional { get; set; }
        public DateTime publishedDate { get; set; }
        public bool hasUrge { get; set; }
        public OfferType offerType { get; set; }
        public string phone { get; set; }
        public string contractId { get; set; }
        public int transmissionTypeId { get; set; }
    }

    public class Seller
    {
        public string name { get; set; }
        public bool isProfessional { get; set; }
        public string contractId { get; set; }
    }

}
