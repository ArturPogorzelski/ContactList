using ContactList.API.Services;
using ContactList.Authentication.Services;
using ContactList.Core.Dtos;
using ContactList.Core.Entities;
using ContactList.Core.Interfaces;
using ContactList.Core.Validators;
using ContactList.Infrastructure.Data.Contexts;
using ContactList.Infrastructure.Helpers.Strategy;
using ContactList.Infrastructure.Helpers;
using ContactList.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ContactList.Application.Commands.Contact;
using ContactList.Application.Handlers;
using ContactList.Application.Queries.Contact;
using MediatR;
using ContactList.Application.Commands;
using ContactList.Application.Queries;
using ContactList.Application.Queries;
using ContactList.Application.Handlers;
using Microsoft.AspNetCore.Hosting;

namespace ContactList.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            // Rejestracja kontekstu bazy danych
            services.AddDbContext<ContactListDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Rejestracja repozytoriów
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ISubcategoryRepository, SubcategoryRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();

            services.AddScoped<IRetryHelper, RetryHelper>();
            services.AddScoped<IRetryStrategy, RetryHelperStaticStrategy>();
            services.AddScoped<IRetryStrategy, RetryHelperStrategy>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // Rejestracja serwisów
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ISubcategoryService, SubcategoryService>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            return services;
        }

        public static IServiceCollection AddValidation(this IServiceCollection services)
        {
            // Rejestracja walidatorów dla DTO
            services.AddScoped<IValidator<CreateContactRequestDto>, CreateContactRequestDtoValidator>();
            services.AddScoped<IValidator<CreateCategoryRequestDto>, CreateCategoryRequestDtoValidator>();
            services.AddScoped<IValidator<CreateSubcategoryRequestDto>, CreateSubcategoryRequestDtoValidator>();
            services.AddScoped<IValidator<LoginRequestDto>, LoginRequestDtoValidator>();
            services.AddScoped<IValidator<RegisterRequestDto>, RegisterRequestDtoValidator>();
            services.AddScoped<IValidator<UpdateContactRequestDto>, UpdateContactRequestDtoValidator>();
            services.AddScoped<IValidator<UpdateCategoryRequestDto>, UpdateCategoryRequestDtoValidator>();
            services.AddScoped<IValidator<UpdateSubcategoryRequestDto>, UpdateSubcategoryRequestDtoValidator>();
            


            return services;
        }
        public static IServiceCollection AddCQRSHandlers(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));


            
            services.AddScoped<IRequestHandler<CreateContactCommand, ContactDto>, CreateContactCommandHandler>();
            services.AddScoped<IRequestHandler<UpdateContactCommand, ContactDto>, UpdateContactCommandHandler>();
            services.AddScoped<IRequestHandler<DeleteContactCommand, Unit>, DeleteContactCommandHandler>();
            services.AddScoped<IRequestHandler<GetAllContactsForUserQuery, IEnumerable<ContactDto>>, GetAllContactsForUserQueryHandler>();
            services.AddScoped<IRequestHandler<GetContactByIdQuery, ContactDto>, GetContactByIdQueryHandler>();
            services.AddScoped<IRequestHandler<GetAllContactsQuery, IEnumerable<ContactDto>>, GetAllContactsQueryHandler>();

           
            services.AddScoped<IRequestHandler<RegisterUserCommand, UserDto>, RegisterUserCommandHandler>();

            services.AddScoped<IRequestHandler<LoginUserCommand, LoginRequestDto>, LoginUserCommandHandler>();

            services.AddScoped<IRequestHandler<GetUserByIdQuery, UserDto>, GetUserByIdQueryHandler>();
            services.AddScoped<IRequestHandler<GetAllUserQuery, IEnumerable<UserDto>>, GetAllUserQueryHandler>();
            return services;
        }
    }
}
