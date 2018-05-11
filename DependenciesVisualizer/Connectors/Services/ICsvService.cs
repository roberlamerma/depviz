namespace DependenciesVisualizer.Connectors.Services
{
    public interface ICsvService
    {
        void ImportDependenciesFromCsvFile(string csvFile);
    }
}
