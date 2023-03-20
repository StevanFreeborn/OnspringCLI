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
        var options = services.GetRequiredService<IOptions<GlobalOptions>>().Value;
        var logFilePath = Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory,
          "logs",
          $"{DateTime.Now:yyyy_MM_dd-HH_mm_ss}_log.json"
        );

        loggerConfiguration
        .MinimumLevel.Verbose()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal)
        .Enrich.FromLogContext()
        .WriteTo.File(
          new CompactJsonFormatter(),
          logFilePath,
          options.LogLevel
        )
        .WriteTo.Console(
          restrictedToMinimumLevel: logLevelSwitch.MinimumLevel,
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
          LogEventLevel.Information
        );

        services.AddSingleton(logLevelSwitch);
        services.AddOptions<GlobalOptions>().BindCommandLine();
        services.AddSingleton<IOnspringClientFactory, OnspringClientFactory>();
        services.AddSingleton<IAttachmentTransferSettingsFactory, AttachmentTransferSettingsFactory>();
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
    .UseCommandHandler
    <
      Commands.Attachments.Download.BulkCommand,
      Commands.Attachments.Download.BulkCommand.Handler
    >()
    .UseCommandHandler
    <
      Commands.Attachments.Delete.BulkCommand,
      Commands.Attachments.Delete.BulkCommand.Handler
    >()
    .UseCommandHandler<TransferCommand, TransferCommand.Handler>();
  }
}