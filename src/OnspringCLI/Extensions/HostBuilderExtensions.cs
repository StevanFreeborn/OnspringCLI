namespace OnspringCLI.Extensions;

[ExcludeFromCodeCoverage]
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
            .Destructure.With(
              new OnspringFileRequestDestructuringPolicy(),
              new OnspringSaveFileRequestDestructuringPolicy()
            )
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal)
            .Enrich.FromLogContext()
            .WriteTo.Logger(
              lc =>
                lc
                  .MinimumLevel.Verbose()
                  .WriteTo.File(
                    new CompactJsonFormatter(),
                    logFilePath,
                    options.LogLevel
                  )
            )
            .WriteTo.Logger(
              lc =>
                lc
                  .MinimumLevel.ControlledBy(logLevelSwitch)
                  .WriteTo.Console(
                    theme: AnsiConsoleTheme.Code
                  )
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
          services.AddSingleton<IProgressBarFactory, ProgressBarFactory>();
          services.AddSingleton<IOnspringService, OnspringService>();
          services.AddSingleton<IAttachmentsProcessor, AttachmentsProcessor>();
          services.AddSingleton<IRecordsProcessor, RecordsProcessor>();
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
      .UseCommandHandler<TransferCommand, TransferCommand.Handler>()
      .UseCommandHandler<ReferencesCommand, ReferencesCommand.Handler>();
  }
}