const axios = require("axios");
const querystring = require("querystring");
const { TeamsActivityHandler, CardFactory } = require("botbuilder");
const { doSemanticHybridSearch } = require("./cogSearch");

class SearchApp extends TeamsActivityHandler {
  constructor() {
    super();
  }

  // Message extension Code
  // Search.
  async handleTeamsMessagingExtensionQuery(context, query) {
    const searchQuery = query.parameters[0].value;
    const response = await doSemanticHybridSearch(searchQuery);

    const attachments = [];

    for await (const result of response.results) {
      console.log(`Title: ${result.document.title}`);
      console.log(`Content: ${result.document.content}`);
      console.log(`Url: ${result.document.url}`);
      console.log(`Filepath: ${result.document.filepath}`);
      console.log(`\n`);
      const adaptiveCard = CardFactory.adaptiveCard({
        $schema: "http://adaptivecards.io/schemas/adaptive-card.json",
        type: "AdaptiveCard",
        version: "1.4",
        body: [
          {
            type: "TextBlock",
            text: `${result.document.content}`,
            wrap: true,
            size: "medium",
          }
        ],
        actions:[
            {
              type: "Action.OpenUrl",
              title: `${result.document.filepath}`,
              url: `${result.document.url}`
            }
        ]
      });
      const preview = CardFactory.heroCard(result.document.filepath, result.document.content);
      const attachment = { ...adaptiveCard, preview };
      attachments.push(attachment);
    };

    return {
      composeExtension: {
        type: "result",
        attachmentLayout: "list",
        attachments: attachments,
      },
    };
  }
}

module.exports.SearchApp = SearchApp;
