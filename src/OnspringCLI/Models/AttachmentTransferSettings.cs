namespace OnspringCLI.Models;

public class AttachmentTransferSettings : IAttachmentTransferSettings
{
  public int SourceAppId { get; set; } = 0;
  public int TargetAppId { get; set; } = 0;
  public int SourceMatchFieldId { get; set; } = 0;
  public int TargetMatchFieldId { get; set; } = 0;
  public Dictionary<string, int> AttachmentFieldMappings { get; set; } = new Dictionary<string, int>();
  public Dictionary<int, int> AttachmentFieldIdMappings => AttachmentFieldMappings.ToDictionary(
    kvp => int.TryParse(kvp.Key, out var id) ? id : 0,
    kvp => kvp.Value
  );
  public int ProcessFlagFieldId { get; set; } = 0;
  public string ProcessFlagValue { get; set; } = string.Empty;
  public string ProcessedFlagValue { get; set; } = string.Empty;
  public Guid ProcessFlagListValueId { get; set; } = Guid.Empty;
  public Guid ProcessedFlagListValueId { get; set; } = Guid.Empty;
}