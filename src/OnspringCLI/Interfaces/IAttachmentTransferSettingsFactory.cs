namespace OnspringCLI.Interfaces;

public interface IAttachmentTransferSettingsFactory
{
  IAttachmentTransferSettings Create(FileInfo configFile);
}