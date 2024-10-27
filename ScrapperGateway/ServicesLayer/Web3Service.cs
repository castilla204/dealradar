using AutoMapper;
using DataLayer;

namespace ServicesLayer
{
    public class Web3Service : IWeb3Service
    {
        private readonly IWeb3Data _web3Data;
        private readonly IMapper _mapper;

        public Web3Service(IMapper mapper)
        {
            _mapper = mapper;
            _web3Data = new Web3Data(_mapper);
        }

        public async Task<string> GetWallapop(string keywords, int pagestoscrap, int? category, string? latitude, string? longitude, int? minprice, int? maxprice)
        {
            return await _web3Data.MakeRequestAsync(keywords, pagestoscrap, category, latitude, longitude, minprice, maxprice);
        }
    }
}
