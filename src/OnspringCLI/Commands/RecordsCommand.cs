namespace OnspringCLI.Commands;

public class RecordsCommand : Command
{
  public RecordsCommand() : base("records", "Manage records")
  {
    AddCommand(new FindCommand());
  }
}