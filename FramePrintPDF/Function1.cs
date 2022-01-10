using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FramePrintPDF
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string testStr = req.Query["test"];
            if(testStr != null)
            {
                string responseMessage = string.IsNullOrEmpty(testStr)
                    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                    : $"Hello, {testStr}. This HTTP triggered function executed successfully.";

                return new OkObjectResult(responseMessage);
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var myPrintInput = new PrintInput(requestBody);
            string base64str = myPrintInput.getPdfSource();

            return new OkObjectResult(base64str);
        }
    }
}
