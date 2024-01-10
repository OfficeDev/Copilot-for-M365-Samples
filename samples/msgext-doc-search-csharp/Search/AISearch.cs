using Azure.Search.Documents.Models;
using Azure.Search.Documents;
using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using Azure;

namespace msgext_doc_search_csharp.Search
{
    public class AISearch
    {
        private readonly string serviceEndpoint;
        private readonly string indexName;
        private readonly string key;
        private readonly string openaiApiKey;
        private readonly string openaiEndpoint;
        private readonly string deploymentName;

        public AISearch(IConfiguration configuration)
        {
            // Azure AI Search Keys
            serviceEndpoint = configuration["AZURE_SEARCH_ENDPOINT"];
            indexName = configuration["AZURE_SEARCH_INDEX_NAME"];
            key = configuration["AZURE_SEARCH_ADMIN_KEY"];

            // OpenAI Keys
            openaiApiKey = configuration["AZURE_OPENAI_API_KEY"];
            openaiEndpoint = configuration["AZURE_OPENAI_SERVICE_NAME"];
            deploymentName = configuration["AZURE_OPENAI_DEPLOYMENT_NAME"];
        }

        public async Task<SearchResults<SearchDocument>> SemanticHybridSearch(string query)
        {
            // Initialize Azure AI Search client  
            var searchCredential = new AzureKeyCredential(key);
            var indexClient = new SearchIndexClient(new Uri(serviceEndpoint), searchCredential);
            var searchClient = indexClient.GetSearchClient(indexName);

            // Generate the embedding for the query  
            var queryEmbeddings = await GenerateEmbeddings(query);

            // Perform the vector similarity search  
            var searchOptions = new SearchOptions
            {
                VectorSearch = new()
                {
                    Queries = { new VectorizedQuery(queryEmbeddings.ToArray()) { KNearestNeighborsCount = 3, Fields = { "contentVector" } } }
                },
                SemanticSearch = new()
                {
                    SemanticConfigurationName = "default",
                    //QueryCaption = new(QueryCaptionType.Extractive),
                    //QueryAnswer = new(QueryAnswerType.Extractive),
                },
                QueryType = SearchQueryType.Simple,
                Size = 3,
                Select = { "title", "content", "url", "filepath" },

            };

            SearchResults<SearchDocument> response = await searchClient.SearchAsync<SearchDocument>(query, searchOptions);
            return response;

        }
        // Function to generate embeddings  
        private async Task<ReadOnlyMemory<float>> GenerateEmbeddings(string text)
        {
            // Initialize OpenAI client  
            var credential = new AzureKeyCredential(openaiApiKey);
            var openAIClient = new OpenAIClient(new Uri(openaiEndpoint), credential);
            var response = await openAIClient.GetEmbeddingsAsync(new EmbeddingsOptions(deploymentName, new List<string> { text }));
            return response.Value.Data[0].Embedding;
        }

    }
}