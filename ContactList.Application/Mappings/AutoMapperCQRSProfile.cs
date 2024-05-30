using AutoMapper;
using ContactList.Application.Commands;
using ContactList.Application.Commands.Contact;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Application.Mappings
{
    public class AutoMapperCQRSProfile : Profile
    {
        public AutoMapperCQRSProfile()
        {
            CreateMap<Contact, ContactDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.SubcategoryName, opt => opt.MapFrom(src => src.Subcategory != null ? src.Subcategory.Name : src.CustomSubcategory));

            CreateMap<CreateContactRequestDto, Contact>()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Subcategory, opt => opt.Ignore());

            CreateMap<UpdateContactRequestDto, Contact>()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Subcategory, opt => opt.Ignore());

            CreateMap<User, UserDto>();
            CreateMap<RegisterRequestDto, User>();
            CreateMap<Category, CategoryDto>();
            CreateMap<Subcategory, SubcategoryDto>();
            CreateMap<Role, RoleDto>();
            CreateMap<RegisterRequestDto, RegisterUserCommand>();
            CreateMap<RegisterUserCommand, User>();

            CreateMap<RegisterRequestDto, RegisterUserCommand>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src));

            CreateMap<RegisterUserCommand, User>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
             .ForMember(dest => dest.UserId, opt => opt.Ignore());// Zakładamy, że hashowanie hasła nastąpi później

            CreateMap<LoginRequestDto, LoginUserCommand>();

            CreateMap<CreateContactRequestDto, CreateContactCommand>()
            .ForPath(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForPath(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForPath(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForPath(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForPath(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForPath(dest => dest.CustomSubcategory, opt => opt.MapFrom(src => src.CustomSubcategory))
            .ForPath(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            .ForPath(dest => dest.SubcategoryId, opt => opt.MapFrom(src => src.SubcategoryId));

            CreateMap<CreateContactCommand, Contact>();

            CreateMap<UpdateContactRequestDto, UpdateContactCommand>();
            CreateMap<UpdateContactCommand, Contact>();
        }
    }
}
