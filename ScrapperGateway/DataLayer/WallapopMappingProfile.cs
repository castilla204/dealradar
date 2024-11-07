using AutoMapper;
using ScrapperGateway.Models.Wallapop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLayer.Mapping
{
    public class WallapopMappingProfile : Profile
    {
        public WallapopMappingProfile()
        {
            CreateMap<ScrapperGateway.Models.Wallapop.Root, DataLayer.Models.AdModel>()
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.description, opt => opt.MapFrom(src => src.description))
                .ForMember(dest => dest.title, opt => opt.MapFrom(src => src.title))
                .ForMember(dest => dest.url, opt => opt.MapFrom(src => src.web_slug))
                .ForMember(dest => dest.price, opt => opt.MapFrom(src => src.price))
                .ForMember(dest => dest.images, opt => opt.MapFrom(src => src.images.Select(img => img.original).ToList()))
                .ForMember(dest => dest.Adscore, opt => opt.MapFrom(src => 0)) // Ajusta esto según tu lógica para calcular Adscore
                .ForMember(dest => dest.finalScore, opt => opt.MapFrom(src => 0)) // Ajusta esto según tu lógica para calcular finalScore
                .ForMember(dest => dest.goodThings, opt => opt.MapFrom(src => new List<string>())) // Ajusta según necesites
                .ForMember(dest => dest.badThings, opt => opt.MapFrom(src => new List<string>())) // Ajusta según necesites
                .ForMember(dest => dest.publishDate, opt => opt.MapFrom(src => src.creation_date))
                .ForMember(dest => dest.categoryId, opt => opt.MapFrom(src => src.category_id))
                .ForMember(dest => dest.category, opt => opt.MapFrom(src => string.Empty)) // Ajusta si tienes un mapeo específico para la categoría
                .ForMember(dest => dest.province, opt => opt.MapFrom(src => src.location.postal_code))
                .ForMember(dest => dest.provinceId, opt => opt.MapFrom(src => 0)) // Ajusta según tu lógica para obtener el ID de la provincia
                .ForMember(dest => dest.city, opt => opt.MapFrom(src => src.location.city))
                .ForMember(dest => dest.cityId, opt => opt.MapFrom(src => 0)) // Ajusta según tu lógica para obtener el ID de la ciudad
                .ForMember(dest => dest.highlighted, opt => opt.MapFrom(src => src.visibility_flags.highlighted))
                .ForMember(dest => dest.isNew, opt => opt.MapFrom(src => src.is_refurbished))
                .ForMember(dest => dest.isReserved, opt => opt.MapFrom(src => src.flags.reserved ? "true" : "false"))
                .ForMember(dest => dest.slug, opt => opt.MapFrom(src => src.web_slug))
                .ForMember(dest => dest.sellerType, opt => opt.MapFrom(src => string.Empty)) // Ajusta según necesites
                .ForMember(dest => dest.tags, opt => opt.MapFrom(src => new List<string>())) // Ajusta según necesites
                .ForMember(dest => dest.userId, opt => opt.MapFrom(src => ParseSellerId(src.seller_id)))
                .ForMember(dest => dest.updateDate, opt => opt.MapFrom(src => src.modification_date))
                .ForMember(dest => dest.ScrappedDate, opt => opt.MapFrom(src => DateTime.Now)); // Ajusta según necesites
        }

        private static int ParseSellerId(string sellerId)
        {
            return int.TryParse(sellerId, out int result) ? result : 0;
        }
    }
}
