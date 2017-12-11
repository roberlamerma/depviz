using DependenciesVisualizer.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependenciesVisualizer.Connectors.ViewModels
{
    public class TfsUriAndProjectSelectorViewModel : ViewModelBase
    {
        public string TfsUri
        {
            get
            {
                return ConfigurationManager.AppSettings["tfsUrl"];
            }
            set
            {
                Uri uriResult;
                if (!string.IsNullOrWhiteSpace(value) && IsTfsUrlValid(value, out uriResult))
                {
                    ConfigurationManager.AppSettings["tfsUrl"] = value;
                }
            }
        }

        public string ProjectName
        {
            get
            {
                return ConfigurationManager.AppSettings["tfsprojectName"];
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    ConfigurationManager.AppSettings["tfsprojectName"] = value;

                    // https://stackoverflow.com/questions/5274829/configurationmanager-appsettings-how-to-modify-and-save
                    // https://stackoverflow.com/questions/4216809/configurationmanager-doesnt-save-settings
                }
            }
        }

        private bool IsTfsUrlValid(string tfsUrl, out Uri uriResult)
        {
            bool result = Uri.TryCreate(tfsUrl, UriKind.Absolute, out uriResult)
                          && uriResult.Scheme == Uri.UriSchemeHttp;

            return result;
        }
    }
}
