using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.Data
{
    public class Invoice
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Number { get; set; }
        [Required]
        [MaxLength(200)]
        public string CustomerName { get; set; }
        [Required]
        public DateTime Date { get; set; }

        public DateTime? DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }

        [Required]
        public decimal Price { get; set; }
        [Required]
        public decimal Taxes { get; set; }
        [Required]
        public decimal TotalPrice { get; set; }
        [Required]
        [MaxLength(200)]
        public string Description { get; set; }
    }
}
