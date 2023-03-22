namespace OnspringCLI.Extensions;

[ExcludeFromCodeCoverage]
public static class ArgumentResultExtensions
{
  public static List<int> ParseToIntegerList(this ArgumentResult result)
  {
    return result
    .Tokens[0]
    .Value
    .Split(',',
      StringSplitOptions.TrimEntries |
      StringSplitOptions.RemoveEmptyEntries
    )
    .Select(
      int.Parse
    )
    .ToList();
  }
}