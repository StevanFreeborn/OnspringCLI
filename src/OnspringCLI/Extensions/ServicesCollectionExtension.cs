namespace OnspringCLI.Extensions;

static class ServicesCollectionExtension
{
  public static IServiceCollection AddSerilog(this IServiceCollection services)
  {
    Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

    return services;
  }
}