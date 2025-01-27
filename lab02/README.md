# Durable Functions ラボ

## はじめに

このラボでは Durable Functions を使用して AI Agent のオーケストレーションを行う処理を実装します。

## ひな形のソースコードの準備

このリポジトリにある `lab02/lab02-start` フォルダーを任意の場所にコピーしてください。

コピーが完了 Visual Studio 2022 で `lab02-start` フォルダーにある `DurableMultiAgentTemplate.sln` を開いてください。

## ソースコードの確認

このコードは 2 つのプロジェクトで構成されています。

1. `DurableMutliAgentTemplate`
   - Durable Functions を使用して AI Agent のオーケストレーションを行う処理を実装します。このラボではこのプロジェクトのみコードを変更します。
2. `DurableMutliAgentTemplate.Client`
   - `DurableMultiAgentTemplate` で定義された Web API を呼び出すための画面を提供します。Blazor Web App で実装されています。

### `DurableMutliAgentTemplate` プロジェクト

このプロジェクトの `Agents` フォルダーには 5 つの AI Agent が実装されています。

![](images/2025-01-27-13-38-17.png)

それぞれの Agent は以下の機能を提供します。

- `GetDestinationSuggestAgent`
  - "希望の行き先に求める条件を自然言語で与えると、おすすめの旅行先を提案します。
- `GetClimateAgent`
  - 指定された場所の気候を取得します。
- `GetSightseeingSpotAgent`
  - 指定された場所の観光名所を取得します。
- `GetHotelAgent`
  - 指定された場所のホテルを取得します。
- `SubmitReservationAgent`
  - 宿泊先の予約を行います。

このサンプルでは、この Agent は 30% の確率でエラーを返すようになっています。さらに応答は、ほぼ固定の文字列を返します。実際のシステムでは Azure OpenAI Service などで提供される AI サービスを呼び出すことになります。

実際に `SubmitReservationActivity` を見てみましょう。

```csharp:SubmitReservationActivity.cs
using Azure.AI.OpenAI;
using DurableMultiAgentTemplate.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;

namespace DurableMultiAgentTemplate.Agent.SubmitReservationAgent;

public class SubmitReservationActivity(AzureOpenAIClient openAIClient, IOptions<AppConfiguration> configuration)
{
    private readonly AzureOpenAIClient _openAIClient = openAIClient;
    private readonly AppConfiguration _configuration = configuration.Value;

    [Function(AgentActivityName.SubmitReservationAgent)]
    public string Run([ActivityTrigger] SubmitReservationRequest req, FunctionContext executionContext)
    {
        // This is sample code. Replace this with your own logic.
        var result = $"""
        予約番号は {Guid.NewGuid()} です。
        --------------------------------
        ホテル名：{req.Destination}
        チェックイン日：{req.CheckIn}
        チェックアウト日：{req.CheckOut}
        人数：{req.GuestsCount} 名
        --------------------------------
        """;

        return result;
    }
}
```

他の Agent も同様な実装になっています。





------------


TDB

1. コードを眺めよう
2. localsettings.json の更新
3. オーケストレーターの実装
4. 動作確認
5. リトライの実装
6. 動作確認
