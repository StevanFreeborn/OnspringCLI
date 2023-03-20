namespace OnspringCLI.Interfaces;

public interface IAttachmentTransferSettings
{
  string SourceInstanceKey { get; set; }
  string TargetInstanceKey { get; set; }
  int SourceAppId { get; set; }
  int TargetAppId { get; set; }
  int SourceMatchFieldId { get; set; }
  int TargetMatchFieldId { get; set; }
  Dictionary<int, int> AttachmentFieldMappings { get; set; }
  int ProcessFlagFieldId { get; set; }
  string ProcessFlagValue { get; set; }
  string ProcessedFlagValue { get; set; }
}