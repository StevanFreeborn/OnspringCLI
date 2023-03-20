namespace OnspringCLI.Models;

public class AttachmentTransferSettings : IAttachmentTransferSettings
{
  public string SourceInstanceKey { get; set; } = string.Empty;
  public string TargetInstanceKey { get; set; } = string.Empty;
  public int SourceAppId { get; set; } = 0;
  public int TargetAppId { get; set; } = 0;
  public int SourceMatchFieldId { get; set; } = 0;
  public int TargetMatchFieldId { get; set; } = 0;
  public Dictionary<int, int> AttachmentFieldMappings { get; set; } = new Dictionary<int, int>();
  public int ProcessFlagFieldId { get; set; } = 0;
  public string ProcessFlagValue { get; set; } = string.Empty;
  public string ProcessedFlagValue { get; set; } = string.Empty;
}