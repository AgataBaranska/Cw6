
using Cw6.DAL;
using Cw6.Middleware;
using Cw6.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Cw6
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.

        //wstrzykiwanie klas przydatnych w wielu miejscach w kodzie(np. logowanie, komunikacja z bd)
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDbService, MssqlDBService>();
            services.AddTransient<IStudentsDbService, SqlServerDbService>();
            services.AddTransient<ILogService, FileLogService>();
            services.AddControllers();//zarejestrowanie kontrolerów z widokami i stronami
            //Dodawanie dokumentacji
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo { Title = "Students App API", Version = "v1" });

            });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //definiuje tzw. middlewary
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbService service)
        {
            app.UseDeveloperExceptionPage();

            //obs³uga b³êdów
            app.UseMiddleware<ExceptionMiddleware>();

            //Dodawanie dokumentacji krok 2 - dokumentacja publiczna
            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json","Students App API");
            }
            );


            //Middleware - Index: sxxxxx ->DB
            app.UseMiddleware<LoggingMiddleware>();
            app.UseWhen(context => context.Request.Path.ToString().Contains("secret"), app =>
             {
                 app.Use(async (context, next) =>
                 {
                     if (!context.Request.Headers.ContainsKey("Index"))
                     {
                         context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                         await context.Response.WriteAsync("Musisz podac numer indexu");
                         return; //short circut
                    }

                     string index = context.Request.Headers["Index"].ToString();
                    //exists in DB
                    var stud = service.GetStudent(index);
                     if (stud == null)
                     {
                         context.Response.StatusCode = StatusCodes.Status404NotFound;
                         await context.Response.WriteAsync("Podanego indexu nie ma w bazie");
                         return;
                     }
                     await next();
                 });
             });



            //dodaje now¹ instancjê klasy EndpointRoutingMiddleware(rutowanie zadañ u¿ytkowników na poststawie adresu url, metody http)
            app.UseRouting(); 





            //mój middleware: doklejanie do odpowiedzi  nag³owek http
            app.Use(async (context, c) =>
            {
                context.Response.Headers.Add("Secret", "1234");
                await c.Invoke();//przepuszczenie rz¹dania do kolejnego middleware w kolejce
            });


            app.UseMiddleware<CustomMiddleware>();


            //dodaje middleware, który sprawdza czy ktoœ ma dostêp do czegoœ
            app.UseAuthorization();


            //definiuje endpointy
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
