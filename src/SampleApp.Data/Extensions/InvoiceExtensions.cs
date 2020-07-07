using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleApp.Data.Extensions
{
    public static class InvoiceExtensions
    {

        public static IQueryable<Invoice> Due(this IQueryable<Invoice> invoices)
        {
            return invoices.Where(i => i.PaymentDate == null);
        }

        public static IQueryable<Invoice> Overdue(this IQueryable<Invoice> invoices)
        {
            var today = DateTime.Now.Date;
            return invoices.Due().Where(i => i.DueDate < today );
        }
    }
}
