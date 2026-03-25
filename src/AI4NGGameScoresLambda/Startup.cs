using AI4NGGameScoresLambda.Interfaces;
using AI4NGGameScoresLambda.Services;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
namespace AI4NGGameScoresLambda;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

        // AWS SDK config (uses environment/role automatically)
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient());

        // Http context (for user claims)
        services.AddHttpContextAccessor();

        // Custom services
        services.AddScoped<IUserContextService, UserContextService>();
        services.AddScoped<IParticipantScoreProfileResolver, ParticipantScoreProfileResolver>();
        services.AddScoped<IGameScoresService, GameScoresService>();
        services.AddScoped<IGameScoresRepository, GameScoresRepository>();

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
            });
        });
    }
}