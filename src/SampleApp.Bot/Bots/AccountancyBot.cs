using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace SampleApp.Bot.Bots
{
    public class AccountancyBot : ActivityHandler
    {
        public IConfiguration Configuration { get; set; }

        public AccountancyBotWorker BotWorker { get; private set; }

        public AccountancyBot(IConfiguration configuration, AccountancyBotWorker botWorker)
        {
            this.Configuration = configuration;
            this.BotWorker = botWorker;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var utterance = turnContext.Activity.Text;
            var language = GuessLanguage(utterance);
            var sentiment = Sentiment(utterance, language);
            var appId = GetAppId(language);
            var prediction = await ParseUtterance(utterance, language, appId);
            var predictionConfidence = GetIntentPredictionConfidence(prediction);

            var replyText = (sentiment, predictionConfidence, language) switch
            {
                (SentimentScore.Bad, _, "en") => "Let's have this conversation being a polite one, please",
                (SentimentScore.Bad, _, "it") => "Rimaniamo sul professionale, prego",
                (SentimentScore.Neutral, IntentPredictionScore.NotConfident, "en") => "Let's have this conversation being a polite one, please",
                (SentimentScore.Neutral, IntentPredictionScore.NotConfident, "it") => "Rimaniamo sul professionale, prego",
                (SentimentScore.Good, IntentPredictionScore.NotConfident, "en") => "I can't answer your question, I'm afraid",
                (SentimentScore.Good, IntentPredictionScore.NotConfident, "it") => "Mi spiace, non sono in grado di rispondere",
                (_, IntentPredictionScore.Confident, { }) => BotWorker.ProduceAnswer(prediction, language)
            };

            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        private string GuessLanguage(string utterance)
        {
            var credentials = new ApiKeyServiceClientCredentials(Configuration["TextAnalytics:SubscriptionKey"]);

            var client = new TextAnalyticsClient(credentials)
            {
                Endpoint = Configuration["TextAnalytics:Endpoint"]
            };
            var result = client.DetectLanguage(utterance, "");
            if (result.DetectedLanguages == null || result.DetectedLanguages.Count == 0)
                return "en";
            else
                return result.DetectedLanguages[0].Iso6391Name;
        }

        private Guid GetAppId(string language)
        {
            switch (language)
            {
                case "it":
                    return Guid.Parse(Configuration["LUIS:AppIdIT"]);
                case "en":
                default:
                    return Guid.Parse(Configuration["LUIS:AppIdEN"]);
            }
        }

        private SentimentScore Sentiment(string utterance, string language)
        {
            var credentials = new ApiKeyServiceClientCredentials(Configuration["TextAnalytics:SubscriptionKey"]);

            var client = new TextAnalyticsClient(credentials)
            {
                Endpoint = Configuration["TextAnalytics:Endpoint"]
            };

            var result = client.Sentiment(utterance, language);

            if (result.Score < 0.4)
                return SentimentScore.Bad;
            else if (result.Score >= 0.4 && result.Score < 0.65)
                return SentimentScore.Neutral;
            else
                return SentimentScore.Good;
        }

        private async Task<PredictionResponse> ParseUtterance(string utterance, string language, Guid appId)
        {
            var credentials = new ApiKeyServiceClientCredentials(Configuration["LUIS:PredictionKey"]);
            using var luisClient = new LUISRuntimeClient(credentials, new System.Net.Http.DelegatingHandler[] { })
            {
                Endpoint = Configuration["LUIS:Endpoint"]
            };
            var requestOptions = new PredictionRequestOptions
            {
                DatetimeReference = DateTime.Now,
                PreferExternalEntities = true
            };

            var predictionRequest = new PredictionRequest
            {
                Query = utterance,
                Options = requestOptions
            };

            var prediction = await luisClient.Prediction.GetSlotPredictionAsync(
                appId,
                slotName: Configuration["LUIS:SlotName"],
                predictionRequest,
                verbose: true,
                showAllIntents: true,
                log: true);
            return prediction;
        }

        private IntentPredictionScore GetIntentPredictionConfidence(PredictionResponse prediction)
        {
            var result = prediction.Prediction.Intents
                .Where(i => i.Key == prediction?.Prediction?.TopIntent)
                .SingleOrDefault();
            if (result.Value.Score < 0.7)
                return IntentPredictionScore.NotConfident;
            else
                return IntentPredictionScore.Confident;
        }

        private enum IntentPredictionScore
        {
            Confident,
            NotConfident
        }

        private enum SentimentScore
        {
            Good,
            Neutral,
            Bad
        }
    }
}
