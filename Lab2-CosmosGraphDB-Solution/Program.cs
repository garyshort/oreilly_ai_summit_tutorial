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
using System.IO;
using System.Text.RegularExpressions;
using CosmosDBGraphSample;

namespace Lab2CosmosGraphDBSolution
{
    class Program
    {
        private static DocumentClient client;
        private static DocumentCollection graph;
        private static readonly string DatabaseId = 
            ConfigurationManager.AppSettings["database"];
        private static readonly string CollectionId = 
            ConfigurationManager.AppSettings["collection"];

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting ... ");

            Console.WriteLine("--- Initialize Cosmos DB database graph " + 
                "collection ---");

            Console.WriteLine("Connecting to CosmosDB endpoint");

            client = new DocumentClient(
                new Uri(ConfigurationManager.AppSettings["endpoint"]), 
                ConfigurationManager.AppSettings["authKey"], 
                new ConnectionPolicy { EnableEndpointDiscovery = false });

            Console.WriteLine("Connecting (or creating one) to a CosmosDB " +
                "database: {0}", DatabaseId);

            CreateDatabaseIfNotExistsAsync().Wait();
            Console.WriteLine("Connecting (or creating one) to CosmosDB " +
                "database '{0}' graph collection: {1}", DatabaseId, CollectionId);

            CreateCollectionIfNotExistsAsync().Wait();

            Console.WriteLine("--- Step 1: Add data into graph collection ---");

            Console.WriteLine("  Run Gremlin using CreateGremlinQuery");
            AddDataAsync().Wait();

            Console.WriteLine("--- Step 2: Query graph collection using Gremlin---");

            // <--- A D D  Y O U R  C O D E  H E R E  --->

            Console.WriteLine("--- Press any key to exit --- ");
            Console.ReadKey();
        }

        private static async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await client.ReadDatabaseAsync(
                    UriFactory.CreateDatabaseUri(DatabaseId));

                Console.WriteLine(
                    "  Connected to CosmosDB database '{0}'", 
                    DatabaseId);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(
                        new Database { Id = DatabaseId });

                    Console.WriteLine(
                        "  New CosmosDB database '{0}' was created", 
                        DatabaseId);
                }
                else
                {
                    throw;
                }
            }
        }
        private static async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                graph = await client.ReadDocumentCollectionAsync(
                    UriFactory.CreateDocumentCollectionUri(
                        DatabaseId, CollectionId));

                Console.WriteLine("  Connected to CosmosDB '{0}' database graph" +
                    " collection '{1}'", DatabaseId, CollectionId);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    graph = await client.CreateDocumentCollectionIfNotExistsAsync(
                        UriFactory.CreateDatabaseUri(DatabaseId),
                        new DocumentCollection { Id = CollectionId },
                        new RequestOptions { OfferThroughput = 1000 });

                    Console.WriteLine("  Created a new graph collection '{1}' " +
                        "for CosmosDB database'{1}'", DatabaseId, CollectionId);
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task AddDataAsync()
        {
            // Start with a clean db
            await DropDatabase();
            Console.WriteLine("Database dropped");

            // Read in the marked up file
            string text = File.ReadAllText(
                @"C:\Users\gashort\Documents\Presentations\AISummitTutorial\" +
                @"Lab2-CosmosGraphDB-Solution\AnnaKareninaMarkedUp.txt");

            // Split the text by chapter
            string[] chapters = text.Split(
                new string[] { "Chapter/O" }, 
                StringSplitOptions.RemoveEmptyEntries);

            // Take Part 1 of the book (34 Chapters)
            string[] targetChapters = chapters.Take(34).ToArray();

            // Iterate over the chapters...
            for (int i = 0; i < targetChapters.Length; i++)
            {
                // Grab the current chapter
                string chapter = targetChapters[i];

                // Clean the chapter of special chars
                chapter = chapter.Replace("\r", " ");
                chapter = chapter.Replace("\n", " ");

                // Create a node for this chapter
                AddChapterAsync(i + 1).Wait();

                // Grab each of the word/entity pairs
                string[] wep = chapter.Split(
                    " ".ToArray(), 
                    StringSplitOptions.RemoveEmptyEntries);

                // Iteraterate over the word/entity pairs
                for (int j = 0; j < wep.Length; j++)
                {
                    // Grab the current tuple
                    string tuple = wep[j];

                    // Separate each tuple into the word and the entity
                    string[] we = tuple.Split(
                        "/".ToCharArray(), 
                        StringSplitOptions.RemoveEmptyEntries);

                    string word = we[0];
                    string entity = we[1];

                    // Peek ahead until the entity changes.
                    // Concatenate the words of equal entities
                    for (int k = j + 1; k < wep.Length; k++)
                    {
                        // Grab the next tuple
                        string nextTuple = wep[k];

                        // Separate each tuple into the word and the entity
                        string[] nextWE = nextTuple.Split(
                            "/".ToCharArray(), 
                            StringSplitOptions.RemoveEmptyEntries);

                        string nextWord = nextWE[0];
                        string nextEntity = nextWE[1];

                        if (entity == nextEntity)
                        {
                            word += " " + nextWord;
                            continue;
                        }
                        else
                        {
                            j = k;
                            break;
                        }
                    }

                    // Handle each entity
                    switch (entity)
                    {
                        case "PERSON":
                            AddPersonAsync(
                                word, 
                                "chapter" + (i + 1).ToString()).Wait();
                            break;

                            // <--- A D D  Y O U R  C O D E  H E R E  --->
                    }

                }
            }

            
        }

        private static async Task DropDatabase()
        {
            await ExecuteQueryAsync("g.V().drop()");
        }

        private static async Task ExecuteQueryAsync(string queryString)
        {
            IDocumentQuery<dynamic> query =
                client.CreateGremlinQuery<dynamic>(graph, queryString);

            while (query.HasMoreResults)
            {
                foreach (dynamic result in await query.ExecuteNextAsync())
                {
                    Console.WriteLine(
                        $"\t {JsonConvert.SerializeObject(result)}");
                }
            }
        }

        private static async Task AddChapterAsync(int chapterCount)
        {
            string queryString = string.Format(
                "g.addV('Chapter').property('id', 'chapter{0}')", 
                chapterCount);

            await ExecuteQueryAsync(queryString);

        }

        private static async Task AddPersonAsync(
            string fullName, 
            string chapterId)
        {
            string id = fullName.Split().Last().ToLower();
            string lastName = fullName.Split().Last();
            string firstName = fullName.Split().First();

            // Does this person exist?
            var result = 
                await ExecuteQueryReturnResultAsync(
                    string.Format("g.V('{0}')", id));

            if (result.Count == 0)
            {
                // person doesn't exist so add it
                string q = 
                    string.Format(
                        "g.addV('Person').property('id', '{0}')" +
                        ".property('firstName', '{1}')" +
                        ".property('lastName', '{2}')", 
                        id, 
                        firstName, 
                        lastName);

                await ExecuteQueryAsync(q);
            }

            // If an edge already exists been this person and this chapter then
            // increment the mentionsCount otherwise add the edge
            string edgeQuery = 
                string.Format(
                    "g.V('{0}').outE('mentionedInChapter').where(inV()" +
                    ".has('id', '{1}'))", 
                    id, 
                    chapterId);

            FeedResponse<dynamic> matchingEdges = 
                await ExecuteQueryReturnResultAsync(edgeQuery);

            if (matchingEdges.Count > 0)
            {
                // Matching edge exists, update the mentions count
                Edge edge = 
                    JsonConvert.DeserializeObject<Edge>(
                        matchingEdges
                        .First()
                        .ToString());

                int mentions = edge.properties.mentions+1;
                string q = string.Format(
                    "g.E('{0}').property('mentions', {1})", 
                    edge.id, 
                    mentions);

                await ExecuteQueryAsync(q);
            }
            else
            {
                // Edge doesn't exist so add one and set number of mentions to 1
                string q2 = 
                    string.Format(
                        "g.V('{0}').addE('mentionedInChapter').to(g.V('{1}'))" +
                        ".property('mentions', 1)", 
                        id, 
                        chapterId);

                await ExecuteQueryAsync(q2);
            }


        }

        private static async Task<FeedResponse<dynamic>> 
            ExecuteQueryReturnResultAsync(string queryString)
        {
            IDocumentQuery<string> query =
                client.CreateGremlinQuery<string>(
                    graph,
                    queryString);
            return await query.ExecuteNextAsync();
        }
    }
}
