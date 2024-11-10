using AutoMapper;
using DataLayer.Models.PostGresModels;
using DataLayer.Models;

public class AdMappingProfile : Profile
{
    public AdMappingProfile()
    {
        CreateMap<AdModel, Ad>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.description))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.title))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.url))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => (decimal?)src.price))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.images != null ? src.images.ToArray() : Array.Empty<string>()))
            .ForMember(dest => dest.AdScore, opt => opt.MapFrom(src => (int?)src.Adscore))
            .ForMember(dest => dest.FinalScore, opt => opt.MapFrom(src => (int?)src.finalScore))
            .ForMember(dest => dest.GoodThings, opt => opt.MapFrom(src => src.goodThings != null ? src.goodThings.ToArray() : Array.Empty<string>()))
            .ForMember(dest => dest.BadThings, opt => opt.MapFrom(src => src.badThings != null ? src.badThings.ToArray() : Array.Empty<string>()))
             .ForMember(dest => dest.PublishDate, opt => opt.MapFrom(src =>
        src.publishDate != DateTime.MinValue ?
            new DateTimeOffset(DateTime.SpecifyKind(src.publishDate, DateTimeKind.Utc)) :
            (DateTimeOffset?)null))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.category))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => (int?)src.categoryId))
            .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.province))
            .ForMember(dest => dest.ProvinceId, opt => opt.MapFrom(src => (int?)src.provinceId))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.city))
            .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => (int?)src.cityId))
            .ForMember(dest => dest.Highlighted, opt => opt.MapFrom(src => src.highlighted))
            .ForMember(dest => dest.IsNew, opt => opt.MapFrom(src => src.isNew))
            .ForMember(dest => dest.IsReserved, opt => opt.MapFrom(src => ParseReservedStatus(src.isReserved)))
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.slug))
            .ForMember(dest => dest.SellerType, opt => opt.MapFrom(src => src.sellerType))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.tags != null ? src.tags.ToArray() : Array.Empty<string>()))
    
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src =>
        src.updateDate != DateTime.MinValue ?
            new DateTimeOffset(DateTime.SpecifyKind(src.updateDate, DateTimeKind.Utc)) :
            (DateTimeOffset?)null))
                .ForMember(dest => dest.ScrappedDate, opt => opt.MapFrom(src =>
        src.ScrappedDate != DateTime.MinValue ?
            new DateTimeOffset(DateTime.SpecifyKind(src.ScrappedDate, DateTimeKind.Utc)) :
            (DateTimeOffset?)null))
            .ForMember(dest => dest.Likes, opt => opt.Ignore());

        CreateMap<Ad, AdModel>()
            .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.url, opt => opt.MapFrom(src => src.Url))
            .ForMember(dest => dest.price, opt => opt.MapFrom(src => src.Price.HasValue ? (double)src.Price.Value : 0))
            .ForMember(dest => dest.images, opt => opt.MapFrom(src => src.Images != null ? src.Images.ToList() : new List<string>()))
            .ForMember(dest => dest.Adscore, opt => opt.MapFrom(src => src.AdScore ?? 0))
            .ForMember(dest => dest.finalScore, opt => opt.MapFrom(src => src.FinalScore ?? 0))
            .ForMember(dest => dest.goodThings, opt => opt.MapFrom(src => src.GoodThings != null ? src.GoodThings.ToList() : new List<string>()))
            .ForMember(dest => dest.badThings, opt => opt.MapFrom(src => src.BadThings != null ? src.BadThings.ToList() : new List<string>()))
            .ForMember(dest => dest.publishDate, opt => opt.MapFrom(src => src.PublishDate.HasValue ? src.PublishDate.Value.DateTime : DateTime.MinValue))
            .ForMember(dest => dest.category, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.categoryId, opt => opt.MapFrom(src => src.CategoryId ?? 0))
            .ForMember(dest => dest.province, opt => opt.MapFrom(src => src.Province))
            .ForMember(dest => dest.provinceId, opt => opt.MapFrom(src => src.ProvinceId ?? 0))
            .ForMember(dest => dest.city, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.cityId, opt => opt.MapFrom(src => src.CityId ?? 0))
            .ForMember(dest => dest.highlighted, opt => opt.MapFrom(src => src.Highlighted))
            .ForMember(dest => dest.isNew, opt => opt.MapFrom(src => src.IsNew))
            .ForMember(dest => dest.isReserved, opt => opt.MapFrom(src => src.IsReserved.ToString()))
            .ForMember(dest => dest.slug, opt => opt.MapFrom(src => src.Slug))
            .ForMember(dest => dest.sellerType, opt => opt.MapFrom(src => src.SellerType))
            .ForMember(dest => dest.tags, opt => opt.MapFrom(src => src.Tags != null ? src.Tags.ToList() : new List<string>()))
            .ForMember(dest => dest.updateDate, opt => opt.MapFrom(src => src.UpdateDate.HasValue ? src.UpdateDate.Value.DateTime : DateTime.MinValue))
            .ForMember(dest => dest.ScrappedDate, opt => opt.MapFrom(src => src.ScrappedDate.HasValue ? src.ScrappedDate.Value.DateTime : DateTime.MinValue));
    }

    private static bool ParseReservedStatus(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;
        value = value.ToLower();
        return value == "true" || value == "1" || value == "yes" || value == "reserved";
    }
}