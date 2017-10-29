using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace DependenciesVisualizer.Connectors.Models
{
    [DelimitedRecord(";"), IgnoreFirst(1)]
    public class CsvDependency
    {
        public string Title;
        public int Id;

        public string SuccessorIds;

        public string Status;
        public string Tags;
    }
}
