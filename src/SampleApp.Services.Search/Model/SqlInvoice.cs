using Microsoft.Azure.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.Services.Search.Model
{
    public class SqlInvoice
    {
        [Key]
        public int Id { get; set; }

        [IsSearchable, IsFilterable]
        public string CustomerName { get; set; }

        [IsSearchable, IsFilterable]
        public string Description { get; set; }
    }
}
