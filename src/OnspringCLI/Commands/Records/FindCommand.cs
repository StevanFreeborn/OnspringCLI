namespace OnspringCLI.Commands.Records;

public class FindCommand : Command
{
  public FindCommand() : base("find", "Find records")
  {
    AddCommand(new ReferencesCommand());
  }
}