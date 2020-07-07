using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.Services
{
    public class UrlServices
    {
        public IConfiguration Configuration { get; }
        public UrlServices(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string InvoiceUrl(string invoiceNumber)
        {
            var fileName = invoiceNumber.Replace("/20", "-2K");
            return $"{Configuration["StorageAccount:InvoicesContainerPath"]}/{fileName}.pdf";
        }
    }
}
