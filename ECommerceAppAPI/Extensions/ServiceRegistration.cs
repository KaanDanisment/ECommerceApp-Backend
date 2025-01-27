using Business.Abstract;
using Business.Concrete;
using Core.DataAccess;
using Core.DataAccess.Abstract;
using Core.Entities.Concrete;
using Core.Utilities.Security.jwt;
using Core.Utilities.Security.RefreshToken;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using DataAccess.Concrete.EntityFramework.Context;
using DataAccess.Concrete.EntityFramework.UnitOfWork;
using DataAccess.Core.Concrete.EntityFramework;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TokenOptions = Core.Utilities.Security.jwt.TokenOptions;
using Core.Utilities.Security.Encryption;
using FluentValidation.AspNetCore;
using Entities.Validators;
using FluentValidation;


namespace ECommerceAppAPI.Extensions
{
    public static class ServiceRegistration
    {
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            //DbContext
            services.AddDbContext<ECommerceContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            //Identity
            services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<ECommerceContext>()
                .AddDefaultTokenProviders();

            //Repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICategoryRepository, EfCategoryRepository>();
            services.AddScoped<IOrderRepository, EfOrderRepository>();
            services.AddScoped<IProductRepository, EfProductRepository>();
            services.AddScoped<IImageRepository, EfImageRepository>();
            services.AddScoped<ISubcategoryRepository, EfSubcategoryRepository>();
            services.AddScoped<IOrderProductRepository, EfOrderProductRepository>();
            services.AddScoped<IAddressRepository, EfAddressRepository>();
            services.AddScoped(typeof(IGenericRepository<IdentityUserToken<string>>),
                provider =>
                    new EfGenericRepositoryBase<IdentityUserToken<string>, ECommerceContext>(
                        provider.GetService<ECommerceContext>()));


            //Services
            services.AddScoped<ICategoryService, CategoryManager>();
            services.AddScoped<IProductService, ProductManager>();
            services.AddScoped<IOrderService, OrderManager>();
            services.AddScoped<IAuthService, AuthManager>();
            services.AddScoped<ITokenHelper, JwtHelper>();
            services.AddScoped<IImageService, ImageManager>();
            services.AddScoped<ISubcategoryService, SubcategoryManager>();
            services.AddScoped<IAccountService, AccountManager>();
            services.AddScoped<RefreshTokenHelper, RefreshTokenHelper>();
            services.AddScoped<IAdminService, AdminManager>();
            services.AddScoped<IAwsS3Service, AwsS3Manager>();


            //Authentication
            var tokenOptions = configuration.GetSection("TokenOptions").Get<TokenOptions>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = tokenOptions.Issuer,
                    ValidateIssuer = true,
                    ValidAudience = tokenOptions.Audience,
                    ValidateAudience = true,
                    IssuerSigningKey = SecurityKeyHelper.CreateSecurityKey(tokenOptions.SecurityKey),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                };

                // Cookie içinden token okumak için event ayarı
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Cookie'den token'ı okumaya çalış
                        if (context.Request.Cookies.TryGetValue("AccessToken", out var token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        }
    }
}
