namespace OnspringCLI.Extensions;

static class HostBuilderExtensions
{
  public static IHostBuilder AddSerilog(this IHostBuilder hostBuilder)
  {
    return hostBuilder
    .UseSerilog(
      (hostingContext, services, loggerConfiguration) =>
      {
        var logLevelSwitch = services.GetRequiredService<LoggingLevelSwitch>();

        loggerConfiguration
        .MinimumLevel.ControlledBy(logLevelSwitch)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "OnspringCLI")
        .WriteTo.Console(
          restrictedToMinimumLevel: LogEventLevel.Information,
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
        services.AddOptions<OnspringClientOptions>().BindCommandLine();
        services.AddSingleton<IOnspringService, OnspringService>();
        services.AddSingleton<IAttachmentsProcessor, AttachmentsProcessor>();
        services.AddSingleton<IReportService, ReportService>();
      }
    );
  }

  public static IHostBuilder AddCommandHandlers(this IHostBuilder hostBuilder)
  {
    return hostBuilder
    .UseCommandHandler<ReporterCommand, ReporterCommand.Handler>();
  }
}