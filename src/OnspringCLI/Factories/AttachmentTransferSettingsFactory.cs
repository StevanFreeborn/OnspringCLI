namespace OnspringCLI.Factories;

public class AttachmentTransferSettingsFactory : IAttachmentTransferSettingsFactory
{
  public IAttachmentTransferSettings Create(FileInfo settingsFile)
  {
    var settings = new AttachmentTransferSettings();

    var config = new ConfigurationBuilder()
      .AddJsonFile(settingsFile.FullName, optional: false, reloadOnChange: true)
      .Build();

    config.Bind(settings);

    return settings;
  }
}