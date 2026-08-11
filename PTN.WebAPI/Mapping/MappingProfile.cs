using System;
using AutoMapper;
using Microsoft.Extensions.Localization;
using PTN.WebAPI.Dtos;
using PTN.WebAPI.Entities;
using StatusCodes = PTN.WebAPI.Enums.StatusCodes;
using PTN.WebAPI.Extensions;

namespace PTN.WebAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RequestLogEntity, RequestCacheModel>();
            CreateMap<RequestCacheModel, RequestLogDto>();
            
            CreateMap<RequestLogEntity, RequestLogDto>()
                .ForMember(dest => dest.Message, opt => opt.MapFrom((src, dest, destMember, context) =>
                {
                    IStringLocalizer? localizer = null;
                    if (context.TryGetItems(out var items) && items.TryGetValue("Localizer", out var locObj))
                    {
                        localizer = locObj as IStringLocalizer;
                    }

                    int codeInt = src.StatusCode ?? 1;

                    StatusCodes codeEnum = codeInt switch
                    {
                        200 or 1 => StatusCodes.Success,
                        400 or 2 => StatusCodes.BadRequest,
                        401 or 3 => StatusCodes.Unauthorized,
                        404 or 4 => StatusCodes.NotFound,
                        _ => StatusCodes.InternalServerError
                    };

                    return codeEnum.GetDescription(localizer);
                }));

            CreateMap<RequestLogDto, RequestLogEntity>();
            CreateMap<RequestLogCreateDto, RequestLogEntity>();
            CreateMap<RequestLogUpdateDto, RequestLogEntity>();

            // Kullanıcı (User) Mappings
            CreateMap<UserEntity, UserDto>();
            CreateMap<UserCreateDto, UserEntity>();
            CreateMap<UserUpdateDto, UserEntity>();
        }
    }
}