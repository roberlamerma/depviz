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
        public Dictionary<int, DependencyItem> DependenciesModel { get; }
        public event EventHandler<EventArgs> DependenciesModelChanged;

        public void RaiseDependenciesModelChanged()
        {
            throw new NotImplementedException();
        }

        public void ImportDependenciesFromCsvFile(string csvFile)
        {
            var engine = new FileHelperEngine<CsvDependency>();

            var records = engine.ReadFile(csvFile);

            if (engine.GetFileHeader() != engine.HeaderText.Trim())
            {
                throw new Exception(string.Format("Read CSV column headers: '{0}' from file '{1}' do not match the expected ones: '{2}'", engine.HeaderText.Trim(), csvFile, engine.GetFileHeader()));
            }

            // ToDo: finish importing dependencies
        }
    }
}
