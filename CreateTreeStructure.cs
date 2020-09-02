using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using queueitems.Model;

namespace queuetrigger
{
    public static class CreateTreeStructure
    {

       
        [FunctionName("CreateTreeStructure")]
        public static void Run([QueueTrigger("people-queue", Connection = "storage-connection")]string item, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {item}");
            var person = JsonConvert.DeserializeObject<Person>(item);

            GraphData graphData = new GraphData();

            //graphData.ReadPerson(log);
            graphData.AddPerson(person, log);

        }
    }
}
