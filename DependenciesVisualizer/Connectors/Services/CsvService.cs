using System;
using System.Collections.Generic;
using System.Diagnostics;
using DependenciesVisualizer.Connectors.Models;
using DependenciesVisualizer.Model;
using DependenciesVisualizer.Contracts;
using FileHelpers;
using log4net;

namespace DependenciesVisualizer.Connectors.Services
{
    public class CsvService : IDependenciesService, ICsvService
    {
        //public Dictionary<int, DependencyItem> DependenciesModel { get; }
        private Dictionary<int, DependencyItem> dependenciesModel;

        public event EventHandler<EventArgs> DependenciesModelChanged = delegate { };

        public event EventHandler<EventArgs> DependenciesModelAboutToChange = delegate { };

        public ILog Logger { get; private set; }

        public CsvService(ILog logger)
        {
            this.Logger = logger;
        }

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
                string error = string.Format("[CSV] Column headers: '{0}' from file '{1}' do not match the expected ones: '{2}'", engine.HeaderText.Trim(), csvFile, engine.GetFileHeader());
                this.Logger.Error(error);
                throw new Exception(error);
            }

            var theModel = new Dictionary<int, DependencyItem>();
            DependencyItem tempItem;

            foreach (var csvDependency in records)
            {
                tempItem = new DependencyItem(
                    csvDependency.Id,
                    csvDependency.Title,
                    csvDependency.SuccessorIds ?? new List<int>(),
                    csvDependency.Tags ?? new List<string>());

                theModel.Add(csvDependency.Id, tempItem);
                this.Logger.Debug(string.Format(@"[CSV] Got: {0}", tempItem.ToString()));
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
