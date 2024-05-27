using AutoMapper;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;

namespace ContactList.API.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Contact, ContactDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.SubcategoryName, opt => opt.MapFrom(src => src.Subcategory != null ? src.Subcategory.Name : src.CustomSubcategory));

            CreateMap<CreateContactRequestDto, Contact>()
                .ForMember(dest => dest.Category, opt => opt.Ignore()) // Ignorujemy Category, bo ustawiamy je w ContactService
                .ForMember(dest => dest.Subcategory, opt => opt.Ignore()); // Ignorujemy Subcategory, bo ustawiamy je w ContactService

            CreateMap<UpdateContactRequestDto, Contact>()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Subcategory, opt => opt.Ignore());

            CreateMap<User, UserDto>();
            CreateMap<RegisterRequestDto, User>();
            CreateMap<Category, CategoryDto>();
            CreateMap<Subcategory, SubcategoryDto>();
            CreateMap<Role, RoleDto>(); // Dodajemy mapowanie dla roli
            CreateMap<LoginRequestDto, User>();
        }
    }
}
