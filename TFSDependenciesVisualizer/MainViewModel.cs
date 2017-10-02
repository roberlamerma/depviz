using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TFSDependencyVisualizer.Helpers;

namespace TFSDependenciesVisualizer
{
    class MainViewModel
    {
        public WorkItemStore WorkItemStore { get; private set; }

        public Action<object, MouseButtonEventArgs> DoubleClickDelegate { get; private set; }

        public MainViewModel()
        {
            this.Initialize();

            this.DoubleClickDelegate = (x, y) => { };
        }

        private void Initialize()
        {
            var urlString = ConfigurationManager.AppSettings["tfsUrl"];
            var projectString = ConfigurationManager.AppSettings["projectName"];

            if (TFSConnector.IsTFSUrlValid(urlString, projectString, out var uri))
            {
                try
                {
                    this.WorkItemStore = TFSConnector.GetWorkItemStore(uri);
                }
                catch (Exception)
                {
                    
                }
            }
        }
    }
}
