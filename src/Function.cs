using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Alexa.NET.Response.Directive.Templates;
using Alexa.NET.Response.Directive.Templates.Types;

using System.Net.Http;
using Newtonsoft.Json;
using Alexa.NET.Request.Type;
using System.Linq;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializerAttribute(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace Alexa.BridgeBot.Lambda
{
    public class Function
    {
        // URL template for Liquid Content
        string MapUrl => "http://dev.virtualearth.net/REST/v1/Traffic/Incidents/{1}?key={0}";

        Dictionary<string, string> Bridges => new Dictionary<string, string>
        {
            {"san mateo", "37.573331,-122.262676,37.618759,-122.149551" },
            {"oakland", "" },
            {"golden gate", "" }
        };

        private ILambdaContext Context { get; set; }

        // Liquid Content requires an API Key to access the APIs
        private String MapApiKey { get; set; }

        private readonly IDictionary<string, Func<SkillRequest, SkillResponse>> handlers;

        public Function()
        {
            handlers = new Dictionary<string, Func<SkillRequest, SkillResponse>>() {
                { "LaunchRequest", HandleLaunchRequest },
                { "IntentRequest", HandleIntentRequest },
                { "SessionEndedRequest", HandleSessionEndRequest },

                { "GetBridgeTraffic", HandleBridgeTrafficIntent }
            };
        }

        // Intent handlers encapsulate the business logic for each custom intent
        #region Intent Handlers
        public SkillResponse HandleBridgeTrafficIntent(SkillRequest input)
        {
            // Log the method type for debugging purposes
            Context.Logger.LogLine("Calling SendDirections Intent");

            string title = "Liquid Summit Directions";
            string speech = "Thank you for your interest in the Liquid Summit Conference.  You can find directions to the event in your Alexa app.";


            var directions = GetDirectionsAsync().Result;

            if (directions is null)
            {
                return ResponseBuilder.TellWithCard(
                    new PlainTextOutputSpeech()
                    {
                        Text = speech
                    },
                    title,
                    speech
                );
            }

            return new SkillResponse()
            {
                Version = "1.0",
                Response = new ResponseBody()
                {
                    Card = new StandardCard()
                    {
                        Title = $"{title}",
                        Content = $"{directions.details.directions}",
                        Image = new CardImage
                        {
                            SmallImageUrl = directions.details.mapImage.First().url,
                            LargeImageUrl = directions.details.mapImage.First().url
                        }
                    },
                    OutputSpeech = new PlainTextOutputSpeech()
                    {
                        Text = speech
                    },
                    ShouldEndSession = true,
                    Directives = {
                        new DisplayRenderTemplateDirective()
                        {
                            Template = new BodyTemplate1() {
                                Token = "Directions-Map",
                                Title="Liquid Summit Directions",
                                Content = new TemplateContent()
                                {
                                    Primary = new TemplateText() {
                                        Type = "PlainText",
                                        Text = $"{directions.details.directions}"
                                    }
                                }
                            }
                        },
                        new HintDirective() {
                            Hint = new Hint(){
                                Type="PlainText",
                                Text = "send me directions to the conference."
                            }
                        }
                    }
                }
            };
        }
        #endregion

        // The service handlers are responsible for making service calls to Bing
        #region Service Handlers
        private async Task<SpeakerDetails> GetTrafficInfoAsync()
        {

            string url = string.Format(MapUrl, MapApiKey);

            var json = await GetContentAsync(url);

            Context.Logger.LogLine("GetKeynoteSpeakerAsync:");
            Context.Logger.LogLine("-------------------------------");
            Context.Logger.LogLine($"Address: {url}");
            Context.Logger.LogLine($"Results: {json}");
            Context.Logger.LogLine("-------------------------------");

            var speakerList = JsonConvert.DeserializeObject<SpeakerContentViewModel>(json);
            var obj = JsonConvert.SerializeObject(speakerList);

            Context.Logger.LogLine($"Results: {speakerList.speakers.Count}");

            if (speakerList.speakers == null || speakerList.speakers.Count == 0) return null;


            return speakerList.speakers?.First()?.details;

        }

        #endregion

        // Request Handlers are responsible for the three request types we could recieve.
        #region Request Handlers
        /// <summary>
        /// A sample Launch Request handler.  This returns rudimentary information about the app.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public SkillResponse HandleLaunchRequest(SkillRequest input)
        {
            // Log the method type for debugging purposes
            Context.Logger.LogLine("Calling HandleLaunchRequest");

            return ResponseBuilder.Tell(
                new PlainTextOutputSpeech()
                {
                    Text = "Welcome to the Bridge Bot. You can ask me for traffic information on any of the nearby bridges."
                }
            );

        }

        /// <summary>
        /// The HandleIntentRequest method processes all intent requests and will 
        /// delegate the request to the appropriate intent handler based on the name of
        /// the intent that is requested.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public SkillResponse HandleIntentRequest(SkillRequest input)
        {
            // Log the method type for debugging purposes
            Context.Logger.LogLine("Calling HandleRequest");
            var request = input.Request as IntentRequest;
            return handlers[request.Intent.Name](input);
        }

        public SkillResponse HandleSessionEndRequest(SkillRequest input)
        {
            // Log the method type for debugging purposes
            Context.Logger.LogLine("Calling HandleSessionEndRequest");

            return ResponseBuilder.Empty();
        }

        #endregion

        /// <summary>
        /// The main entry point for our Alexa Skill
        /// </summary>
        /// <param name="input">This is the request object from Alexa</param>
        /// <param name="context">The context information for the request</param>
        /// <returns></returns>
        public SkillResponse Handler(SkillRequest input, ILambdaContext context)
        {
            Context = context;

            MapApiKey = Environment.GetEnvironmentVariable("mapapikey");

            return handlers[input.Request.Type](input);
        }

    }
}
