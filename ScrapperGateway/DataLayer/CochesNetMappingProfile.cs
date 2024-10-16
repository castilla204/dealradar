using AutoMapper;
using ScrapperGateway.Models.CochesNet;
using DataLayer.Models.Wallapop;
using System.Linq;

namespace DataLayer.Mappers
{
    public class CochesNetMappingProfile : Profile
    {
        public CochesNetMappingProfile()
        {
            CreateMap<ScrapperGateway.Models.CochesNet.Root, DataLayer.Models.Wallapop.Root>()
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.title, opt => opt.MapFrom(src => src.title))
                .ForMember(dest => dest.description, opt => opt.MapFrom(src => src.title)) // Usando title como descripción ya que CochesNet no tiene una descripción explícita
                .ForMember(dest => dest.price, opt => opt.MapFrom(src => new DataLayer.Models.Wallapop.Price
                {
                    cashPrice = new DataLayer.Models.Wallapop.CashPrice
                    {
                        value = src.price.amount,
                        includeTaxes = src.price.hasTaxes
                    }
                }))
                .ForMember(dest => dest.category, opt => opt.MapFrom(src => new DataLayer.Models.Wallapop.Category
                {
                    id = 100, // Valor por defecto para coches
                    name = "Coches",
                    slug = "coches"
                }))
                .ForMember(dest => dest.images, opt => opt.MapFrom(src =>
                    src.resources
                       .Where(r => r.type.ToLower() == "image")
                       .Select(r => r.url)
                       .ToList()))
                .ForMember(dest => dest.url, opt => opt.MapFrom(src => src.url))
                .ForMember(dest => dest.publishDate, opt => opt.MapFrom(src => src.publishedDate))
                .ForMember(dest => dest.location, opt => opt.MapFrom(src => new DataLayer.Models.Wallapop.Location
                {
                    city = new DataLayer.Models.Wallapop.City
                    {
                        id = src.location.mainProvinceId,
                        name = src.location.mainProvince,
                        slug = src.location.mainProvince.ToLower().Replace(" ", "-")
                    },
                    province = new DataLayer.Models.Wallapop.Province
                    {
                        id = src.location.mainProvinceId,
                        name = src.location.mainProvince,
                        slug = src.location.mainProvince.ToLower().Replace(" ", "-")
                    }
                }))
                .ForMember(dest => dest.sellerType, opt => opt.MapFrom(src => src.seller.isProfessional ? "professional" : "particular"))
                .ForMember(dest => dest.isNew, opt => opt.MapFrom(src => src.km == 0))
                .ForMember(dest => dest.tags, opt => opt.MapFrom(src => new List<DataLayer.Models.Wallapop.Tag>
                {
                    new DataLayer.Models.Wallapop.Tag { type = "km", text = src.km.ToString() },
                    new DataLayer.Models.Wallapop.Tag { type = "year", text = src.year.ToString() },
                    new DataLayer.Models.Wallapop.Tag { type = "fuel", text = src.fuelType }
                }))
                .ForMember(dest => dest.origin, opt => opt.MapFrom(src => new DataLayer.Models.Wallapop.Origin
                {
                    name = "CochesNet",
                    provider = "CochesNet"
                }));
        }
    }
}