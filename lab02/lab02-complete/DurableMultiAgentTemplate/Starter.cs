using System.Text.Json;
using Castle.Core.Logging;
using DurableMultiAgentTemplate.Agent.Orchestrator;
using DurableMultiAgentTemplate.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace DurableMultiAgentTemplate;

public class Starter(ILogger<Starter> logger)
{
    [Function("SyncStarter")]
    public async Task<HttpResponseData> SyncStarter(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "invoke/sync")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        logger.LogInformation("Sync HTTP trigger function processed a request.");
        // リクエスト データを取得
        var reqData = await GetRequestData(req);

        if (reqData == null)
        {
            // リクエスト データがない場合は、BadRequest を返す
            var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Please pass a prompt in the request body");
            return badRequestResponse;
        }

        // AgentOrchestrator オーケストレーションを開始
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(AgentOrchestrator), reqData);

        logger.LogInformation($"Started orchestration with ID = '{instanceId}'.");

        // インスタンスの完了を待機し、結果を取得
        var metadata = await client.WaitForInstanceCompletionAsync(instanceId, getInputsAndOutputs: true);

        // レスポンスを作成して返却
        var res = HttpResponseData.CreateResponse(req);
        await res.WriteStringAsync(metadata.SerializedOutput ?? "");
        return res;
    }

    private async Task<AgentRequestDto?> GetRequestData(HttpRequestData req)
    {
        var requestBody = await req.ReadAsStringAsync();

        if (string.IsNullOrEmpty(requestBody)) return null;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var reqData = JsonSerializer.Deserialize<AgentRequestDto>(requestBody, options);

        if (reqData == null) return null;
        if (reqData.Messages == null || !reqData.Messages.Any()) return null;
        return reqData;
    }
}
