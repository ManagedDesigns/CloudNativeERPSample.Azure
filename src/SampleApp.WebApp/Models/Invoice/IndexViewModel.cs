using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.Models.Invoice
{
    public class IndexViewModel
    {
        public string CustomerName { get; set; }
        
        public string Query { get; set; }

        public bool DeepSearch { get; set; }

        public IEnumerable<IndexViewModel.Invoice> Invoices { get; set; }
        public Invoice.State InvoiceState { get; set; }

        public class Invoice
        {
            public int Id { get; set; }

            public string Number { get; set; }
            public string CustomerName { get; set; }

            [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
            public DateTime Date { get; set; }

            [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
            public DateTime? DueDate { get; set; }

            [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
            public DateTime? PaymentDate { get; set; }

            [DisplayFormat(DataFormatString = "{0:###,###,##0.00}")]
            public decimal Price { get; set; }

            [DisplayFormat(DataFormatString = "{0:###,###,##0.00}")]
            public decimal Taxes { get; set; }

            [DisplayFormat(DataFormatString = "{0:###,###,##0.00}")]
            public decimal TotalPrice { get; set; }
            public string Description { get; set; }

            public enum State
            {
                Any,
                Due,
                Overdue
            }
        }
    }
}
