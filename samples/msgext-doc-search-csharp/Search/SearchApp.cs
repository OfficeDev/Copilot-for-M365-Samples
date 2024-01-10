using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using AdaptiveCards;
using Newtonsoft.Json.Linq;
using Azure.Search.Documents.Models;

namespace msgext_doc_search_csharp.Search;

public class SearchApp : TeamsActivityHandler
{
    private readonly string _adaptiveCardFilePath = Path.Combine(".", "Resources", "helloWorldCard.json");
    private readonly AISearch _aiSearch;

    public SearchApp(AISearch aiSearch)
    {
        _aiSearch = aiSearch;
    }

    // Search
    protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
    {
        var templateJson = await System.IO.File.ReadAllTextAsync(_adaptiveCardFilePath, cancellationToken);
        var template = new AdaptiveCards.Templating.AdaptiveCardTemplate(templateJson);

        var text = query?.Parameters?[0]?.Value as string ?? string.Empty;
        var hybridResponse = await _aiSearch.SemanticHybridSearch(text);
        var attachments = new List<MessagingExtensionAttachment>();
        Console.WriteLine($"Semantic Hybrid Search Results:");
        await foreach (SearchResult<SearchDocument> result in hybridResponse.GetResultsAsync())
        {
            Debug.WriteLine($"Title: {result.Document["title"]}");
            Debug.WriteLine($"Url: {result.Document["url"]}");
            Debug.WriteLine($"Content: {result.Document["content"]}");
            Debug.WriteLine($"Filepath: {result.Document["filepath"]}\n");

            var previewCard = new HeroCard { Title = result.Document["filepath"].ToString(), Text = result.Document["content"].ToString() };
            var adaptiveCardJson = template.Expand(new { content = result.Document["content"], filepath = result.Document["filepath"], url = result.Document["url"] });
            var adaptiveCard = AdaptiveCard.FromJson(adaptiveCardJson).Card;
            var attachment = new MessagingExtensionAttachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard,
                Preview = previewCard.ToAttachment()
            };

            attachments.Add(attachment);

        }
        return new MessagingExtensionResponse
        {
            ComposeExtension = new MessagingExtensionResult
            {
                Type = "result",
                AttachmentLayout = "list",
                Attachments = attachments
            }
        };
    }
}
