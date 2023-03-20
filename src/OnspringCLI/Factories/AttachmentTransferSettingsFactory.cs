namespace OnspringCLI.Factories;

public class AttachmentTransferSettingsFactory : IAttachmentTransferSettingsFactory
{
  private readonly Func<IAttachmentTransferSettings> _factory;

  public AttachmentTransferSettingsFactory(
    Func<IAttachmentTransferSettings> factory
  )
  {
    _factory = factory;
  }

  public IAttachmentTransferSettings Create(FileInfo configFile)
  {
    var settings = _factory();

    new ConfigurationBuilder()
      .AddJsonFile(
        configFile.FullName,
        optional: false,
        reloadOnChange: true
      )
      .Build()
      .Bind(settings);

    return settings;
  }
}