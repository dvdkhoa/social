using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialNetwork.BLL.Services;
using SocialNetwork.BLL.Services.Implements;
using SocialNetwork.DAL.Identity;
using SocialNetwork.DAL.Identity.Models;
using SocialNetwork.DAL.Repositories;
using SocialNetwork.DAL.Resources;

namespace SocialNetwork.Ioc
{
    public class DependencyInjection
    {
        private readonly IConfiguration Configuration;
        public DependencyInjection(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void InjectDependencies(IServiceCollection services)
        {
            // DbContext nay danh cho Identity
            services.AddDbContext<AppIdentityDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("SQLServer"),
                                        b =>
                                        {
                                            b.MigrationsAssembly("SocialNetwork.Api");
                                        });
            });


            services.AddIdentity<User,IdentityRole>()
                    .AddEntityFrameworkStores<AppIdentityDbContext>()
                    .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 3;
                options.Password.RequireNonAlphanumeric = false;
            });

            // Đăng ký resouces context(MongoDb)
            services.AddSingleton( s => new ResourceDbContext(
                                    Configuration.GetSection("MongoDb:ConnectionString").Value,
                                    Configuration.GetSection("MongoDb:DbName").Value));
            // Đăng ký các Repository
            services.AddScoped<UserRepository>();
            services.AddScoped<FeedRepository>();
            services.AddScoped<PostRepository>();

            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IPostService, PostService>();

        }
    }
}