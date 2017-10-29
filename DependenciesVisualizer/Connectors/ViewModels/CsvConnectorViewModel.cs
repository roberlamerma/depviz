using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CsvHelper;
using DependenciesVisualizer.Connectors.Models;
using DependenciesVisualizer.Connectors.Services;
using DependenciesVisualizer.Contracts;
using DependenciesVisualizer.ViewModels;
using FileHelpers;
using Microsoft.Win32;
using Ninject;

namespace DependenciesVisualizer.Connectors.ViewModels
{
    public class CsvConnectorViewModel : ViewModelBase, IConnectorViewModel
    {
        private readonly ICsvService csvService;
        private string selectedCsvFile;

        [Inject]
        public CsvConnectorViewModel(ICsvService csvService)
        {
            this.csvService = csvService;
            this.PickCsvFile = new RelayCommand<object>(this.ExecutePickCsvFile, o => true);
        }

        private void ExecutePickCsvFile(object o)
        {
            OpenFileDialog openPicker = new OpenFileDialog();

            openPicker.DefaultExt = ".csv";
            openPicker.Filter = "Comma Separated files|*.csv";

            bool? result = openPicker.ShowDialog();

            if (result == true)
            {

                this.csvService.ImportDependenciesFromCsvFile(openPicker.FileName);


                //using (var sr = new StreamReader(openPicker.FileName))
                //{
                //    var reader = new CsvReader(sr);
                //    reader.Configuration.HasHeaderRecord = true;
                //    reader.Configuration.Delimiter = ";";

                //    //CSVReader will now read the whole file into an enumerable
                //    IEnumerable<CsvDependency> records = reader.GetRecords<CsvDependency>();

                //    //First 5 records in CSV file will be printed to the Output Window
                //    foreach (CsvDependency record in records.ToList())
                //    {
                //        Debug.Print("{0} {1}, {2}, {3}", record.Title, record.Id, record.SuccessorIds,
                //                    record.Status, record.Tags);
                //    }
                //}


                this.SelectedCsvFile = openPicker.FileName;
            }
        }

        public string Name => "CSV";

        public void Initialize()
        {
            var csvFile = ConfigurationManager.AppSettings["csvFile"];

            if (!string.IsNullOrWhiteSpace(csvFile))
            {
                if (File.Exists(csvFile))
                {
                    this.selectedCsvFile = csvFile;
                }
                else
                {
                    ConfigurationManager.AppSettings["csvFile"] = string.Empty;
                }
            }
            else
            {
                this.selectedCsvFile = null;
            }
        }

        public IDependenciesService DependenciesService => (IDependenciesService)this.csvService;

        public string SelectedCsvFile
        {
            get => this.selectedCsvFile;
            set
            {
                if (this.selectedCsvFile == value)
                {
                    return;
                }

                this.selectedCsvFile = value;
                this.OnPropertyChanged("SelectedCsvFile");
            }
        }

        public ICommand PickCsvFile { get; private set; }
    }
}
