using AutoMapper;
using DataLayer;

namespace ServicesLayer
{
    public class Web4Service : IWeb4Service
    {
        private readonly IWeb4Data _web4Data;
        private readonly IMapper _mapper;

        public Web4Service(IWeb4Data web4Data, IMapper mapper)
        {
            _web4Data = web4Data;
            _mapper = mapper;
        }

        public async Task<string> GetWallapop(string keywords)
        {
            return await _web4Data.MakeRequestAsync(keywords);
        }
    }
}
