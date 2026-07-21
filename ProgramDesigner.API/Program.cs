
using Hospital.Infrastructure;
using Hospital.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using ProgramDesigner.Application.Mappings;
using ProgramDesigner.Application.Serices.Abstractions;
using ProgramDesigner.Application.Services;
using ProgramDesigner.Domain.Contracts;
using ProgramDesigner.Infrastructure.DbContexts;
using Web.MiddelWares;

namespace ProgramDesigner.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            // Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

            });


            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ISerivceManager, ServiceManager>();
            builder.Services.AddAutoMapper(M => M.AddProfile(new MappingProfile()));
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            app.UseMiddleware<GlobalErrorHandlingMiddleware>();
            app.Run();
        }
    }
}
