using System;
using System.Collections.Generic;
using System.Diagnostics;
using DependenciesVisualizer.Connectors.Models;
using DependenciesVisualizer.Model;
using DependenciesVisualizer.Contracts;
using FileHelpers;

namespace DependenciesVisualizer.Connectors.Services
{
    public class CsvService : IDependenciesService, ICsvService
    {
        //public Dictionary<int, DependencyItem> DependenciesModel { get; }
        private Dictionary<int, DependencyItem> dependenciesModel;

        public event EventHandler<EventArgs> DependenciesModelChanged = delegate { };

        public void RaiseDependenciesModelChanged()
        {
            this.DependenciesModelChanged(this, EventArgs.Empty);
        }

        public void ImportDependenciesFromCsvFile(string csvFile)
        {
            var engine = new FileHelperEngine<CsvDependency>();

            var records = engine.ReadFile(csvFile);

            if (engine.GetFileHeader() != engine.HeaderText.Trim())
            {
                throw new Exception(string.Format("Read CSV column headers: '{0}' from file '{1}' do not match the expected ones: '{2}'", engine.HeaderText.Trim(), csvFile, engine.GetFileHeader()));
            }

            var theModel = new Dictionary<int, DependencyItem>();

            foreach (var csvDependency in records)
            {
                theModel.Add(csvDependency.Id, new DependencyItem(
                    csvDependency.Id,
                    csvDependency.Title,
                    csvDependency.SuccessorIds ?? new List<int>(),
                    csvDependency.Tags ?? new List<string>()));
            }

            this.DependenciesModel = theModel;
            // ToDo: finish importing dependencies
        }

        public Dictionary<int, DependencyItem> DependenciesModel
        {
            get => this.dependenciesModel;
            private set
            {
                if (this.dependenciesModel == value)
                {
                    return;
                }

                this.dependenciesModel = value;
                this.RaiseDependenciesModelChanged();
            }
        }
    }
}
