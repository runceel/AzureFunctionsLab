using DurableMultiAgentTemplate.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableMultiAgentTemplate.Agent.Orchestrator;

public class AgentOrchestrator()
{
    // リトライを構成した TaskOptions を定義
    private static TaskOptions DefaultTaskOptions { get; } = new(
    new TaskRetryOptions(new RetryPolicy(
        3,
        TimeSpan.FromSeconds(1),
        1,
        TimeSpan.FromSeconds(10))));

    [Function(nameof(AgentOrchestrator))]
    public async Task<AgentResponseDto> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        // オーケストレーターで正しくログが出力されるようにするため ReplaySafeLogger を作成
        ILogger logger = context.CreateReplaySafeLogger("AgentOrchestrator");
        var reqData = context.GetInput<AgentRequestDto>();

        ArgumentNullException.ThrowIfNull(reqData);

        // AgentDecider呼び出し（呼び出すAgentの決定）
        var agentDeciderResult = await context.CallActivityAsync<AgentDeciderResult>(AgentActivityName.AgentDeciderActivity, reqData, DefaultTaskOptions);

        // AgentDeciderでエージェントを呼び出さない場合には、そのまま返す
        if (!agentDeciderResult.IsAgentCall)
        {
            logger.LogInformation("No agent call happened");
            return new AgentResponseDto { Content = agentDeciderResult.Content };
        }

        // AgentDeciderが選択したエージェントを並列で呼び出す
        logger.LogInformation("Agent call happened");
        var parallelAgentCall = new List<Task<string>>();
        foreach (var agentCall in agentDeciderResult.AgentCalls)
        {
            var args = agentCall.Arguments;
            parallelAgentCall.Add(context.CallActivityAsync<string>(agentCall.AgentName, args, DefaultTaskOptions));
        }

        await Task.WhenAll(parallelAgentCall);

        // Synthesizer呼び出し（回答集約）
        SynthesizerRequest synthesizerRequest = new()
        {
            AgentCallResult = parallelAgentCall.Select(x => x.Result).ToList(),
            AgentReques = reqData,
            CalledAgentNames = agentDeciderResult.AgentCalls.Select(x => x.AgentName).ToList()
        };
        return await context.CallActivityAsync<AgentResponseDto>(AgentActivityName.SynthesizerActivity, synthesizerRequest, DefaultTaskOptions);
    }
}
