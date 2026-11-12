
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System.Text;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    public string FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogLine($"Received: {input}");

        dynamic? json = JsonConvert.DeserializeObject<dynamic>(input.ToString()!);

        string? issueUrl = (string?)json?.issue?.html_url;

        if (string.IsNullOrEmpty(issueUrl))
        {
            return "No issue URL found";
        }

        string payload = JsonConvert.SerializeObject(
            new { text = $"Issue Created: {issueUrl}" }
        );

        string? slackUrl = Environment.GetEnvironmentVariable("SLACK_URL");

        if (string.IsNullOrEmpty(slackUrl))
        {
            return "SLACK_URL is not configured";
        }

        using var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, slackUrl)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        using var response = client.Send(request);
        return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }
}
