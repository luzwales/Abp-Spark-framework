using AutoMapper;
using Abp.Spark.Users;

namespace Abp.Spark;

public class AbpSparkApplicationAutoMapperProfile : Profile
{
    public AbpSparkApplicationAutoMapperProfile()
    {
        CreateMap<AppUser, UserDto>();
    }
}
