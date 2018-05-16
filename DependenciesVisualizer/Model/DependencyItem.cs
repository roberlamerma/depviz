using System.Collections.Generic;

namespace DependenciesVisualizer.Model
{
    public class DependencyItem
    {
        public DependencyItem(int id)
        {
            this.Id = id;
            this.Successors = new List<int>();
            this.Tags = new List<string>();
        }

        public DependencyItem(int id, string title, List<int> successors, List<string> tags)
        {
            this.Id = id;
            this.Title = title;
            this.Successors = successors;
            this.Tags = tags;
        }

        public int Id { get; }
        public string Title { get; set; }

        public string State { get; set; }

        public string ReducedTitle {
            get
            {
                if (this.Title != null) {
                    var titleLength = this.Title.Length;
                    if (titleLength < 50)
                    {
                        return this.Title;
                    } else
                    {
                        var firstLine = this.Title.Substring(0, 49);
                        var secondLine = this.Title.Substring(49, titleLength - 50 + 1);
                        return string.Format(@"{0}{1}{2}", firstLine, @"&#92;", secondLine);
                    }
                }

                return null;
            }
        }
        public List<int> Successors { get; }
        public List<string> Tags { get; }

        public override string ToString()
        {
            return string.Format(@"<{0}> State: {1} | Title: {2}", this.Id, this.State, this.Title);
            //var longTitle = this.Title;
            //try
            //{
            //    var longTitleLenght = longTitle.Length;
            //    if (longTitleLenght < 50)
            //    {
            //        return longTitle;
            //    }
            //    else if (longTitleLenght > 90)
            //    {
            //        var firstLine = longTitle.Substring(0, 49);
            //        var secondLine = longTitle.Substring(49, 49);
            //        return string.Format(@"{0}{1}{2}...\l", firstLine, @"&#92;", secondLine);
            //    }
            //    else
            //    {
            //        var firstLine = longTitle.Substring(0, 49);
            //        var secondLine = longTitle.Substring(49, longTitleLenght - 50 + 1);
            //        return string.Format(@"{0}{1}{2}\l", firstLine, @"&#92;", secondLine);
            //    }
            //} catch (Exception)
            //{
            //    return longTitle; // Just in case...
            //}
        }
    }
}
