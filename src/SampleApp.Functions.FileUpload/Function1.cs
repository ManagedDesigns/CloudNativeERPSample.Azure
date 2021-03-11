using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using SampleApp.Services.OCR;

namespace SampleApp.Functions.FileUpload
{
    public static class Upload
    {
        [FunctionName("Upload")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request for file upload");

            try
            {
                await UploadFileToBlobStorage(req, log);
                await RegisterOutgoingInvoice(req, log);
                return new OkResult();
            }
            catch (Exception e)
            {
                log.LogInformation($"An exception of type {e.GetType().Name} occurred\nMessage: {e.Message}");
                return new BadRequestResult();
            }
        }

        private static async Task UploadFileToBlobStorage(HttpRequest req, ILogger log)
        {
            string connectionString = "";
            string containerName = "invoices";
            var fileName = req.Form.Files[0].FileName;
            var container = new BlobContainerClient(connectionString, containerName);
            var blob = container.GetBlobClient(fileName);
            var blobHttpHeader = new BlobHttpHeaders();
            blobHttpHeader.ContentType = "application/pdf";

            await blob.UploadAsync(req.Body, blobHttpHeader);

            log.LogInformation($"File {fileName} has been uploaded");
        }

        private static async Task RegisterOutgoingInvoice(HttpRequest req, ILogger log)
        {
            var subscriptionKey = "";
            var endpoint = "";
            var trainingUrl = "";

            log.LogInformation($"Starting OCR");
            var recognizer = new Recognizer(subscriptionKey, endpoint, trainingUrl);
            var result = await recognizer.RecognizeForm(req.Body);
            log.LogInformation($"OCR completed");
        }
    }
}
