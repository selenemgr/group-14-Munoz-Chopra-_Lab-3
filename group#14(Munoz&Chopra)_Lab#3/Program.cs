using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using group_14_Munoz_Chopra__Lab_3.Data;
using Microsoft.EntityFrameworkCore;

namespace group_14_Munoz_Chopra__Lab_3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add SQL Server DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddSession();
            builder.Services.AddHttpContextAccessor();

            // Add AWS S3 service
            var awsSection = builder.Configuration.GetSection("AWS");
            var awsOptions = new AWSOptions
            {
                Credentials = new BasicAWSCredentials(
                    awsSection["AccessKey"],
                    awsSection["SecretKey"]
                ),
                Region = RegionEndpoint.GetBySystemName(awsSection["Region"])
            };

            builder.Services.AddDefaultAWSOptions(awsOptions);
            builder.Services.AddAWSService<IAmazonS3>();

            // Add AWS DynamoDB service
            builder.Services.AddAWSService<IAmazonDynamoDB>();
            builder.Services.AddSingleton<IDynamoDBContext>(sp =>
            {
                var client = sp.GetRequiredService<IAmazonDynamoDB>();
                return new DynamoDBContext(client);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession(); 
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
