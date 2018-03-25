using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DependenciesVisualizer.Model;
using Shields.GraphViz.Models;

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
        Yellowgreen
    }

    public static class GraphVizHelper
    {
        private static ImmutableDictionary<Id, Id> emptyDictionary = new Dictionary<Id, Id>().ToImmutableDictionary();

        public static void AddGeneralStatements(ref List<Statement> statements)
        {
            // Graph orientation: left to right
            var generalStyleSettings = new Dictionary<Id, Id>();
            generalStyleSettings.Add(new Id("rankdir"), new Id("LR"));
            var generalStyleAttributes = new AttributeStatement(AttributeKinds.Graph, generalStyleSettings.ToImmutableDictionary());
            statements.Add(generalStyleAttributes);

            // We draw filled rectangles
            var generalNodeStyleSettings = new Dictionary<Id, Id>();
            generalNodeStyleSettings.Add(new Id("shape"), new Id("Mrecord"));
            generalNodeStyleSettings.Add(new Id("style"), new Id("filled"));
            generalNodeStyleSettings.Add(new Id("fontsize"), new Id("11"));
            generalNodeStyleSettings.Add(new Id("color"), new Id("black"));
            //generalNodeStyleSettings.Add(new Id("fontname"), new Id("Monospace"));
            var generalNodeStyleAttributes = new AttributeStatement(AttributeKinds.Node, generalNodeStyleSettings.ToImmutableDictionary());

            statements.Add(generalNodeStyleAttributes);
        }

        public static void AddEdgeStatement(ref List<Statement> statements, int origin, int destination)
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
}

            https://graphviz.gitlab.io/_pages/doc/info/colors.html
            https://graphviz.gitlab.io/_pages/doc/info/attrs.html#k:color
         */

        public static void DefineNode(ref List<Statement> statements, int nodeId, string title, Color color, bool outerBold)
        {
            Dictionary<Id, Id> nodeStyleSettings = null;
            Dictionary<Id, Id> nodeLabelSettings = null;

            nodeStyleSettings = new Dictionary<Id, Id>();
            //nodeStyleSettings.Add(new Id("color"), new Id("black"));

            switch (color)
            {
                case Color.Lightskyblue:
                    nodeStyleSettings.Add(new Id("fillcolor"), new Id("lightskyblue"));
                    break;
                case Color.Papaya:
                    nodeStyleSettings.Add(new Id("fillcolor"), new Id("papayawhip"));
                    break;
                case Color.Lightgray:
                    nodeStyleSettings.Add(new Id("fillcolor"), new Id("lightgrey"));
                    break;
                case Color.Tomato:
                    nodeStyleSettings.Add(new Id("fillcolor"), new Id("tomato"));
                    break;
                case Color.Gold:
                    nodeStyleSettings.Add(new Id("fillcolor"), new Id("gold"));
                    break;
                case Color.Yellowgreen:
                    nodeStyleSettings.Add(new Id("fillcolor"), new Id("yellowgreen"));
                    break;
            }

            if (outerBold)
            {
                nodeStyleSettings.Add(new Id("color"), new Id("red"));
                nodeStyleSettings.Add(new Id("penwidth"), new Id("2"));
            }

            nodeLabelSettings = new Dictionary<Id, Id>();
            nodeStyleSettings.Add(new Id("label"), new Id("{ " + Convert.ToString(nodeId) + " | " + title + "}"));

            var node = new NodeStatement(new Id(Convert.ToString(nodeId)), nodeStyleSettings.ToImmutableDictionary());
            statements.Add(node);
        }

        public static Color GetColorByState(string state)
        {
            // Done yellowgreen
            // Committed Gold
            // In Progress Tomato
            // New lightskyblue

            Color ret = Color.Lightgray;

            if (!string.IsNullOrWhiteSpace(state))
            {
                state = state.ToUpper().Trim();

                switch (state)
                {
                    case "NEW":
                    case "APPROVED":
                        ret = Color.Lightskyblue;
                        break;
                    case "COMMITTED":
                        ret = Color.Gold;
                        break;
                    case "IN PROGRESS":
                        ret = Color.Tomato;
                        break;
                    case "DONE":
                        ret = Color.Yellowgreen;
                        break;
                    default:
                        ret = Color.Lightgray;
                        break;
                }
            }

            return ret;
        }

        public static Graph CreateDependencyGraph(Dictionary<int, DependencyItem> model)
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

                    Color color = GetColorByState(entry.Value.State);

                    var outerBold = false;
                    if (entry.Value.Tags.Any())
                    {
                        if (entry.Value.Tags.Any(str => str.Contains("External")))
                        {
                            outerBold = true;
                        } 
                    }

                    DefineNode(ref statements, entry.Value.Id, entry.Value.ReducedTitle, color, outerBold);

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

    }
}
