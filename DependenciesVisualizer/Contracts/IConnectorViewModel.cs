namespace DependenciesVisualizer.Contracts
{
    public interface IConnectorViewModel
    {
        string Name { get; }

        bool IsConfigurable { get; }

        void Initialize();

        IDependenciesService DependenciesService { get; }
    }
}