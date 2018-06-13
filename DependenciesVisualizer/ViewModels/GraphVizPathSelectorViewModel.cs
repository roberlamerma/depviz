using DependenciesVisualizer.Helpers;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace DependenciesVisualizer.ViewModels
{
    public class GraphVizPathSelectorViewModel : ViewModelBase
    {
        private string graphVizPath;
        private string errorMessage;

        public ICommand Close { get; private set; }
        public ICommand SetPath { get; private set; }

        public bool CloseApplication { get; private set; }

        public GraphVizPathSelectorViewModel()
        {
            // ToDo: Inject the logger, and log errors

            this.graphVizPath = Properties.Settings.Default.graphvizPath;
            this.CloseApplication = false;

            this.Close = new RelayCommand<object>(this.ExecuteClose, o => true);
            this.SetPath = new RelayCommand<object>(this.ExecuteSetPath, o => true);
        }

        private void ExecuteSetPath(object obj)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.ShowNewFolderButton = false;
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && Directory.Exists(fbd.SelectedPath))
                {
                    try
                    {
                        GraphVizHelper.TryGraphvizPath(fbd.SelectedPath);

                        this.GraphVizPath = fbd.SelectedPath;

                        ((Window)obj).Close();
                    }
                    catch (Exception)
                    {
                        this.ErrorMessage = string.Format(@"The path '{0}' is not a valid Graphviz path. Remember to select the 'bin' directory (i.g. 'C:\Program Files (x86)\Graphviz2.38\bin')", fbd.SelectedPath);
                    }
                }
            }
        }

        private void ExecuteClose(object obj)
        {
            this.CloseApplication = true;
            ((Window)obj).Close();
        }

        public string GraphVizPath
        {
            get => this.graphVizPath;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.graphVizPath = value;

                    // If no exceptions are captured, the values are saved
                    Properties.Settings.Default.graphvizPath = this.graphVizPath;

                    // Save the settings
                    Properties.Settings.Default.Save();
                    this.OnPropertyChanged("GraphVizPath");
                }
            }
        }

        public string ErrorMessage
        {
            get => this.errorMessage;
            set
            {
                if (this.errorMessage == value)
                {
                    return;
                }

                this.errorMessage = value;
                this.OnPropertyChanged("ErrorMessage");
            }
        }
        
    }
}
