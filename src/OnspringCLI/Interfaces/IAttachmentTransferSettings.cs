namespace OnspringCLI.Interfaces;

public interface IAttachmentTransferSettings
{
  int SourceAppId { get; set; }
  int TargetAppId { get; set; }
  int SourceMatchFieldId { get; set; }
  int TargetMatchFieldId { get; set; }
  Dictionary<string, int> AttachmentFieldMappings { get; set; }
  Dictionary<int, int> AttachmentFieldIdMappings { get; }
  int ProcessFlagFieldId { get; set; }
  string ProcessFlagValue { get; set; }
  string ProcessedFlagValue { get; set; }
  Guid ProcessFlagListValueId { get; set; }
  Guid ProcessedFlagListValueId { get; set; }
}