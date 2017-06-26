using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDBGraphSample
{
    public class Properties
    {
        public int mentions { get; set; }
    }

    public class Edge
    {
        public string id { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public string inVLabel { get; set; }
        public string outVLabel { get; set; }
        public string inV { get; set; }
        public string outV { get; set; }
        public Properties properties { get; set; }
    }
}
