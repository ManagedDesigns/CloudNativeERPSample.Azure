using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Connector.Authentication;
using SampleApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace SampleApp.Bot.Bots
{
    public class AccountancyBotWorker
    {
        public Database Database { get; private set; }

        public AccountancyBotWorker(Database database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public string ProduceAnswer(PredictionResponse prediction, string language)
        {
            switch (prediction.Prediction.TopIntent)
            {
                case "RunningGrossIncome":
                    return RunningGrossIncomeBotWorker(prediction, language);
                case "YearlyGrossIncome":
                    return YearlyGrossIncomeBotWorker(prediction, language);
                case "OutgoingInvoicePayment":
                    return OutgoingInvoicePaymentBotWorker(prediction, language);
                default:
                    return null;
            }
        }

        public string RunningGrossIncomeBotWorker(PredictionResponse prediction, string language)
        {
            var year = DateTime.Now.Year;
            var runningGrossIncome = Database.Invoices.Where(i=>i.Date.Year==year).Sum(i => i.Price);
            return language switch
            {
                "it" => $"Il fatturato corrente ammonta a {runningGrossIncome:0.##} Euro",
                "en" => $"The running gross income is {runningGrossIncome:0.##} Euro",
                _ => throw new NotImplementedException()
            };
        }

        public string YearlyGrossIncomeBotWorker(PredictionResponse prediction, string language)
        {
            var value = ExtractEntityScalarValue(prediction, "year");
            var year = prediction.Prediction.Entities.Count == 0 ? DateTime.Now.Year - 1 : int.Parse(value);
            var runningGrossIncome = Database.Invoices.Where(i => i.Date.Year == year).Sum(i => i.Price);
            return language switch
            {
                "it" => $"Nel {year} abbiamo fatturato {runningGrossIncome:0.##} Euro",
                "en" => $"We grossed {runningGrossIncome:0.##} Euro in {year}",
                _ => throw new NotImplementedException()
            };
        }

        public string OutgoingInvoicePaymentBotWorker(PredictionResponse prediction, string language)
        {
            var invoiceNumber = ExtractEntityScalarValue(prediction, "invoiceNumber");
            var invoice = Database.Invoices.Where(i => i.Number == invoiceNumber).FirstOrDefault();

            if (invoice == null)
                return language switch
                {
                    "en" => "The invoice was not found in the database",
                    "it" => "La fattura specificata è inesistente",
                    _ => throw new NotImplementedException()
                };
            else
                return (language, invoice.PaymentDate) switch
                {
                    ("en", null) => $"Invoice {invoice.Number} hasn't been paid yet",
                    ("it", null) => $"La fattura {invoice.Number} non è ancora stata pagata",
                    ("en", DateTime d) => $"Invoice {invoice.Number} was paid on {invoice.PaymentDate:dd/MM/yyyy}",
                    ("it", DateTime d) => $"La fattura {invoice.Number} è stata pagata il {invoice.PaymentDate:dd/MM/yyyy}",
                    _ => throw new NotImplementedException()
                };
        }

        private string ExtractEntityScalarValue(PredictionResponse prediction, string entityName)
        {
            return (string)((prediction.Prediction.Entities[entityName] as JArray).First() as JValue).Value;
        }
    }
}
