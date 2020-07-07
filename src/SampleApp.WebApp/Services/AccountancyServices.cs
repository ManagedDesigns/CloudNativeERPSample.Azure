using iText.Html2pdf;
using Microsoft.Extensions.Configuration;
using SampleApp.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using SampleApp.Services.Search;

namespace SampleApp.Services
{
    public class AccountancyServices
    {
        private readonly IConfiguration _configuration;
        private readonly Database _database;

        public AccountancyServices(IConfiguration configuration, Database database)
        {
            _configuration = configuration;
            _database = database;
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

        public void SavePDF(IFormFile file)
        {
            UploadStreamToBlob(file.OpenReadStream(), file.FileName);
        }

        private void GeneratePDF(Invoice invoice)
        {
            string invoiceHtml = $"<div><p>Customer: {invoice.CustomerName}</p><p>Invoice # {invoice.Number}</p></div>";
            string fileName = $@"{invoice.Id}.pdf";

            using MemoryStream fs = new MemoryStream();
            HtmlConverter.ConvertToPdf(invoiceHtml, fs);

            UploadStreamToBlob(fs, fileName);
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
