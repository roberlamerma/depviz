using System;
using System.Collections.Generic;

namespace DependenciesVisualizer
{
    public class DependencyItem
    {
        public DependencyItem(int id)
        {
            this.Id = id;
            this.Successors = new List<int>();
            this.Tags = new List<string>();
        }

        public int Id { get; }
        public string Title { get; set; }
        public List<int> Successors { get; }
        public List<string> Tags { get; }

        public override string ToString()
        {
            var longTitle = string.Format(@"<{0}> {1}", this.Id, this.Title);
            try
            {
                var longTitleLenght = longTitle.Length;
                if (longTitleLenght < 50)
                {
                    return longTitle;
                }
                else if (longTitleLenght > 90)
                {
                    var firstLine = longTitle.Substring(0, 49);
                    var secondLine = longTitle.Substring(49, 49);
                    return string.Format(@"{0}{1}{2}...\l", firstLine, Environment.NewLine, secondLine);
                }
                else
                {
                    var firstLine = longTitle.Substring(0, 49);
                    var secondLine = longTitle.Substring(49, longTitleLenght - 50 + 1);
                    return string.Format(@"{0}{1}{2}\l", firstLine, Environment.NewLine, secondLine);
                }
            } catch (Exception)
            {
                return longTitle; // Just in case...
            }
        }
    }
}
