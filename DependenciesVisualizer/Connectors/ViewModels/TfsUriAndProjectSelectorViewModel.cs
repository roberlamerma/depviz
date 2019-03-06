using DependenciesVisualizer.Helpers;
using DependenciesVisualizer.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace DependenciesVisualizer.Connectors.ViewModels
{
    public class TfsUriAndProjectSelectorViewModel : ViewModelBase
    {

        private string tempTfsUrlString;
        private Uri tempTfsUrlUri;
        private string tempProjectName;

        public TfsUriAndProjectSelectorViewModel()
        {
            this.HaveSettingsChanged = false;

            this.tempTfsUrlString = Properties.Settings.Default.tfsUrl;
            this.tempProjectName = Properties.Settings.Default.tfsprojectName;
            this.tempTfsUrlUri = new Uri(this.tempTfsUrlString);

            this.Connect = new RelayCommand<object>(this.ExecuteConnect, o => true);
            this.Cancel = new RelayCommand<object>(this.ExecuteCancel, o => true);
        }

        public string TfsUri
        {
            get
            {
                return this.tempTfsUrlString;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    //Properties.Settings.Default.tfsUrl = value;
                    this.tempTfsUrlString = value;
                }
            }
        }

        public ICommand Connect { get; private set; }

        public ICommand Cancel { get; private set; }

        public WorkItemStore Store { get; private set; }

        public bool HaveSettingsChanged { get; private set; }

        public string ProjectName
        {
            get
            {
                return tempProjectName;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    //Properties.Settings.Default.tfsprojectName = value;
                    this.tempProjectName = value;
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
        private string errorMessage;

        private bool IsTfsUrlValid(string tfsUrl, out Uri uriResult)
        {
            bool result = Uri.TryCreate(tfsUrl, UriKind.Absolute, out uriResult)
                          && uriResult.Scheme == Uri.UriSchemeHttp;

            return result;
        }

        private void ExecuteCancel(object obj)
        {
            var uControl = obj as UserControl;
            ((Window)uControl.Parent).Close();
        }

        private void ExecuteConnect(object obj)
        {
            try
            {
                // If any of the settings has changed, we need to verify against TFS
                if (Properties.Settings.Default.tfsUrl != this.TfsUri || Properties.Settings.Default.tfsprojectName != this.ProjectName)
                {
                    this.HaveSettingsChanged = true;

                    if (!IsTfsUrlValid(this.TfsUri, out tempTfsUrlUri))
                    {
                        throw new Exception(string.Format(@"The url '{0}' is not valid", this.TfsUri));
                    }

                    var store = TfsHelper.GetWorkItemStore(new Uri(this.TfsUri));

                    if (store == null)
                    {
                        throw new CannotConnectException(string.Format(@"Cannot connect to TFS uri: {0}", this.TfsUri));
                    }

                    var project = store.Projects[this.tempProjectName];

                    // If no exceptions are captured, the values are saved
                    Properties.Settings.Default.tfsUrl = this.TfsUri;
                    Properties.Settings.Default.tfsprojectName = this.ProjectName;

                    // Save the settings
                    Properties.Settings.Default.Save();

                    this.Store = store;
                }

                this.ExecuteCancel(obj);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
                this.HaveSettingsChanged = false;
            }
        }
    }
}
