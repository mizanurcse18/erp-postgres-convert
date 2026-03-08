using System;
using System.Collections.Generic;
using System.Text;

namespace Manager.Core.CommonDto
{
    public class Widget
    {
        public string id { get; set; }
        public string title { get; set; }
        public Table table { get; set; }
        public Dictionary<string, string> ranges { get; set; }
        public string currentRange { set; get; }
        public MainChart mainChart { get; set; }
        public List<Footer> footer { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        //public DateTime FromDate { get; set; }
        //public DateTime ToDate { get; set; }

    }
    public class Table
    {
        public List<Columns> columns { get; set; }
        public List<Rows> rows { get; set; }
    }

    public class Columns
    {
        public string id { get; set; }
        public string title { get; set; }
    }

    public class Rows
    {
        public string id { get; set; }
        public List<Cells> cells { get; set; }
    }
    public class Cells
    {
        public string id { get; set; }
        public string value { get; set; }
        public string classes { get; set; }
        public string icon { get; set; }
    }

    public class MainChart
    {
        public string[] labels { get; set; }
        public Dictionary<string, List<MainChartDatasets>> datasets { get; set; }
        public MainChartOptions options { get; set; }
    }

    public class MainChartDatasets
    {
        public int[] data { get; set; }
        public string[] backgroundColor { get; set; }
        public string[] hoverBackgroundColor { get; set; }
    }
    public class MainChartDatasetObject
    {
        public int[] data { get; set; }
        public string[] backgroundColor { get; set; }
        public string[] hoverBackgroundColor { get; set; }
    }

    public class MainChartOptions
    {
        public int cutoutPercentage { get; set; }
        public bool spanGaps { get; set; }
        public MainChartOptionsLegend legend { get; set; }
        public bool maintainAspectRatio { get; set; }

    }

    public class MainChartOptionsLegend
    {
        public bool display { get; set; }
        public string position { get; set; }
        public MainChartOptionsLegendLabel labels { get; set; }
    }
    public class MainChartOptionsLegendLabel
    {
        public int padding { get; set; }
        public bool usePointStyle { get; set; }
    }
    public class Footer
    {
        public string title { get; set; }
        public Dictionary<string, string> count { get; set; }
    }
}
