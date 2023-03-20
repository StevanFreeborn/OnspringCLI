namespace OnspringCLI.Processors;

public class AttachmentsTransferProcessor : IAttachmentsTransferProcessor
{/

  private readonly IOptions<GlobalOptions> _globalOptions;
  private readonly ILogger _logger;
  private readonly IOnspringService _onspringService;

  public AttachmentsTransferProcessor(
    IOptions<GlobalOptions> globalOptions,
    ILogger logger,
    IOnspringService onspringService
  )
  {
    _globalOptions = globalOptions;
    _logger = logger.ForContext<AttachmentsTransferProcessor>();
    _onspringService = onspringService;
  }

  public async Task<bool> ValidateMatchFields(
    IAttachmentTransferSettings settings
  )
  {
    var sourceMatchField = await _onspringService.GetField(
      _globalOptions.Value.SourceApiKey,
      settings.SourceMatchFieldId
    );

    var targetMatchField = await _onspringService.GetField(
      _globalOptions.Value.TargetApiKey,
      settings.TargetMatchFieldId
    );

    if (
      sourceMatchField is null ||
      IsValidMatchFieldType(sourceMatchField) is false ||
      targetMatchField is null ||
      IsValidMatchFieldType(targetMatchField) is false
    )
    {
      return false;
    }

    return true;
  }

  internal static bool IsValidMatchFieldType(Field field)
  {
    var isSupportedField = field.Type is
    FieldType.Text or
    FieldType.AutoNumber or
    FieldType.Date or
    FieldType.Number or
    FieldType.Formula;

    if (isSupportedField is false)
    {
      return false;
    }

    if (
      field.Type is FieldType.Formula &&
      field is FormulaField formulaField &&
      formulaField is not null
    )
    {
      return formulaField.OutputType is not FormulaOutputType.ListValue;
    }

    return true;
  }
}