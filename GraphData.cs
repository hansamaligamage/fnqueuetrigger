using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;
using Gremlin.Net.Structure.IO.GraphSON;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using queueitems.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace queuetrigger
{
    class GraphData
    {
        private static string host = Environment.GetEnvironmentVariable("host");
        private static string key = Environment.GetEnvironmentVariable("key");
        private static string database = Environment.GetEnvironmentVariable("database");
        private static string container = Environment.GetEnvironmentVariable("container");
        private static int port = 443;

        public void ReadPerson(ILogger log)
        {

            var gremlinServer = InitiateGraph();

            string query = "g.V()";
            ExecuteQuery(gremlinServer, query);

        }


        public void AddPerson(Person person, ILogger log)
        {
            var gremlinServer = InitiateGraph();
            
            string query = "g.addV('" + container  +"').property('id', '" + person.Id  + "').property('name', '" + person.Name  
                + "').property('city', '" + person.City  + "')";
            ExecuteQuery(gremlinServer, query);
            if (person.Connections != null)
            {
                foreach (Connection connection in person.Connections)
                {
                    query = "g.V('" + person.Id + "').addE('" + connection.Relationship + "').to(g.V('" + connection.RelatedPerson + "'))";
                    ExecuteQuery(gremlinServer, query);
                }
            }
        }

        #region Private Methods

        private void ExecuteQuery (GremlinServer gremlinServer, string query)
        {
            using (var client = new GremlinClient(gremlinServer, new GraphSON3Reader(), new GraphSON3Writer(), 
                GremlinClient.GraphSON2MimeType))
            {
                // Create async task to execute the Gremlin query.
                var resultSet = SubmitRequest(client, query).Result;
                if (resultSet.Count > 0)
                {
                    Console.WriteLine("\tResult:");
                    foreach (var result in resultSet)
                    {
                        // The vertex results are formed as Dictionaries with a nested dictionary for their properties
                        string output = JsonConvert.SerializeObject(result);
                        Console.WriteLine($"\t{output}");
                    }
                    Console.WriteLine();
                }

                // Print the status attributes for the result set.
                // This includes the following:
                //  x-ms-status-code            : This is the sub-status code which is specific to Cosmos DB.
                //  x-ms-total-request-charge   : The total request units charged for processing a request.
                //  x-ms-total-server-time-ms   : The total time executing processing the request on the server.
                //PrintStatusAttributes(resultSet.StatusAttributes);
                Console.WriteLine();
            }
        }

        private GremlinServer InitiateGraph ()
        {
            string containerLink = "/dbs/" + database + "/colls/" + container;
            var gremlinServer = new GremlinServer(hostname: host, port: port, enableSsl: true, username: containerLink, password: key);
            return gremlinServer;
        }

        private Task<ResultSet<dynamic>> SubmitRequest(GremlinClient gremlinClient, string query)
        {
            try
            {
                return gremlinClient.SubmitAsync<dynamic>(query);
            }
            catch (ResponseException e)
            {
                Console.WriteLine("\tRequest Error!");

                // Print the Gremlin status code.
                Console.WriteLine($"\tStatusCode: {e.StatusCode}");

                // On error, ResponseException.StatusAttributes will include the common StatusAttributes for successful requests, as well as
                // additional attributes for retry handling and diagnostics.
                // These include:
                //  x-ms-retry-after-ms         : The number of milliseconds to wait to retry the operation after an initial operation was throttled. This will be populated when
                //                              : attribute 'x-ms-status-code' returns 429.
                //  x-ms-activity-id            : Represents a unique identifier for the operation. Commonly used for troubleshooting purposes.
                PrintStatusAttributes(e.StatusAttributes);
                Console.WriteLine($"\t[\"x-ms-retry-after-ms\"] : { GetValueAsString(e.StatusAttributes, "x-ms-retry-after-ms")}");
                Console.WriteLine($"\t[\"x-ms-activity-id\"] : { GetValueAsString(e.StatusAttributes, "x-ms-activity-id")}");

                throw;
            }
        }

        private void PrintStatusAttributes(IReadOnlyDictionary<string, object> attributes)
        {
            Console.WriteLine($"\tStatusAttributes:");
            Console.WriteLine($"\t[\"x-ms-status-code\"] : { GetValueAsString(attributes, "x-ms-status-code")}");
            Console.WriteLine($"\t[\"x-ms-total-server-time-ms\"] : { GetValueAsString(attributes, "x-ms-total-server-time-ms")}");
            Console.WriteLine($"\t[\"x-ms-total-request-charge\"] : { GetValueAsString(attributes, "x-ms-total-request-charge")}");
        }

        private string GetValueAsString(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            return JsonConvert.SerializeObject(GetValueOrDefault(dictionary, key));
        }

        private object GetValueOrDefault(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }

            return null;
        }

        #endregion
    }
}
