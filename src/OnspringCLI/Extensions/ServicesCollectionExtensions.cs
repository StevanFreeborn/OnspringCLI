namespace OnspringCLI.Extensions;

public static class ServicesCollectionExtensions
{
  public static IServiceCollection AddAttachmentTransferSettingsFactory(
    this IServiceCollection services
  )
  {
    services.AddTransient<IAttachmentTransferSettings, AttachmentTransferSettings>();
    services.AddSingleton<Func<IAttachmentTransferSettings>>(
      sp => () => sp.GetRequiredService<IAttachmentTransferSettings>()
    );
    services.AddSingleton<IAttachmentTransferSettingsFactory, AttachmentTransferSettingsFactory>();

    return services;
  }
}