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
        public string body { get; set; }
    }

    public class LambdaResponse
    {
        [JsonProperty(PropertyName = "statusCode")]
        public HttpStatusCode statusCode { get; set; }

        [JsonProperty(PropertyName = "headers")]
        public Dictionary<string, string> headers { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string body { get; set; }
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
            try
            {
                // データの読み込み
                var myPrintInput = new PrintInput(input.body);
                string base64str = myPrintInput.getPdfSource();

                // 結果データを書きだす(json形式) -------------------------------------------------------
                return new LambdaResponse
                {
                    statusCode = HttpStatusCode.OK,
                    headers = new Dictionary<string, string>() {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" }
                },
                    body = base64str,
            };
            }
            catch (Exception ex)
            {
                return new LambdaResponse
                {
                    statusCode = HttpStatusCode.InternalServerError,
                    headers = new Dictionary<string, string>(){
                        { "Content-Type", "application/json" },
                        { "Access-Control-Allow-Origin", "*" }
                    },
                    body = "{\"error\":\"" + ex.Message + "\"}"
                };
            }

        }
    }
}
