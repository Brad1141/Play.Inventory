using Microsoft.OpenApi.Models;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

public class Startup
{

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {

        services.AddMongo()
            // "<>" are used to specify type arguments
            .AddMongoRepository<InventoryItem>("inventoryitems")
            .AddMongoRepository<CatalogItem>("catalogitems")
            .AddMassTransitWithRabbitMq();

        AddCatalogClient(services);

        // services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        // services.AddSwaggerGen();


        // at runtime ASP.NET will remove the async suffix from our methods
        // this will cause our 'created at action' method (in POST) to throw an error
        // fix this by suppressing this action
        services.AddControllers(options =>
            options.SuppressAsyncSuffixInActionNames = false
        );
        // services.AddRazorPages();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Inventory.Service", Version = "v1" });
        });
    }

    private static void NewMethod(IServiceCollection services)
    {
        Random jitterer = new Random();

        // our CatalogClient class needs an instance of the httpClient in order to work
        // so we bind the CatalogClient by writing "AddHttpClient<CatalogClient>"
        // however, before we inject the HTTPClient, we need to specify the correct address
        // we can do this using a configuration action defined as a lamda expression
        services.AddHttpClient<CatalogClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5180");
        })
        // implement exponential backoff
        // TransientHttpErrorPolicy handles 500 errors (internal server errors)
        // "Or<TimeoutRejectedException>()": if we fail because of timeout produced by the policy below (more than 1 sec),
        // we will also retry
        .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
            5,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                            + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000))

        ))
        // if the request fails more than 3 times "open the circuit" for 15 seconds
        // "open the curcuit": immediately fail the request so we do not waste threads
        .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
            3,
            TimeSpan.FromSeconds(15)
        ))
        // wait one second before giving up
        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        app.UseStaticFiles();
    }

    private static void AddCatalogClient(IServiceCollection services)
    {
        Random jitterer = new Random();
        
        // our CatalogClient class needs an instance of the httpClient in order to work
        // so we bind the CatalogClient by writing "AddHttpClient<CatalogClient>"
        // however, before we inject the HTTPClient, we need to specify the correct address
        // we can do this using a configuration action defined as a lamda expression
        services.AddHttpClient<CatalogClient>(client => 
        {
            client.BaseAddress = new Uri("http://localhost:5180");
        })
        // implement exponential backoff
        // TransientHttpErrorPolicy handles 500 errors (internal server errors)
        // "Or<TimeoutRejectedException>()": if we fail because of timeout produced by the policy below (more than 1 sec),
        // we will also retry
        .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
            5,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                            + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000))

        ))
        // if the request fails more than 3 times "open the circuit" for 15 seconds
        // "open the curcuit": immediately fail the request so we do not waste threads
        .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
            3,
            TimeSpan.FromSeconds(15)
        ))
        // wait one second before giving up
        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
    }
}