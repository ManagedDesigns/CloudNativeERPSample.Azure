using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using SampleApp.Data;

namespace SampleApp.Services.OCR
{
    public class Recognizer
    {
        public string SubscriptionKey { get; private set; }
        public string Endpoint { get; private set; }

        public Recognizer(RecognizerConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            SubscriptionKey = config.Key ?? throw new ArgumentNullException(nameof(config.Key));
            Endpoint = config.Endpoint ?? throw new ArgumentNullException(nameof(config.Endpoint));
        }

        public Invoice Scan(Stream file)
        {
            var credential = new AzureKeyCredential(SubscriptionKey);
            var client = new DocumentAnalysisClient(new Uri(Endpoint), credential);
            
            AnalyzeDocumentOperation operation = client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-invoice", file).Result;
            AnalyzeResult result = operation.Value;
            var invoice = BuildInvoice(result);
            
            return invoice;
        }

        private Invoice BuildInvoice(AnalyzeResult result)
        {            
            AnalyzedDocument document = result.Documents[0] ?? null;
            if (document == null)
                return null;

            var invoice = new Invoice();
            if (document.Fields.TryGetValue("VendorName", out DocumentField? vendorNameField))
            {
                if (vendorNameField.FieldType == DocumentFieldType.String)
                {
                    string vendorName = vendorNameField.Value.AsString();
                    invoice.VendorName = vendorName;
                }
            }

            if (document.Fields.TryGetValue("CustomerName", out DocumentField? customerNameField))
            {
                if (customerNameField.FieldType == DocumentFieldType.String)
                {                   
                    string customerName = customerNameField.Value.AsString();
                    invoice.CustomerName = customerName;
                }
            }
            
            return invoice;
        }
    }
}
