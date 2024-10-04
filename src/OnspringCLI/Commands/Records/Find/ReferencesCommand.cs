namespace OnspringCLI.Commands.Records.Find;

public class ReferencesCommand : Command
{
  public ReferencesCommand() : base("references", "Locate references to records")
  {
    AddOption(
      new Option<int>(
        aliases: ["--app-id", "-a"],
        description: "The app id to search for references in."
      )
      {
        IsRequired = true
      }
    );

    AddOption(
      new Option<List<int>>(
        aliases: ["--record-ids", "-r"],
        description: "A comma separated list of record ids to find references for.",
        parseArgument: result => result.ParseToIntegerList()
      )
      {
        IsRequired = true
      }
    );

    AddOption(
      new Option<string>(
        aliases: ["--output-directory", "-o"],
        description: "The name of the directory to write the report to.",
        getDefaultValue: () => "output"
      )
    );
  }
}