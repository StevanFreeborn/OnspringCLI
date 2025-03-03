
namespace OnspringCLI.Commands.Records.Update.Bulk;

public class YearCommand : Command
{
  public YearCommand() : base("year", "Adjusts the value of a list and/or date field by given years")
  {
    var settingsFileOption = new Option<FileInfo>(
      aliases: ["--file", "-f"],
      description: "The path to the .csv file that contains the list of apps and fields to update."
    )
    {
      IsRequired = true
    };

    settingsFileOption.AddValidator(FileInfoOptionValidator.Validate);

    AddOption(settingsFileOption);

    AddOption(
      new Option<int>(
        aliases: ["--years", "-y"],
        description: "The number of years to adjust the field by."
      )
      {
        IsRequired = true
      }
    );
  }

  public new class Handler : ICommandHandler
  {
    public Task<int> InvokeAsync(InvocationContext context)
    {
      // TODO: Implement this method
      // 1. Parse the csv file to get a list of apps and their fields
      // 2. Validate the apps and fields exist and can be accessed by this app
      // 3. For each app page through the records and update the field value of fields
      //    that match a field in the list
      //    a. If the field is a list field, get the correct GUID for the target year. Add the value if necessary.
      //    b. If the field is a date field, adjust the date by the number of years
      //    c. Update the record with the new field values
      //    d. Log the record id and the field that was updated
      //    e. If the record could not be updated, log the record id and the error message, and continue to the next record
      // 4. Log the number of records updated and the number of records that could not be updated
      // 5. Return 0 if all records were updated successfully, 1 if any records could not be updated
      // 6. Write out errors to a file if any records could not be updated
      throw new NotImplementedException();
    }

    [ExcludeFromCodeCoverage]
    public int Invoke(InvocationContext context)
    {
      throw new NotImplementedException();
    }

  }
}