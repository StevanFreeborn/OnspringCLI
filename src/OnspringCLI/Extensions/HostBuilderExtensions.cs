namespace OnspringCLI.Extensions;

static class HostBuilderExtensions
{
  public static IHostBuilder AddSerilog(this IHostBuilder hostBuilder)
  {
    return hostBuilder
    .UseSerilog(
      (hostingContext, services, loggerConfiguration) =>
      {
        var options = services.GetRequiredService<IOptions<GlobalOptions>>().Value;

        loggerConfiguration
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "OnspringCLI")
        .WriteTo.Console(
          restrictedToMinimumLevel: options.LogLevel,
          theme: AnsiConsoleTheme.Code
        );
      }
    );
  }

  public static IHostBuilder AddServices(this IHostBuilder hostBuilder)
  {
    return hostBuilder
    .ConfigureServices(
      (hostingContext, services) =>
      {
        var logLevelSwitch = new LoggingLevelSwitch(
          LogEventLevel.Debug
        );

        services.AddSingleton(logLevelSwitch);
        services.AddOptions<GlobalOptions>().BindCommandLine();
        services.AddSingleton<IOnspringService, OnspringService>();
        services.AddSingleton<IAttachmentsProcessor, AttachmentsProcessor>();
        services.AddSingleton<IReportService, ReportService>();
      }
    );
  }

  public static IHostBuilder AddCommandHandlers(this IHostBuilder hostBuilder)
  {
    return hostBuilder
    .UseCommandHandler<ReportCommand, ReportCommand.Handler>()
    .UseCommandHandler<BulkDownloadCommand, BulkDownloadCommand.Handler>();
  }
}