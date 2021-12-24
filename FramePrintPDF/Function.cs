using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FramePrintPDF
{
    public class LambdaRequest
    {
        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }
    }

    public class LambdaResponse
    {
        [JsonProperty(PropertyName = "statusCode")]
        public HttpStatusCode StatusCode { get; set; }

        [JsonProperty(PropertyName = "headers")]
        public Dictionary<string, string> Headers { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }
    }

    public class Function
    {
        
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public LambdaResponse FunctionHandler(LambdaRequest input, ILambdaContext context)
        {
            // データの読み込み
            var myPrintInput = new PrintInput(input.Body);
            string message = myPrintInput.getPdfSource();

            // 結果データを書きだす(json形式) -------------------------------------------------------
            return new LambdaResponse
            {
                StatusCode = HttpStatusCode.OK,
                Headers = new Dictionary<string, string>() {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" }
                },
                Body = message
            };
        }
    }
}
