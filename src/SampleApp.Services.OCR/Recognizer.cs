using Microsoft.Azure.CognitiveServices.FormRecognizer;
using Microsoft.Azure.CognitiveServices.FormRecognizer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Services.OCR
{
    public class Recognizer
    {
        public string SubscriptionKey { get; private set; }
        public string Endpoint { get; private set; }

        public string TrainingDataUrl { get; private set; }

        public Recognizer(string subscriptionKey, string endpoint, string trainingDataUrl)
        {
            SubscriptionKey = subscriptionKey ?? throw new ArgumentNullException(nameof(subscriptionKey));
            Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            TrainingDataUrl = endpoint ?? throw new ArgumentNullException(nameof(trainingDataUrl));
        }

        public async Task<AnalyzeResult> RecognizeForm(Stream pdfFormFile)
        {
            var formClient = new FormRecognizerClient(new ApiKeyServiceClientCredentials(SubscriptionKey))
            {
                Endpoint = this.Endpoint
            };

            var modelId = await TrainModelAsync(formClient, TrainingDataUrl);
            var result = await AnalyzePdfForm(formClient, modelId, pdfFormFile);
            await DeleteModel(formClient, modelId);

            return result;
        }

        private static async Task<Guid> TrainModelAsync(IFormRecognizerClient formClient, string trainingDataUrl)
        {
            if (!Uri.IsWellFormedUriString(trainingDataUrl, UriKind.Absolute))
                return Guid.Empty;

            try
            {
                var result = await formClient.TrainCustomModelAsync(new TrainRequest(trainingDataUrl));
                var model = await formClient.GetCustomModelAsync(result.ModelId);

                return result.ModelId;
            }
            catch (ErrorResponseException e)
            {
                return Guid.Empty;
            }
        }

        private async Task<AnalyzeResult> AnalyzePdfForm(IFormRecognizerClient formClient, Guid modelId, Stream pdfFormFile)
        {

            try
            {
                return await formClient.AnalyzeWithCustomModelAsync(modelId, pdfFormFile, contentType: "application/pdf");
            }
            catch (ErrorResponseException e)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static async Task DeleteModel(IFormRecognizerClient formClient, Guid modelId)
        {
            try
            {
                await formClient.DeleteCustomModelAsync(modelId);
            }
            catch (ErrorResponseException e)
            {
            }
        }
    }
}
