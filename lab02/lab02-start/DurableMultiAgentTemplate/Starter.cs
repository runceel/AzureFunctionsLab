using System.Text.Json;
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
        // ここにオーケストレーターを呼び出す処理を記述します
        return req.CreateResponse(System.Net.HttpStatusCode.OK);
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
