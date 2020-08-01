using Api.Extensions;
using Api.Extensions.Swagger;
using Domain.UnitOfWork;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using System.Threading.Tasks;

namespace Api
{
#pragma warning disable CS1591 // O coment�rio XML ausente n�o foi encontrado para o tipo ou membro vis�vel publicamente
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<IUnitOfWork>())

                    /* JSONPatch ainda usa o newtonsoft e n�o sabemos quando vai deixar de usar, vide:
                     * https://github.com/dotnet/aspnetcore/issues/16968
                     * mas assim que poss�vel, remover essa chamada
                    */
                    .AddNewtonsoftJson(opts => opts.SerializerSettings.Converters.Add(new StringEnumConverter()))

                    .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddCustomLocalization();

            if (Environment.IsDevelopment())
            {
                services.AddDevelopmentLogging();
                services.AddDevelopmentServices();
            }

            if (Environment.IsStaging() || Environment.IsProduction())
            {
                services.AddStagingServices();
            }

            services.AddServices();

            services.AddHealthChecks();

            services.AddCustomVersioning();

            services.AddCustomSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
                              ILogger<Startup> logger, 
                              IApiVersionDescriptionProvider provider)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (Environment.IsProduction()  || Environment.IsStaging())
            {
                app.UseResponseCaching();
                app.UseExceptionHandler(options => options.Run(async context =>
                {
                    logger.LogError(context.Features.Get<IExceptionHandlerPathFeature>().Error.Message);
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await Task.CompletedTask;
                }));
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                // build a swagger endpoint for each discovered API version
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
                c.RoutePrefix = string.Empty;
            });

            app.UseRequestLocalization();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
#pragma warning restore CS1591 // O coment�rio XML ausente n�o foi encontrado para o tipo ou membro vis�vel publicamente
}
