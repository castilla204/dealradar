using AutoMapper;
using ScrapperGateway.Models.Vinted;
using DataLayer.Models.Wallapop;
using System;
using System.Collections.Generic;

namespace DataLayer
{
    public class VintedMappingProfile : Profile
    {
        public VintedMappingProfile()
        {
            CreateMap<ScrapperGateway.Models.Vinted.Root, DataLayer.Models.Wallapop.Root>()
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.id.ToString()))
                .ForMember(dest => dest.title, opt => opt.MapFrom(src => src.title))
                .ForMember(dest => dest.description, opt => opt.MapFrom(src => src.title))
                .ForMember(dest => dest.price, opt => opt.MapFrom(src => new DataLayer.Models.Wallapop.Price
                {
                    cashPrice = new DataLayer.Models.Wallapop.CashPrice
                    {
                        value = (int)(decimal.Parse(src.price) * 100),
                        includeTaxes = true
                    }
                }))
                .ForMember(dest => dest.categoryId, opt => opt.Ignore())
                .ForMember(dest => dest.category, opt => opt.Ignore())
                .ForMember(dest => dest.categoryTree, opt => opt.Ignore())
                .ForMember(dest => dest.images, opt => opt.MapFrom(src => new List<string> { src.photo != null ? src.photo.url : "" }))
                .ForMember(dest => dest.isNew, opt => opt.MapFrom(src => src.status == "new"))
                .ForMember(dest => dest.publishDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.updateDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.sortDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.url, opt => opt.MapFrom(src => src.url))
                .ForMember(dest => dest.userId, opt => opt.MapFrom(src => src.user.id))
                .ForMember(dest => dest.location, opt => opt.Ignore())
                .ForMember(dest => dest.origin, opt => opt.MapFrom(src => new DataLayer.Models.Wallapop.Origin
                {
                    name = "Vinted",
                    provider = "Vinted"
                }))
                .ForMember(dest => dest.tags, opt => opt.Ignore())
                .ForMember(dest => dest.searchLink, opt => opt.Ignore())
                .ForMember(dest => dest.sellerType, opt => opt.MapFrom(src => src.user.business ? "professional" : "particular"))
                .ForMember(dest => dest.sellType, opt => opt.MapFrom(src => "buy"))
                .ForMember(dest => dest.seoTitle, opt => opt.MapFrom(src => src.title))
                .ForMember(dest => dest.isReserved, opt => opt.MapFrom(src => "false"))
                .ForMember(dest => dest.highlighted, opt => opt.MapFrom(src => src.promoted))
                .ForMember(dest => dest.contactable, opt => opt.MapFrom(src => true));
        }
    }
}