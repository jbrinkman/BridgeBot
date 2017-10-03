using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Response;

using System.Net.Http;
using Newtonsoft.Json;
using Alexa.NET.Request.Type;
using System.Linq;
using Alexa.BridgeBot.Lambda.viewmodels;


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
			{"san mateo bridge", "37.564618,-122.272450,37.631134,-122.112215" },
			{"dumbarton bridge", "37.489248,-122.139863,37.537884,-122.070231" },
			{"bay bridge", "37.537884,-122.070231,37.822621,-122.321842" },
			{"golden gate bridge", "37.809331,-122.479695,37.829536,-122.477367" }
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
			IntentRequest request = input.Request as IntentRequest;
			string bridgeName = request.Intent.Slots["BridgeName"].Value;

			// Log the method type for debugging purposes
			Context.Logger.LogLine("Calling HandleBridgeTrafficIntent Intent");


			string title = "Bridge Traffic";
			string speech = $"I'm sorry, we couldn't find any traffic data for the {bridgeName}.";

			if (Bridges.ContainsKey(bridgeName))
			{
				var traffic = GetTrafficInfoAsync(bridgeName).Result;

			};

			//var traffic = GetTrafficInfoAsync().Result;

			//if (traffic is null)
			//{
			return ResponseBuilder.TellWithCard(
				new PlainTextOutputSpeech()
				{
					Text = speech
				},
				title,
				speech
			);
			//}

			//return new SkillResponse()
			//{
			//    Version = "1.0",
			//    Response = new ResponseBody()
			//    {
			//        Card = new StandardCard()
			//        {
			//            Title = $"{title}",
			//            Content = $"{directions.details.directions}",
			//            Image = new CardImage
			//            {
			//                SmallImageUrl = directions.details.mapImage.First().url,
			//                LargeImageUrl = directions.details.mapImage.First().url
			//            }
			//        },
			//        OutputSpeech = new PlainTextOutputSpeech()
			//        {
			//            Text = speech
			//        },
			//        ShouldEndSession = true,
			//        Directives = {
			//            new DisplayRenderTemplateDirective()
			//            {
			//                Template = new BodyTemplate1() {
			//                    Token = "Directions-Map",
			//                    Title="Liquid Summit Directions",
			//                    Content = new TemplateContent()
			//                    {
			//                        Primary = new TemplateText() {
			//                            Type = "PlainText",
			//                            Text = $"{directions.details.directions}"
			//                        }
			//                    }
			//                }
			//            },
			//            new HintDirective() {
			//                Hint = new Hint(){
			//                    Type="PlainText",
			//                    Text = "send me directions to the conference."
			//                }
			//            }
			//        }
			//    }
			//};
		}
		#endregion

		// The service handlers are responsible for making service calls to Bing
		#region Service Handlers
		private async Task<TrafficEvent> GetTrafficInfoAsync(string bridge)
		{

			string url = string.Format(MapUrl, MapApiKey, Bridges[bridge]);

			var json = await GetContentAsync(url);

			Context.Logger.LogLine("GetTrafficInfoAsync:");
			Context.Logger.LogLine("-------------------------------");
			Context.Logger.LogLine($"Address: {url}");
			Context.Logger.LogLine($"Results: {json}");
			Context.Logger.LogLine("-------------------------------");

			var traffic = JsonConvert.DeserializeObject<TrafficEvent>(json);

			Context.Logger.LogLine($"Results: {traffic}");

			return traffic;

		}

		private async Task<string> GetContentAsync(string Url)
		{
			var client = new HttpClient
			{
				BaseAddress = new Uri(Url)
			};

			var response = client.GetAsync("");
			var json = await response.Result.Content.ReadAsStringAsync();

			return json;
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

			MapApiKey = Environment.GetEnvironmentVariable("MapApiKey");

			return handlers[input.Request.Type](input);
		}

	}
}
