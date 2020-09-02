# Queue trigger in .NET Core to create new nodes in the graph database

This sample code explains how to track the new profile items in the queue storage and save them in the Cosmos DB using Gremlin API. Code is using .NET Core 3.1 on Visual Studio 2019 and use Gremlin API on Azure Cosmos DB

## Technology stack
* .NET Core 3.1 on Visual Studio 2019
* Queue storage is used on Azure storage account
* Azure functions v3 (Microsoft.NET.SDK.Functions - 3.0.9) 
* Gremlin.Net used to connect to the Azure Cosmos DB Gremlin API  

## How to run the solution
 * Create a storage account and create a queue inside it, Go to the Access keys section and get the connection string and provide it to the storage-connection setting
 * You have to create a Cosmos DB account with Gremlin (Graph) API then go to the Keys section, get the Gremlin endpoint and key to connect to the database
 * Create a database and graph inside the Cosmos DB account, use the same values for the settings
 * Open the solution file in Visual Studio and run the project
 * Insert a new item in the queue and check it is inserted to the graph database
 
 ## Code snippets
 ### Run method in Queue trigger
 ```
 [FunctionName("CreateTreeStructure")]
 public static void Run([QueueTrigger("people-queue", Connection = "storage-connection")]string item,
   ILogger log)
 {
    log.LogInformation($"C# Queue trigger function processed: {item}");
    var person = JsonConvert.DeserializeObject<Person>(item);

    GraphData graphData = new GraphData();

    graphData.AddPerson(person, log);

   }
 ```
### Generate the graph query
 ```
 public void AddPerson(Person person, ILogger log)
 {
    var gremlinServer = InitiateGraph();
            
    string query = "g.addV('" + container  +"').property('id', '" + person.Id  + "')
        .property('name', '" + person.Name + "').property('city', '" + person.City  + "')";
    ExecuteQuery(gremlinServer, query);
    
    if (person.Connections != null)
    {
        foreach (Connection connection in person.Connections)
        {
            query = "g.V('" + person.Id + "').addE('" + connection.Relationship + "').to(g.V('" + connection.RelatedPerson 
                + "'))";
            ExecuteQuery(gremlinServer, query);
        }
    }
}
        ```

