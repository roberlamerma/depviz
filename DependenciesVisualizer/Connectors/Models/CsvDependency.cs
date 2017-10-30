using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace DependenciesVisualizer.Connectors.Models
{
    [DelimitedRecord(";")]
    [IgnoreFirst(1)]
    [IgnoreEmptyLines()]
    public class CsvDependency
    {
        public string Title;
        public int Id;

        [FieldConverter(typeof(CommaSeparatedtoIntListConverter))]
        public List<int> SuccessorIds;

        public string Status;

        [FieldConverter(typeof(CommaSeparatedtoStringListConverter))]
        public List<string> Tags;
    }

    /*
     * 
     * ToDo: make the CommaSeparatedtoStringListConverter generic, 
     * i.g. CommaSeparatedtoListConverter<T>
     */
    public class CommaSeparatedtoStringListConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            if (!string.IsNullOrWhiteSpace(from))
            {
                if (from.Contains(","))
                {
                    var nonSpacesString = from.Replace(" ", string.Empty);
                    return nonSpacesString.Split(',').ToList<string>();
                }
                else
                {
                    return new List<string>(){from};
                }
            }

            return null;
        }
    }

    public class CommaSeparatedtoIntListConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            if (!string.IsNullOrWhiteSpace(from))
            {
                if (from.Contains(","))
                {
                    var nonSpacesString = from.Replace(" ", string.Empty);
                    var strArray = nonSpacesString.Split(',');
                    var ret = new List<int>(strArray.Length);
                    ret.AddRange(strArray.Select(s => Convert.ToInt32(s)));
                    return ret;
                }
                else
                {
                    return new List<int>() { Convert.ToInt32(from) };
                }
            }

            return null;
        }
    }
}
