
using FantasyChas_Backend.Data;
using FantasyChas_Backend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;

namespace FantasyChas_Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            DotNetEnv.Env.Load();

            //var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("AppDB"));
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000");
                    });
            });

            // Choose what we want to include in the Identity object in the database?
            builder.Services.AddIdentityApiEndpoints<IdentityUser>(options =>
            {

            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 1;
                options.Password.RequiredUniqueChars = 0;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
            builder.Services.AddScoped<IProfessionRepository, ProfessionRepository>();
            builder.Services.AddScoped<ISpeciesRepository, SpeciesRepository>();

            builder.Services.AddSingleton(sp => new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_KEY")));

            var app = builder.Build();

            app.MapIdentityApi<IdentityUser>();
            

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }



            // REMOVE this endpoint when ready
            app.MapGet("/user/character", () =>
            {

                return Results.Ok("Hello!");
            }).RequireAuthorization();

            app.MapGet("/hello", () =>
            {
                return Results.Ok("Hello world!");
            });



            app.UseHttpsRedirection();

            app.UseCors("AllowSpecificOrigins");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
