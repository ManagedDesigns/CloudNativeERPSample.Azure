
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SampleApp.Data;
using SampleApp.Models.Invoice;
using SampleApp.Services;
using SampleApp.Services.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Database _database;
        private readonly Sherlock _sherlock;

        public InvoiceController(ILogger<HomeController> logger, Database database, Sherlock sherlock)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _sherlock = sherlock ?? throw new ArgumentNullException(nameof(sherlock));
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new IndexViewModel()
            {
                Invoices = RetrieveInvoices(null)
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Index(IndexViewModel model)
        {
            model.Invoices = RetrieveInvoices(model);

            return View(model);
        }

        private IEnumerable<IndexViewModel.Invoice> RetrieveInvoices(IndexViewModel model)
        {
            IEnumerable<Invoice> query = null;
            if (model == null)
                query = _database.Invoices;
            else if (model != null && string.IsNullOrEmpty(model.CustomerName) && string.IsNullOrEmpty(model.Query))
                query = _database.Invoices;
            else if (model != null && !string.IsNullOrEmpty(model.CustomerName))
                query = _database.Invoices.Where(i => i.CustomerName.Contains(model.CustomerName));
            else if (model != null && !string.IsNullOrEmpty(model.Query) && !model.DeepSearch)
            {
                var ids = from i in _sherlock.RetrieveInvoices(model.Query).Results
                          select i.Document.Id;
                query = from i in _database.Invoices
                        where ids.Contains(i.Id)
                        select i;
            }
            else if (model != null && !string.IsNullOrEmpty(model.Query) && model.DeepSearch)
            {
                var invoiceNumbers = from i in _sherlock.RetrieveInvoicesDeep(model.Query).Results
                            select i.Document.MetadataStorageName.Replace("K", "0").Replace("-", "/").Replace(".pdf", "");
                query = from i in _database.Invoices
                        where invoiceNumbers.Contains(i.Number)
                        select i;
            }
            var invoices = (from i in query
                             orderby i.Date descending
                             select new IndexViewModel.Invoice
                             {
                                 Id = i.Id,
                                 Number = i.Number,
                                 CustomerName = i.CustomerName,
                                 Date = i.Date,
                                 Description = i.Description,
                                 DueDate = i.DueDate,
                                 PaymentDate = i.PaymentDate,
                                 Price = i.Price,
                                 Taxes = i.Taxes,
                                 TotalPrice = i.TotalPrice
                             })
                             .Take(20)
                             .ToList();

            return invoices;
        }

        [HttpGet]
        public IActionResult View(int id)
        {
            var invoice = _database.Invoices.Where(i => i.Id == id).FirstOrDefault();
            if (invoice == null)
                return RedirectToAction(nameof(InvoiceController.Index));
            return View(invoice);
        }


        [HttpGet]
        public IActionResult Issue()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Issue(Invoice model, [FromServices] AccountancyServices accountancyServices)
        {
            if (!this.ModelState.IsValid)
                return View(model);

            accountancyServices.IssueInvoice(model);
            
            return RedirectToAction(nameof(InvoiceController.Index));
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public void Upload(IFormFile file, [FromServices] AccountancyServices accountancyServices)
        {
            accountancyServices.SavePDF(file);
        }
    }
}