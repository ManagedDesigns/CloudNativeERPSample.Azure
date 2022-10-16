using iText.Html2pdf;
using Microsoft.Extensions.Configuration;
using SampleApp.Data;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using iText.Kernel.Pdf;
using SampleApp.Services.OCR;
using Azure.AI.FormRecognizer.Models;
using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace SampleApp.Services
{
    public class AccountancyServices
    {
        private readonly IConfiguration _configuration;
        private readonly Database _database;
        private readonly Recognizer _recognizer;

        public AccountancyServices(IConfiguration configuration, Database database, Recognizer recognizer)
        {
            _configuration = configuration;
            _database = database;
            _recognizer = recognizer;
        }

        public void IssueInvoice(Invoice invoice)
        {
            if (_database.Invoices.Any(i => i.Number == invoice.Number))
                throw new ArgumentException("Invoice number already exists");

            try
            {
                _database.Invoices.Add(invoice);
                _database.SaveChanges();
                GeneratePDF(invoice);
            }
            finally
            {

            }
        }

        public void RegisterInvoice(Invoice invoice)
        {
            if (_database.Invoices.Any(i => i.Number == invoice.Number))
                throw new ArgumentException("Invoice number already exists");

            try
            {
                _database.Invoices.Add(invoice);
                _database.SaveChanges();
            }
            finally
            {

            }
        }

        public Invoice RegisterIncomingInvoice(Stream stream, string fileName)
        {
            try
            {
                SavePDF(stream, fileName);

                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);

                var invoice = _recognizer.Scan(ms);
                _database.Invoices.Add(invoice);
                _database.SaveChanges();

                return invoice;
            }
            catch
            {
                throw;
            }

        }
        
        public void SavePDF(Stream stream, string fileName)
        {
            UploadStreamToBlob(stream, fileName);

        }

        private void GeneratePDF(Invoice invoice)
        {
            string invoiceHtml = $"<div><p>Customer: {invoice.CustomerName}</p><p>Invoice # {invoice.Number}</p></div>";
            string fileName = $@"{invoice.Number.Replace("/", "-")}.pdf";
            try
            {
                using var workStream = new MemoryStream();
                using var pdfWriter = new PdfWriter(workStream);
                using var document = HtmlConverter.ConvertToDocument(invoiceHtml, pdfWriter);
                document.Close();
                using var blobStream = new MemoryStream(workStream.ToArray());
                UploadStreamToBlob(blobStream, fileName);
            }
            catch
            {

            }
        }
        private void UploadStreamToBlob(Stream stream, string fileName)
        {
            string connectionString = _configuration["StorageAccount:ConnectionString"];
            string containerName = _configuration["StorageAccount:InvoicesContainerName"];
            var container = new BlobContainerClient(connectionString, containerName);
            var blob = container.GetBlobClient(fileName);
            var blobHttpHeader = new BlobHttpHeaders();
            blobHttpHeader.ContentType = "application/pdf";

            blob.UploadAsync(stream, blobHttpHeader).Wait();
        }

 
    }
}
