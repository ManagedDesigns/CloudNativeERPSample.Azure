using Microsoft.Azure.Search;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.Services.Search.Model
{
    public class BlobInvoice
    {
        [IsSearchable]
        public string Content { get; set; }

        [JsonProperty("metadata_storage_name")]
        public string MetadataStorageName { get; set; }
    }
}
