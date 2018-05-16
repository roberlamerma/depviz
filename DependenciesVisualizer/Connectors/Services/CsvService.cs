using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Event arised when the DependenciesModel changed
        /// </summary>
        public event EventHandler<EventArgs> DependenciesModelChanged = delegate { };

        /// <summary>
        /// Event arised when the DependenciesModel is about to change
        /// </summary>
        public event EventHandler<EventArgs> DependenciesModelAboutToChange = delegate { };

        /// <summary>
        /// Event arised when the DependenciesModel could not be changed (error, exception...)
        /// </summary>
        public event EventHandler<EventArgs> DependenciesModelCouldNotBeChanged = delegate { };

        public ILog Logger { get; private set; }

        public CsvService(ILog logger)
        {
            this.Logger = logger;
        }

        public void RaiseDependenciesModelChanged()
        {
            this.DependenciesModelChanged(this, EventArgs.Empty);
        }

        public void RaiseDependenciesModelAboutToChange()
        {
            this.DependenciesModelAboutToChange(this, EventArgs.Empty);
        }

        public void RaiseDependenciesModelCouldNotBeChanged()
        {
            this.DependenciesModelCouldNotBeChanged(this, EventArgs.Empty);
        }

        public void ImportDependenciesFromCsvFile(string csvFile)
        {
            try
            {
                this.RaiseDependenciesModelAboutToChange();

                var engine = new FileHelperEngine<CsvDependency>();

                var records = engine.ReadFile(csvFile);

                if (engine.GetFileHeader() != engine.HeaderText.Trim())
                {
                    throw new Exception(string.Format("[CSV] Column headers: '{0}' from file '{1}' do not match the expected ones: '{2}'", engine.HeaderText.Trim(), csvFile, engine.GetFileHeader()));
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

                    tempItem.State = csvDependency.Status;

                    theModel.Add(csvDependency.Id, tempItem);
                    this.Logger.Debug(string.Format(@"[CSV] Got: {0}", tempItem.ToString()));
                }

                this.DependenciesModel = theModel;
                this.RaiseDependenciesModelChanged();
            }
            catch (Exception ex)
            {
                this.Logger.Error(string.Format(@"{0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace));
                this.RaiseDependenciesModelCouldNotBeChanged();
                throw;
            }
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
            }
        }
    }
}
