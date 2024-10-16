using AutoMapper;
using DataLayer;

namespace ServicesLayer
{
    public class Web1Service : IWeb1Service
    {
        private readonly IWeb1Data _web1Data;
        private readonly IMapper _mapper;
        public Web1Service(IMapper mapper) {

            _mapper = mapper;
            _web1Data = new Web1Data(_mapper);
        }



        public async Task<string> GetCarList(int brandId, int modelId)
        {
            return await _web1Data.MakeRequestAsync(brandId, modelId);
        }

    }
}
