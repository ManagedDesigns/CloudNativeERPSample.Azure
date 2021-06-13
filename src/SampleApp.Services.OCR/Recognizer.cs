using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using Azure.AI.FormRecognizer.Training;
using Microsoft.Azure.Management.Monitor.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SampleApp.Services.OCR
{
    public class Recognizer
    {
        public string SubscriptionKey { get; private set; }
        public string Endpoint { get; private set; }

        public Guid ModelId { get; private set; }
        public string TrainingDataUrl { get; private set; }

        public Recognizer(string subscriptionKey, string endpoint, string trainingDataUrl, Guid modelId)
        {
            SubscriptionKey = subscriptionKey ?? throw new ArgumentNullException(nameof(subscriptionKey));
            Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            TrainingDataUrl = trainingDataUrl ?? throw new ArgumentNullException(nameof(trainingDataUrl));
            ModelId = modelId;
        }

        public Recognizer(RecognizerConfig config)
        {
            if(config==null)
                throw new ArgumentNullException(nameof(config));

            SubscriptionKey = config.Key ?? throw new ArgumentNullException(nameof(config.Key));
            Endpoint = config.Endpoint ?? throw new ArgumentNullException(nameof(config.Endpoint));
            TrainingDataUrl = config.ModelUrl ?? throw new ArgumentNullException(nameof(config.ModelUrl));
            ModelId = Guid.Parse(config.ModelId);
        }

        public RecognizedFormCollection Scan(Stream file)
        {
            var formClient = BuildFormRecognizerClient();
            var forms = formClient
                            .StartRecognizeCustomFormsAsync(ModelId.ToString(), file)
                            .WaitForCompletionAsync()
                            .Result.Value;

            return forms;            
        }

        private FormRecognizerClient BuildFormRecognizerClient()
        {
            var credential = new AzureKeyCredential(SubscriptionKey);
            var uri = new Uri(Endpoint);
            var formClient = new FormRecognizerClient(uri, credential);

            return formClient;
        }

        private Response<RecognizedFormCollection> AnalyzeForm(FormRecognizerClient formClient, Guid modelId, Stream formFile)
        {
            try
            {
                var forms = formClient
                                .StartRecognizeCustomFormsAsync(modelId.ToString(), formFile)
                                .WaitForCompletionAsync()
                                .Result;

                return forms;
            }
            catch (ErrorResponseException e)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
