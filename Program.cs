using BackSide.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using System.Configuration;

namespace BackSide
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // MLS 9/14/23 Access Token Validation is done for the developer when this is called.
            // See https://learn.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-app-configuration?tabs=aspnetcore
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
 
            // MLS 9/13/23 This method was discussed in a README file on github, but not used
            // in corresponding code sample
            // https://github.com/Azure-Samples/ms-identity-javascript-angular-tutorial/blob/main/3-Authorization-II/1-call-api/README.md#about-the-code
            // builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);


            builder.Services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.All;
                logging.RequestHeaders.Add("sec-ch-ua");
                logging.ResponseHeaders.Add("MyResponseHeader");
                logging.MediaTypeOptions.AddText("application/javascript");
                logging.RequestBodyLogLimit = 4096;
                logging.ResponseBodyLogLimit = 4096;

            });

            builder.Services.AddControllers();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                                            options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

            // MLS 6/8/23 forgot to add this.
            // MLS 5/17/23 forgot to add this.
            // Allowing CORS for all domains and HTTP methods for the purpose of the sample
            // In production, modify this with the actual domains and HTTP methods you want to allow
            builder.Services.AddCors(o => o.AddPolicy("default", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // MLS 6/8/23 forgot to add this.
            // MLS 5/17/23 - added at end of day. was missing this all day!
            app.UseCors("default");

            // MLS 9/14/23 Add logging so we can see the JWT Bearer token in requests from
            // Angular client
            app.UseHttpLogging();

            app.UseHttpsRedirection();


            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}