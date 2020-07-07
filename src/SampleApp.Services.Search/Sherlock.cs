using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using SampleApp.Services.Search.Model;
using System;

namespace SampleApp.Services.Search
{
    public class Sherlock
    {
        private readonly string searchServiceName;
        private readonly string queryApiKey;
        private readonly string indexName;
        private readonly string blobIndexName;

        public Sherlock(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            searchServiceName = configuration["AzureSearch:ServiceName"];
            queryApiKey = configuration["AzureSearch:QueryKey"];
            indexName = configuration["AzureSearch:IndexName"];
            blobIndexName = configuration["AzureSearch:BlobIndexName"];
        }

        public DocumentSearchResult<SqlInvoice> RetrieveInvoices(string query)
        {
            var client = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(queryApiKey));
            var parameters = new SearchParameters()
            {
                Select = new[] { nameof(SqlInvoice.Id), nameof(SqlInvoice.CustomerName), nameof(SqlInvoice.Description) },
                Top = 20
            };
            var results = client.Documents.Search<SqlInvoice>(query, parameters);

            return results;
        }

        public DocumentSearchResult<BlobInvoice> RetrieveInvoicesDeep(string query)
        {
            var client = new SearchIndexClient(searchServiceName, blobIndexName, new SearchCredentials(queryApiKey));
            var parameters = new SearchParameters()
            {
                Top = 20
            };
            var results = client.Documents.Search<BlobInvoice>(query, parameters);

            return results;
        }
    }
}
