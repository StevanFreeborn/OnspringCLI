namespace OnspringCLI.Extensions;

static class HostBuilderExtensions
{
  public static IHostBuilder AddServices(this IHostBuilder hostBuilder)
  {
    return hostBuilder.ConfigureServices(
      (context, services) => services.AddSerilog()
    );
  }

  public static IHostBuilder AddCommandHandlers(this IHostBuilder hostBuilder)
  {
    return hostBuilder
    .UseCommandHandler<AttachmentsCommand, AttachmentsCommand.Handler>();
  }
}