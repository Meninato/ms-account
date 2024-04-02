using Account.Api.V1.Models.Requests;
using Account.Api.V1.Models.Responses;
using Account.Data.Entities;
using Account.Jwt;
using AutoMapper;

namespace Account.Api.Helpers;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<UserAccount, AuthenticateResponse>()
            .ForMember(
                dest => dest.Roles,
                opt => opt.MapFrom(source => source.Roles.Select(r => r.Name)
                    .ToList()));

        CreateMap<UserAccount, AccountResponse>()
            .ForMember(
                dest => dest.Roles,
                opt => opt.MapFrom(source => source.Roles.Select(r => r.Name)
                    .ToList()));

        CreateMap<RegisterAccountRequest, UserAccount>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore());

        CreateMap<JwtRefreshToken, RefreshToken>();
    }
}
