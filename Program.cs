#define Use_Azure_Blob_Storage

using BackSide.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using System.Configuration;

// to use DefaultAzureCredential -- passwordless access to DB 
using Microsoft.Data.SqlClient;
using Azure.Identity;

// to use Azure Blob Storage
using Azure.Storage.Blobs;
using BackSide.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Azure.Core;
using System;

// MLS 10/3/23 Need this so I can get to appsettings which I had to define in a separate step in App Service -> Configuration 
// All the variables I put in appsettings.json, go an entry in the AppService -> Configuration
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BackSide
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplication app;

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            // Add services to the container.

            // MLS 11/16/23 Azure reads appsettings.json, so can read section "AzureAd"
            // MLS 9/14/23 Access Token Validation is done for the developer by Microsoft validation code when this is called.
            // See https://learn.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-app-configuration?tabs=aspnetcore
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

            // MLS 9/13/23 This method was discussed in a README file on github, but not used
            // in corresponding code sample
            // https://github.com/Azure-Samples/ms-identity-javascript-angular-tutorial/blob/main/3-Authorization-II/1-call-api/README.md#about-the-code
            // builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

            // MLS 9/14/23 Added to see JWT Bearer Tokens(Access Tokens)
            // Comment out because it slows down my site
            // Logs the output to a console window?
            builder.Services.AddHttpLogging(logging =>
            {
                logging.LoggingFields = HttpLoggingFields.All;
                logging.RequestHeaders.Add("sec-ch-ua");
                logging.ResponseHeaders.Add("MyResponseHeader");
                logging.MediaTypeOptions.AddText("application/javascript");
                logging.RequestBodyLogLimit = 4096;
                logging.ResponseBodyLogLimit = 4096;

            });


            // 9/24/23 set up database context
            // Add database context and cache
            string azure_connection_string = String.Empty;
            string error_database = String.Empty;
            try
            {   
                azure_connection_string = builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")!;
                //azure_connection_string = builder.Configuration.GetConnectionString("Default")!;
                // Uncomment one of the two lines depending on the identity type    
                SqlConnection authenticatedConnection = new SqlConnection(azure_connection_string); // system-assigned identity
                //SqlConnection authenticatedConnection = 
                // new SqlConnection("Server=tcp:<server-name>.database.windows.net;Database=<database-name>;
                // Authentication=Active Directory Default;User Id=<client-id-of-user-assigned-identity>;
                // TrustServerCertificate=True"); // user-assigned identity

                // get the database context
                // MLS 11/7/23 The database context lives for the duration of an HTTP Request.
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(authenticatedConnection.ConnectionString));
            }
            catch (Exception e)
            {
                error_database = e.Message;
            }

#if Use_Azure_Blob_Storage
            String error_blobStorage = String.Empty;
            try
            {
                TokenCredential credential = new DefaultAzureCredential();

                Uri AzureBlobStorageAccountUri = new Uri(builder.Configuration.GetValue<String>("AzureBlobStorageAccount"));
                builder.Services.AddSingleton<BlobServiceClient>(x => new BlobServiceClient(AzureBlobStorageAccountUri, credential));
                // MLS 9/27/23
                builder.Services.AddSingleton<BlobStorageService>();
            }
            catch (Exception e)
            {
                error_blobStorage = e.Message;
            }

#endif
            try
            {
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

                app = builder.Build();

                // MLS 10/3/23
                // Logs created with the default logging providers are displayed: 
                // In Visual Studio:  In the Debug output window when debugging. In the ASP.NET Core Web Server window.
                // With dotnet run:   In the console window 

                app.Logger.LogInformation("Starting the app\n");
                app.Logger.LogInformation($"Connection String: {azure_connection_string}\n\n\n");
                if (!String.IsNullOrEmpty(error_database)) app.Logger.LogWarning($"Error: {error_database}\n\n\n");
#if Use_Azure_Blob_Storage
                if (!String.IsNullOrEmpty(error_blobStorage)) app.Logger.LogWarning($"Error: {error_blobStorage}\n\n\n");
#endif

                // MLS 9/29/23 Azure API Management needs the Swagger definitions to always be present,
                // regardless of the application's environment
                app.UseSwagger();

                // Configure the HTTP request pipeline.
                // MLS 10/16/23 Temporarily Allow application to show Swagger UI in production
                // if (app.Environment.IsDevelopment())
                {
                    app.UseSwaggerUI();
                }

                // MLS 6/8/23 forgot to add this.
                // MLS 5/17/23 - added at end of day. was missing this all day!
                app.UseCors("default");

                // MLS 9/14/23 Add logging so we can see the JWT Bearer token in requests from
                // Angular client
                // app.UseHttpLogging();

                app.UseHttpsRedirection();

                // MLS 11/16/23 Re-instated because Azure can read AzureAd section of appsettings.json file
                // MLS 11/15/23 Remove authentication and authorization in Azure until I can learn more at a later time
                // These setting work on localhost, but not in Azure
                app.UseAuthentication();
                app.UseAuthorization();


                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"If you're seeing this, the application threw this exception:\n{ex.Message}");

            }
        }
    }
}