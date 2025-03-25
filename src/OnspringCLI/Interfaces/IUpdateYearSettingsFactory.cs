namespace OnspringCLI.Interfaces;

public interface IUpdateYearSettingsFactory
{
  Task<UpdateYearSettings> CreateAsync(FileInfo? file);
}