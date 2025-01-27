using DurableMultiAgentTemplate.Model;

namespace DurableMultiAgentTemplate.Client.Services;

public class AgentChatService(HttpClient httpClient)
{
    public async Task<AgentResponseDto> GetAgentResponseAsync(AgentRequestDto agentRequestDto)
    {
        var response = await httpClient.PostAsJsonAsync("invoke/sync", agentRequestDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AgentResponseDto>() ??
            throw new InvalidOperationException("The format of the response from the agent is invalid.");
    }
}
