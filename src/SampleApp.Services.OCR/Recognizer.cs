using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace SampleApp.Services.OCR
{
    public class Recognizer
    {
        public string SubscriptionKey { get; private set; }
        public string Endpoint { get; private set; }

        //public Guid ModelId { get; private set; }
        //public string TrainingDataUrl { get; private set; }

        //public Recognizer(string subscriptionKey, string endpoint, string trainingDataUrl, Guid modelId)
        //{
        //    SubscriptionKey = subscriptionKey ?? throw new ArgumentNullException(nameof(subscriptionKey));
        //    Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        //    //TrainingDataUrl = trainingDataUrl ?? throw new ArgumentNullException(nameof(trainingDataUrl));
        //    //ModelId = modelId;
        //}
        public Recognizer(RecognizerConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            SubscriptionKey = config.Key ?? throw new ArgumentNullException(nameof(config.Key));
            Endpoint = config.Endpoint ?? throw new ArgumentNullException(nameof(config.Endpoint));
            //TrainingDataUrl = config.ModelUrl ?? throw new ArgumentNullException(nameof(config.ModelUrl));
            //ModelId = Guid.Parse(config.ModelId);
        }

        public async Task<AnalyzeResult> Scan(Stream file)
        {
            var credential = new AzureKeyCredential(SubscriptionKey);
            var client = new DocumentAnalysisClient(new Uri(Endpoint), credential);
            
            // sample document
            Uri invoiceUri = new Uri("https://raw.githubusercontent.com/Azure-Samples/cognitive-services-REST-api-samples/master/curl/form-recognizer/sample-invoice.pdf");
            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-invoice", file);
            //AnalyzeDocumentOperation operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-invoice", invoiceUri);

            AnalyzeResult result = operation.Value;

            return result;
        }
    }
}
