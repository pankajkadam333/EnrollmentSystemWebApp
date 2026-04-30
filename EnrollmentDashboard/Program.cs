using EnrollmentDashboard.Business;
using EnrollmentDashboard.DataAccess;
using Serilog;

// Bootstrap logger for startup errors (before config is loaded)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Enrollment Dashboard application");

    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with Serilog, configured from appsettings.json
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    // Register application services — toggle via UseMockData setting
    var useMockData = builder.Configuration.GetValue<bool>("UseMockData");
    Log.Information("UseMockData = {UseMockData}", useMockData);

    if (useMockData)
    {
        builder.Services.AddScoped<IEnrollmentRepository, InMemoryEnrollmentRepository>();
    }
    else
    {
        builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
    }
    builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

    var app = builder.Build();

    // Serilog request logging — logs method, path, status code, elapsed time
    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
