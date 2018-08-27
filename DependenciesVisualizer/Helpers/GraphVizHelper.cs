using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DependenciesVisualizer.Model;
using Shields.GraphViz.Components;
using Shields.GraphViz.Models;
using Shields.GraphViz.Services;

namespace DependenciesVisualizer.Helpers
{
    public enum Color
    {
        Green,
        Papaya,
        Lightgray,
        Tomato,
        Lightskyblue,
        Gold,
        Yellowgreen,
        Dodgerblue3,
        Skyblue3,
        Goldenrod1,
        Seagreen,
        Black,
        White,
        Gray75
    }

    public static class GraphVizHelper
    {
        private static char[] badChars = new char[] { '"', '<', '>', '{', '}'}; //simple example

        private static ImmutableDictionary<Id, Id> emptyDictionary = new Dictionary<Id, Id>().ToImmutableDictionary();

        private static void AddGeneralStatements(ref List<Statement> statements)
        {
            // Graph orientation: left to right
            var generalStyleSettings = new Dictionary<Id, Id>();
            generalStyleSettings.Add(new Id("rankdir"), new Id("LR"));
            var generalStyleAttributes = new AttributeStatement(AttributeKinds.Graph, generalStyleSettings.ToImmutableDictionary());
            statements.Add(generalStyleAttributes);

            // We draw filled rectangles
            var generalNodeStyleSettings = new Dictionary<Id, Id>();
            generalNodeStyleSettings.Add(new Id("shape"), new Id("record"));
            generalNodeStyleSettings.Add(new Id("style"), new Id("filled"));
            generalNodeStyleSettings.Add(new Id("fontsize"), new Id("11"));
            generalNodeStyleSettings.Add(new Id("color"), new Id("gray75"));
            //generalNodeStyleSettings.Add(new Id("fontname"), new Id("Monospace"));
            var generalNodeStyleAttributes = new AttributeStatement(AttributeKinds.Node, generalNodeStyleSettings.ToImmutableDictionary());

            statements.Add(generalNodeStyleAttributes);
        }

        private static void AddEdgeStatement(ref List<Statement> statements, int origin, int destination)
        {
            var edge = new EdgeStatement(new NodeId(Convert.ToString(origin)),
                                         new NodeId(Convert.ToString(destination)),
                                         emptyDictionary);

            statements.Add(edge);
        }

        /*
digraph finite_state_machine {
	node [shape=Mrecord, style=filled, fontsize = 11];
    struct1 [label="<f0> left|<f1> mid&#92; dle|<f2> right"];
    struct2 [label="<f0> one|<f1> two"];
    struct3 [label="hello&#92;nworld |{ b |{c|<here> d|e}| f}| g | h"];
    struct4 [label="Uno|Dos Tres", fontsize = 10];
    struct5 [color="0.408 0.888 1.000", fillcolor="0.333 0.444 1.000" label="{Cinco|Seis Siete|Ocho}"];
    struct6 [color="navy", fillcolor="lightskyblue" label="{Cinco|Seis Siete|Ocho}"];
    struct7 [color="darkgreen", fillcolor="yellowgreen" label="{Cinco|Seis Siete|Ocho}"];
    struct8 [color="black", fillcolor="lightcyan" label="{Cinco|Seis Siete|Ocho}"];
    struct9 [color="black", fillcolor="beige" label="{Cinco|Seis Siete|Ocho}"];
    struct10 [color="black", fillcolor="azure" label="{Cinco|Seis Siete|Ocho}"];
    struct11 [color="black", fillcolor="papayawhip" label="{Cinco|Seis Siete|Ocho}"];
    struct1:f1 -> struct2:f0;
    struct1:f2 -> struct3:here;
    struct12 [label="struct12 |{ [CheckPoint] OSAL 2 - Date/Time and Reboot/Shutdown and USB\n management And Network management | Sprint 89 }"];
    struct13 [label="{struct13 |[CheckPoint] OSAL 2 - Date/Time and Reboot/Shutdown and USB\n management And Network management } |Sprint 89"];
    struct14 [label="{struct14 |{[CheckPoint] OSAL 2 - Date/Time and Reboot/Shutdown and USB\n management And Network management| Sprint 89 } }"];
    struct12 -> struct13;
    struct12 -> struct14;
}

            https://graphviz.gitlab.io/_pages/doc/info/colors.html
            https://graphviz.gitlab.io/_pages/doc/info/attrs.html#k:color
         */
        private static string SplitInLines(string title, ushort maxLineLength)
        {
            if (title != null)
            {
                var graphVizNewline = @"\n";
                var titleLength = title.Length;

                switch (Math.Ceiling((double)titleLength / maxLineLength))
                {
                    case 1:
                        break;
                    case 2:
                        title = title.Insert(maxLineLength, graphVizNewline);
                        break;
                    default:
                        title = title.Insert(maxLineLength, graphVizNewline).Insert((maxLineLength * 2) + 1, graphVizNewline);
                        if (title.Length > (maxLineLength * 3) + 2)
                        {
                            title = title.Substring(0, (maxLineLength * 3) - 2) + @"...";
                        }
                        break;

                }
                return title;
            }

            return null;
        }

        private static void DefineNode(ref List<Statement> statements, int nodeId, string title, string comment, Color nodeColor, Color textColor, bool outerBold, ushort maxLineLength)
        {
            Dictionary<Id, Id> nodeStyleSettings = null;
            Dictionary<Id, Id> nodeLabelSettings = null;

            nodeStyleSettings = new Dictionary<Id, Id>();
            //nodeStyleSettings.Add(new Id("color"), new Id("black"));

            nodeStyleSettings.Add(new Id("fillcolor"), new Id(Enum.GetName(typeof(Color), nodeColor)));

            nodeStyleSettings.Add(new Id("fontcolor"), new Id(Enum.GetName(typeof(Color), textColor)));

            if (outerBold)
            {
                nodeStyleSettings.Add(new Id("color"), new Id("red"));
                nodeStyleSettings.Add(new Id("penwidth"), new Id("2"));
            }

            nodeLabelSettings = new Dictionary<Id, Id>();


            title = string.Concat(title.Split(badChars, StringSplitOptions.RemoveEmptyEntries));
            if (string.IsNullOrWhiteSpace(comment)) {
                nodeStyleSettings.Add(new Id("label"), new Id("{ " + Convert.ToString(nodeId) + " | " + GraphVizHelper.SplitInLines(title, maxLineLength) + "}"));
            } else
            {
                //nodeStyleSettings.Add(new Id("label"), new Id(Convert.ToString(nodeId) + " | { " + GraphVizHelper.SplitInLines(title, maxLineLength) + " | " + comment + " } "));
                nodeStyleSettings.Add(new Id("label"), new Id("{" + Convert.ToString(nodeId) + "| { " + GraphVizHelper.SplitInLines(title, maxLineLength) + " | " + comment + " } }"));

                //{ struct14 |{[CheckPoint] OSAL 2 - Date/Time and Reboot/Shutdown and USB\n management And Network management| Sprint 89 }}
                //struct12 |{ [CheckPoint] OSAL 2 - Date/Time and Reboot/Shutdown and USB\n management And Network management | Sprint 89 }
            }
            // nodeStyleSettings.Add(new Id("label"), new Id(@"two\nlines\nMore long lines\ncheck how this looks like")); FUNCIONA...
            //nodeStyleSettings.Add(new Id("label"), new Id("{<<TABLE>< TR >< TD > AAAA </ TD ></ TR >< TR >< TD > caption </ TD ></ TR ></ TABLE >>}")); NO FUNCIONA

            /*
             <<TABLE>
    <TR><TD>AAAA</TD></TR>
    <TR><TD>caption</TD></TR>
    </TABLE>>
             */

            var node = new NodeStatement(new Id(Convert.ToString(nodeId)), nodeStyleSettings.ToImmutableDictionary());
            statements.Add(node);
        }

        private static void SetNodeAndTextColors(string state, out Color nodeColor, out Color textColor)
        {
            nodeColor = Color.Lightgray;
            textColor = Color.Black;

            if (!string.IsNullOrWhiteSpace(state))
            {
                state = state.ToUpper().Trim();

                switch (state)
                {
                    case "NEW":
                        nodeColor = Color.Dodgerblue3;
                        textColor = Color.White;
                        break;
                    case "APPROVED":
                        nodeColor = Color.Skyblue3;
                        break;
                    case "COMMITTED":
                    case "IN PROGRESS":
                        nodeColor = Color.Goldenrod1;
                        break;
                    case "DONE":
                        nodeColor = Color.Seagreen;
                        textColor = Color.White;
                        break;
                    default:
                        nodeColor = Color.Lightgray;
                        break;
                }
            }
        }

        public static Graph CreateDependencyGraph(Dictionary<int, DependencyItem> model, ushort maxLineLength)
        {
            try
            {
                var statements = new List<Statement>();

                AddGeneralStatements(ref statements);

                foreach (KeyValuePair<int, DependencyItem> entry in model)
                {
                    if (entry.Value.Successors.Any())
                    {
                        foreach (var succesor in entry.Value.Successors)
                        {
                            //AddEdgeStatement(ref statements, "{Hola | " + entry.Value.ToString() + "}", model[succesor].ToString());
                            AddEdgeStatement(ref statements, entry.Value.Id, model[succesor].Id);
                        }
                    }

                    Color nodeColor, textColor;
                    SetNodeAndTextColors(entry.Value.State, out nodeColor, out textColor);

                    var outerBold = false;
                    if (entry.Value.Tags.Any())
                    {
                        if (entry.Value.Tags.Any(str => str.Contains("External")))
                        {
                            outerBold = true;
                        } 
                    }

                    DefineNode(ref statements, entry.Value.Id, entry.Value.Title, entry.Value.Comment, nodeColor, textColor, outerBold, maxLineLength);

                    //else
                    //{
                    //    DefineNode(ref statements, entry.Value.Id, entry.Value.ReducedTitle, Color.Papaya, false);
                    //}
                }

                return new Graph(GraphKinds.Directed, "Name", statements.ToImmutableList());
            }
            catch (Exception ex)
            {
                // ToDo: Add message with error
                // ToDo: Add Logger!
                throw;
            }
        }

        public static void TryGraphvizPath(string path)
        {
            IRenderer renderer = new Renderer(path);

            using (MemoryStream memStream = new MemoryStream())
            {
                var statements = new List<Statement>();
                var graph = new Graph(GraphKinds.Directed, "Test", statements.ToImmutableList());

                Task.Run(async () =>
                {
                    await renderer.RunAsync(graph,
                        memStream,
                        RendererLayouts.Dot,
                        RendererFormats.Png,
                        CancellationToken.None);
                }).Wait();
            }
        }

    }
}
