using DataLayer;
using AutoMapper;

namespace ServicesLayer
{
    public class Web2Service : IWeb2Service
    {
        private readonly IWeb2Data _web2Data;
        private readonly IMapper _mapper;
        public Web2Service(IMapper mapper) {
            _mapper = mapper;
            _web2Data = new Web2Data(_mapper);
        }



        public async Task<string> GetVintedList(string searchKey)
        {
            return await _web2Data.MakeRequestAsync(searchKey);
        }

    }
}
