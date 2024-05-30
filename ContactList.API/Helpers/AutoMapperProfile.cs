using AutoMapper;
using ContactList.Application.Commands;
using ContactList.Application.Commands.Contact;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;

namespace ContactList.API.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Mapowanie z modelu Contact na ContactDto
            CreateMap<Contact, ContactDto>()
                // Mapowanie nazwy kategorii z encji zależnej Category
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                // Warunkowe mapowanie nazwy podkategorii
                .ForMember(dest => dest.SubcategoryName, opt => opt.MapFrom(src => src.Subcategory != null ? src.Subcategory.Name : src.CustomSubcategory));

            // Mapowanie z DTO do tworzenia kontaktu na encję Contact
            CreateMap<CreateContactRequestDto, Contact>()
                // Ignorujemy właściwość Category, ponieważ jest ona ustawiana w serwisie
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                // Ignorujemy właściwość Subcategory z tego samego powodu
                .ForMember(dest => dest.Subcategory, opt => opt.Ignore());

            // Mapowanie z DTO do aktualizacji kontaktu na encję Contact
            CreateMap<UpdateContactRequestDto, Contact>()
                // Podobnie jak powyżej, ignorujemy Category i Subcategory
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Subcategory, opt => opt.Ignore());

            // Proste mapowania między użytkownikami i ich DTOs
            CreateMap<User, UserDto>();
            CreateMap<RegisterRequestDto, User>();
            CreateMap<Category, CategoryDto>();
            CreateMap<Subcategory, SubcategoryDto>();

            // Mapowanie dla roli
            CreateMap<Role, RoleDto>();

            // Mapowanie dla logowania
            CreateMap<LoginRequestDto, User>();




            CreateMap<Contact, ContactDto>()
               .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
               .ForMember(dest => dest.SubcategoryName, opt => opt.MapFrom(src => src.Subcategory != null ? src.Subcategory.Name : src.CustomSubcategory));

            CreateMap<CreateContactRequestDto, Contact>()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Subcategory, opt => opt.Ignore());

            CreateMap<UpdateContactRequestDto, Contact>()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Subcategory, opt => opt.Ignore());

            //CreateMap<User, UserDto>();
            //CreateMap<RegisterRequestDto, User>();
            //CreateMap<Category, CategoryDto>();
            //CreateMap<Subcategory, SubcategoryDto>();
            //CreateMap<Role, RoleDto>();

           
            
            
            
            
            


        }
    }
}
