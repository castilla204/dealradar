using AutoMapper;
using DataLayer.Models.PostGresModels;
using DataLayer.Models;

public class AdMappingProfile : Profile
{
    public AdMappingProfile()
    {
        CreateMap<AdModel, Ad>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.description))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.title))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.url))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.price))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.images.ToArray()))  // Mapping List to Array
            .ForMember(dest => dest.AdScore, opt => opt.MapFrom(src => src.Adscore))
            .ForMember(dest => dest.FinalScore, opt => opt.MapFrom(src => src.finalScore))
            .ForMember(dest => dest.GoodThings, opt => opt.MapFrom(src => src.goodThings.ToArray()))  // List to Array
            .ForMember(dest => dest.BadThings, opt => opt.MapFrom(src => src.badThings.ToArray()))  // List to Array
            .ForMember(dest => dest.PublishDate, opt => opt.MapFrom(src => src.publishDate))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.category))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.categoryId))
            .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.province))
            .ForMember(dest => dest.ProvinceId, opt => opt.MapFrom(src => src.provinceId))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.city))
            .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.cityId))
            .ForMember(dest => dest.Highlighted, opt => opt.MapFrom(src => src.highlighted))
            .ForMember(dest => dest.IsNew, opt => opt.MapFrom(src => src.isNew))
            .ForMember(dest => dest.IsReserved, opt => opt.MapFrom(src => bool.Parse(src.isReserved)))  // String to Boolean
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.slug))
            .ForMember(dest => dest.SellerType, opt => opt.MapFrom(src => src.sellerType))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.tags.ToArray()))  // List to Array
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.userId))
            .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.updateDate))
            .ForMember(dest => dest.ScrappedDate, opt => opt.MapFrom(src => src.ScrappedDate));
    }
}
