using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleApp.Data;
using SampleApp.Services;
using SampleApp.Services.OCR;
using SampleApp.Services.Search;

namespace SampleApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Database>(options => options.UseSqlServer(Configuration["ConnectionString"]));
            services.AddHttpContextAccessor();

            services.AddScoped<RecognizerConfig>((_) => new RecognizerConfig { 
                Endpoint = Configuration["AzureFormRecognizer:Endpoint"],
                Key = Configuration["AzureFormRecognizer:Key"],
                ModelId = Configuration["AzureFormRecognizer:OutgoingInvoicesModelId"],
                ModelUrl = Configuration["AzureFormRecognizer:OutgoingInvoicesModelUrl"]
            });
            services.AddScoped<Recognizer>();

            services.AddScoped<AccountancyServices>();
            services.AddScoped<UrlServices>();
            services.AddScoped<Sherlock>();
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
