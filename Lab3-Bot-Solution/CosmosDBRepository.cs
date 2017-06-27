namespace Lab3BotSolution
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Microsoft.Azure.Graphs;
    using Newtonsoft.Json;
    using Lab3BotSolution.Models;

    public static class CosmosDBRepository
    {
        private static readonly string DatabaseId = ConfigurationManager.AppSettings["database"];
        private static readonly string CollectionId = ConfigurationManager.AppSettings["collection"];
        private static DocumentClient client;
        private static DocumentCollection graph;

        public static void Initialize()
        {
            client = new DocumentClient(new Uri(ConfigurationManager.AppSettings["endpoint"]), ConfigurationManager.AppSettings["authKey"], new ConnectionPolicy { EnableEndpointDiscovery = false });
            ConnectToGraphCollectionAsync().Wait();
        }

        private static async Task ConnectToGraphCollectionAsync()
        {
            try
            {
                graph = await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
            }
            catch 
            {
                throw;
            }
        }


        public static async Task<int> CountNodes(string label = null)
        {
            string graphQuery = "g.V()";
            if (!String.IsNullOrEmpty(label))
            {
                graphQuery += String.Format(".hasLabel('{0}')", label);
            }

            graphQuery += ".count()";

            IDocumentQuery<string> query = client.CreateGremlinQuery<string>(graph, graphQuery);
            
            var val = (await query.ExecuteNextAsync()).GetEnumerator();
            while (val.MoveNext())
            {
                return (int)val.Current;
            }
            return 0;
         }

        public static async Task<string> GetTop1Person(bool isTop)
        {
            var graphQuery = "g.E().hasLabel('mentionedInChapter').order().by('mentions', ";
            graphQuery += (isTop) ? "decr" : "incr";
            graphQuery += ").outV().values('lastName').limit(1)";

            IDocumentQuery<string> query = client.CreateGremlinQuery<string>(graph, graphQuery);

            var val = (await query.ExecuteNextAsync()).GetEnumerator();
            string node = null;
            while (val.MoveNext())
            {
                node = (string)val.Current;
            }

            return node;
        }
        public static async Task<string> GetNodesPage(string label, int pageNum)
        {
            int start = 1 + (10 * pageNum);
            int stop = 10 * (pageNum + 1);
           
            string graphQuery = "g.V()";
            if (!String.IsNullOrEmpty(label))
            {
                graphQuery += String.Format(".hasLabel('{0}')", label);
            }

            graphQuery += String.Format(".order().by('lastName', incr).values('lastName').range({0}, {1})", start, stop);

            IDocumentQuery<string> query = client.CreateGremlinQuery<string>(graph, graphQuery);

            var val = (await query.ExecuteNextAsync()).GetEnumerator();
            List<string> nodes = new List<string>();
            while (val.MoveNext())
            {
                nodes.Add((string)val.Current);
            }

            string str = String.Empty; 
            foreach (var item in nodes)
            {
                str += String.Format("{0} {1}{2}", "* ", item, Environment.NewLine);
            }
           
            return str;
        }

       
    }
}