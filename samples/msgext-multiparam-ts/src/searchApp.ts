import { AdaptiveCards } from "@microsoft/adaptivecards-tools";
import {
  TeamsActivityHandler,
  CardFactory,
  TurnContext,
  MessagingExtensionQuery,
  MessagingExtensionResponse,
  MessagingExtensionParameter,
} from "botbuilder";
import stockCard from "./cards/stock.json";
import stockCardData from "./cards/stock.data.json";
import { StockData } from "./types";

export class SearchApp extends TeamsActivityHandler {

  public async handleTeamsMessagingExtensionQuery(
    context: TurnContext,
    query: MessagingExtensionQuery
  ): Promise<MessagingExtensionResponse> {
    const { parameters } = query;

    // Basic: Find stocks in NASDAQ Stocks
    // [
    //   { name: 'StockIndex', value: 'NASDAQ' },
    //   { name: 'NumberofStocks', value: '' },
    //   { name: 'P/B', value: '' },
    //   { name: 'P/E', value: '' }
    // ]

    // Advanced: Find top 10 stocks in NASDAQ Stocks with P/B < 2 and P/E < 30
    // [
    //   { name: 'StockIndex', value: '' },
    //   { name: 'NumberofStocks', value: 'Top:10' },
    //   { name: 'P/B', value: '<2' },
    //   { name: 'P/E', value: '<30' }
    // ]

    const stockIndex = getParameterByName(parameters, "StockIndex");
    const numberOfStocks = getParameterByName(parameters, "NumberofStocks");
    const pB = getParameterByName(parameters, "P/B");
    const pE = getParameterByName(parameters, "P/E");

    console.log(
      `🔍 StockIndex: '${stockIndex}' | NumberofStocks: '${numberOfStocks}' | P/B: '${pB}' | P/E: '${pE}'`
    );

    const attachments = stockCardData.map((stock) => {
      const card = AdaptiveCards.declare<StockData>(stockCard).render(stock);
      const resultCard = CardFactory.adaptiveCard(card);
      const previewCard = CardFactory.heroCard(stock.companyName, stock.symbol);
      return { ...resultCard, previewCard };
    });

    return {
      composeExtension: {
        type: "result",
        attachmentLayout: "list",
        attachments,
      },
    };
  }
}

const getParameterByName = (parameters: MessagingExtensionParameter[], name: string) => {
  const param = parameters.find(p => p.name === name);
  return param ? param.value : '';
}
