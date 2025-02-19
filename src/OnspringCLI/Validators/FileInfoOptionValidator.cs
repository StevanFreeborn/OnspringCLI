namespace OnspringCLI.Validators;

[ExcludeFromCodeCoverage]
public class FileInfoOptionValidator
{
  public static void Validate(OptionResult result)
  {
    var value = result.GetValueOrDefault<FileInfo>();

    if (value is not null && value.Exists is false)
    {
      result.ErrorMessage = $"The file {value.FullName} does not exist.";
    }
  }
}