using AutoMapper;
using ScrapperGateway.Models.Wallapop;
using System;

namespace DataLayer.Mapping
{
    public class WallapopMappingProfile : Profile
    {
        public WallapopMappingProfile()
        {
            CreateMap<ScrapperGateway.Models.Wallapop.Root, DataLayer.Models.Wallapop.Root>()
                .ForMember(dest => dest.category, opt => opt.MapFrom(src => new DataLayer.Models.Wallapop.Category { id = src.category_id }))
                .ForMember(dest => dest.categoryId, opt => opt.MapFrom(src => src.category_id))
                .ForMember(dest => dest.categoryTree, opt => opt.Ignore())
                .ForMember(dest => dest.city, opt => opt.MapFrom(src => new DataLayer.Models.Wallapop.City { name = src.location.city }))
                .ForMember(dest => dest.contactable, opt => opt.Ignore())
                .ForMember(dest => dest.description, opt => opt.MapFrom(src => src.description))
                .ForMember(dest => dest.highlighted, opt => opt.MapFrom(src => src.visibility_flags.highlighted))
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.images, opt => opt.MapFrom(src => src.images.Select(img => img.original).ToList()))
                .ForMember(dest => dest.isNew, opt => opt.Ignore())
                .ForMember(dest => dest.isReserved, opt => opt.MapFrom(src => src.flags.reserved ? "true" : "false"))
                .ForMember(dest => dest.location, opt => opt.MapFrom(src => new DataLayer.Models.Wallapop.Location
                {
                    city = new DataLayer.Models.Wallapop.City { name = src.location.city },
                    province = new DataLayer.Models.Wallapop.Province { name = src.location.postal_code },
                    region = new DataLayer.Models.Wallapop.Region { name = src.location.country_code }
                }))
                .ForMember(dest => dest.origin, opt => opt.Ignore())
                .ForMember(dest => dest.price, opt => opt.MapFrom(src => new DataLayer.Models.Wallapop.Price
                {
                    cashPrice = new DataLayer.Models.Wallapop.CashPrice { value = (int)src.price, includeTaxes = true }
                }))
                .ForMember(dest => dest.province, opt => opt.Ignore())
                .ForMember(dest => dest.publishDate, opt => opt.MapFrom(src => src.creation_date))
                .ForMember(dest => dest.searchLink, opt => opt.Ignore())
                .ForMember(dest => dest.sellerType, opt => opt.Ignore())
                .ForMember(dest => dest.sellType, opt => opt.Ignore())
                .ForMember(dest => dest.seoTitle, opt => opt.Ignore())
                .ForMember(dest => dest.tags, opt => opt.Ignore())
                .ForMember(dest => dest.title, opt => opt.MapFrom(src => src.title))
                .ForMember(dest => dest.url, opt => opt.MapFrom(src => src.web_slug))
                .ForMember(dest => dest.userId, opt => opt.MapFrom(src => ParseSellerId(src.seller_id)))
                .ForMember(dest => dest.sortDate, opt => opt.MapFrom(src => src.modification_date))
                .ForMember(dest => dest.updateDate, opt => opt.MapFrom(src => src.modification_date));

            // Add any additional mappings for nested objects if needed
            CreateMap<ScrapperGateway.Models.Wallapop.Location, DataLayer.Models.Wallapop.Location>();
            CreateMap<ScrapperGateway.Models.Wallapop.Image, string>()
                .ConvertUsing(src => src.original);
        }

        private static int ParseSellerId(string sellerId)
        {
            return int.TryParse(sellerId, out int result) ? result : 0;
        }
    }
}