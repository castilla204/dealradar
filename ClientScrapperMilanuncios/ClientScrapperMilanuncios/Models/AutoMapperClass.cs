

namespace ClientScrapperMilanuncios.Models
{
    using AutoMapper;
    using System;
    using System.Linq;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    namespace ClientScrapperMilanuncios.Models.ClientScrapperMilanuncios.Mapping
    {
        public class AutoMapperClass : Profile
        {
            public AutoMapperClass()
            {
                //// Mapeo directo de Root a GrupAd sin Schema
                //CreateMap<Root, GrupAd>()
                //    .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.id))
                //    .ForMember(dest => dest.source, opt => opt.MapFrom(src => "milanuncios"))
                //    .ForMember(dest => dest.title, opt => opt.MapFrom(src => src.title))
                //    .ForMember(dest => dest.description, opt => opt.MapFrom(src => src.description))
                //    .ForMember(dest => dest.GrupPrice, opt => opt.MapFrom(src => src.price.cashPrice))
                //    .ForMember(dest => dest.seller, opt => opt.MapFrom(src => src))
                //    .ForMember(dest => dest.GrupLocation, opt => opt.MapFrom(src => src.location))
                //    .ForMember(dest => dest.details, opt => opt.MapFrom(src => src))
                //    .ForMember(dest => dest.media, opt => opt.MapFrom(src => src))
                //    .ForMember(dest => dest.dates, opt => opt.MapFrom(src => src))
                //    .ForMember(dest => dest.url, opt => opt.MapFrom(src => src.url))
                //    .ForMember(dest => dest.positiveAspects, opt => opt.Ignore())
                //    .ForMember(dest => dest.negativeAspects, opt => opt.Ignore());

                //// Mapeo de CashPrice a GrupPrice
                //CreateMap<CashPrice, GrupPrice>()
                //    .ForMember(dest => dest.amount, opt => opt.MapFrom(src => src.value.ToString()))
                //    .ForMember(dest => dest.hasTaxes, opt => opt.MapFrom(src => src.includeTaxes.ToString().ToLower()));

                //// Mapeo de Root a Seller
                //CreateMap<Root, Seller>()
                //    .ForMember(dest => dest.name, opt => opt.Ignore())
                //    .ForMember(dest => dest.isProfessional, opt => opt.MapFrom(src => (src.sellerType == "pro").ToString().ToLower()))
                //    .ForMember(dest => dest.phone, opt => opt.Ignore())
                //    .ForMember(dest => dest.rating, opt => opt.Ignore());

                //// Mapeo de Location a GrupLocation
                //CreateMap<Location, GrupLocation>()
                //    .ForMember(dest => dest.province, opt => opt.MapFrom(src => src.province.name))
                //    .ForMember(dest => dest.city, opt => opt.MapFrom(src => src.city.name))
                //    .ForMember(dest => dest.postalCode, opt => opt.Ignore())
                //    .ForMember(dest => dest.coordinates, opt => opt.Ignore());

                //// Mapeo de Root a Details
                //CreateMap<Root, Details>()
                //    .ForMember(dest => dest.year, opt => opt.Ignore())
                //    .ForMember(dest => dest.kilometers, opt => opt.Ignore())
                //    .ForMember(dest => dest.cubicCapacity, opt => opt.Ignore())
                //    .ForMember(dest => dest.fuelType, opt => opt.Ignore());

                //// Mapeo de Root a Media
                //CreateMap<Root, Media>()
                //    .ForMember(dest => dest.images, opt => opt.MapFrom(src => src.images))
                //    .ForMember(dest => dest.videos, opt => opt.Ignore());

                //// Mapeo de Root a Dates
                //CreateMap<Root, Dates>()
                //    .ForMember(dest => dest.createdAt, opt => opt.MapFrom(src => src.publishDate.ToString("yyyy-MM-ddTHH:mm:ssZ")))
                //    .ForMember(dest => dest.publishedAt, opt => opt.MapFrom(src => src.publishDate.ToString("yyyy-MM-ddTHH:mm:ssZ")));
            }
        }
    }
}
