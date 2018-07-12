using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using aspnetcoregraphql.Data.Repositories;
using aspnetcoregraphql.Models.Operations;
using aspnetcoregraphql.Models.Types;
using aspnetcoregraphql.Models.Schemas;
using GraphQL;
using GraphQL.Types;
using GraphQL.Authorization;
using GraphQL.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using aspnetcoregraphql.Auth;
using System.Security.Claims;

namespace aspnetcoregraphql
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
            // allow calls from different domains
            services.AddCors();
            services.AddMvc();


            services.AddSingleton<EasyStoreQuery>();   
            services.AddSingleton<EasyStoreMutation>();   

            // This should be AddScoped/AddTransient, but for matter of testing
            // I used AddSingleton to keep Repositories with their last statuses
            // which is important for mutations (to act like in memory db)
            // - //////////////////////////////////////////////////////////
            services.AddSingleton<ICategoryRepository, CategoryRepository>();
            services.AddSingleton<IProductRepository, ProductRepository>(); 
            services.AddSingleton<ICustomerRepository, CustomerRepository>();
            services.AddSingleton<IOrderRepository, OrderRepository>();    

            services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
            services.AddSingleton<CategoryType>();
            services.AddSingleton<ProductType>();
            services.AddSingleton<CustomerType>();
            services.AddSingleton<OrderType>();            
            services.AddSingleton<OrderStatusesEnum>();          
            services.AddSingleton<OrderCreateInputType>();
            // - //////////////////////////////////////////////////////////

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAuthorizationEvaluator, AuthorizationEvaluator>();
            // services.AddTransient<IValidationRule, AuthorizationValidationRule>();
            services.AddTransient<IValidationRule>(s => new AuthorizationValidationRule(s.GetRequiredService<IAuthorizationEvaluator>()));

            services.AddSingleton(s =>
            {
                var authSettings = new AuthorizationSettings();

                authSettings.AddPolicy("AdminPolicy", _ => _.RequireClaim(ClaimTypes.Role, new string[] { "Admin" }));
                authSettings.AddPolicy("ManagerPolicy", _ => _.RequireClaim(ClaimTypes.Role, new string[] { "Manager", "Admin" }));
                authSettings.AddPolicy("UserPolicy", _ => _.RequireClaim(ClaimTypes.Role, new string[] { "User", "Manager", "Admin" }));

                return authSettings;
            });

            var sp = services.BuildServiceProvider();
            // services.AddScoped<ISchema>(_ => new EasyStoreSchema(type => (GraphType) sp.GetService(type)) {Query = sp.GetService<EasyStoreQuery>()});
            // services.AddScoped<ISchema>(_ => new EasyStoreSchema(new FuncDependencyResolver(type => (GraphType) sp.GetService(type))) {Query = sp.GetService<EasyStoreQuery>()});
            services.AddSingleton<ISchema>(new EasyStoreSchema(new FuncDependencyResolver(type => sp.GetService(type))));
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(builder =>
                builder.WithOrigins(new string[] {"http://localhost:5000", "http://localhost:8080"})
                    .AllowAnyHeader()
                    .AllowAnyMethod()
            );
            app.UseMvc();
            app.UseDefaultFiles();
            app.UseStaticFiles();          
        }      
    }
}
