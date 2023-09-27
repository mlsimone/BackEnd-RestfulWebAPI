using BackSide.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using System.Configuration;

// to use DefaultAzureCredential -- passwordless access to DB 
using Microsoft.Extensions.Azure;
using Azure.Identity;

// to use Azure Blob Storage
using Azure.Storage.Blobs;
using BackSide.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Azure.Core;

namespace BackSide
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // MLS 9/14/23 Access Token Validation is done for the developer by Microsoft validation code when this is called.
            // See https://learn.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-app-configuration?tabs=aspnetcore
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

            // MLS 9/13/23 This method was discussed in a README file on github, but not used
            // in corresponding code sample
            // https://github.com/Azure-Samples/ms-identity-javascript-angular-tutorial/blob/main/3-Authorization-II/1-call-api/README.md#about-the-code
            // builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

            // MLS 9/14/23 Added to see JWT Bearer Tokens (Access Tokens)
            // Comment out because it slows down my site
            //builder.Services.AddHttpLogging(logging =>
            //{
            //    logging.LoggingFields = HttpLoggingFields.All;
            //    logging.RequestHeaders.Add("sec-ch-ua");
            //    logging.ResponseHeaders.Add("MyResponseHeader");
            //    logging.MediaTypeOptions.AddText("application/javascript");
            //    logging.RequestBodyLogLimit = 4096;
            //    logging.ResponseBodyLogLimit = 4096;

            //});


            // 9/24/23 set up database context
            var connection = String.Empty;
            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddEnvironmentVariables().AddJsonFile("appsettings.json");
                // connection = builder.Configuration.GetConnectionString("Default");
                connection = builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");
            }
            else
            {
                connection = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTIONSTRING");
            }

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                                            options.UseSqlServer(connection));

#if (Use_Azure_Blob_Storage == true)

            TokenCredential credential = new DefaultAzureCredential();
            Uri AzureBlobStorageAccountUri = new Uri(builder.Configuration.GetValue<String>("AzureBlobStorageAccount"));

            builder.Services.AddSingleton<BlobServiceClient>(x => new BlobServiceClient(AzureBlobStorageAccountUri, credential));

            // MLS 9/27/23
            builder.Services.AddSingleton<BlobStorageService>();
#endif


            // MLS 9/26/23
            builder.Services.AddSingleton<FileStorageService>();

            builder.Services.AddControllers();

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
            // app.UseHttpLogging();

            app.UseHttpsRedirection();


            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}