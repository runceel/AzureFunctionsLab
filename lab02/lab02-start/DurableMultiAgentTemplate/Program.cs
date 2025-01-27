using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using DurableMultiAgentTemplate.Model;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var configuration = builder.Configuration;

builder.Services.Configure<AppConfiguration>(configuration.GetSection("AppConfig"));

builder.Services
    .AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddClient<AzureOpenAIClient, AzureOpenAIClientOptions>(options =>
            {
                var endpoint = builder.Configuration["AppConfig:OpenAIEndpoint"];
                if (string.IsNullOrEmpty(endpoint)) throw new InvalidOperationException("AppConfig:OpenAIEndpoint is required.");

                if (builder.Environment.IsDevelopment())
                {
                    // Local development では API Key を使用する
                    var apiKey = builder.Configuration["AppConfig:OpenAIApiKey"];
                    if (string.IsNullOrEmpty(apiKey)) throw new InvalidOperationException("AppConfig:OpenAIApiKey is required.");
                    return new AzureOpenAIClient(
                        new Uri(endpoint),
                        new AzureKeyCredential(apiKey),
                        options);
                }
                else
                {
                    // Azure Functions では Managed Identity を使用する
                    return new AzureOpenAIClient(
                        new Uri(endpoint),
                        new DefaultAzureCredential(),
                        options);
                }
            });
        });

builder.Services.Configure<JsonSerializerOptions>(jsonSerializerOptions =>
    {
        jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        // オーケスとレーター関数のmetadata.SerializedOutputの日本語がエスケープされないように設定
        // https://learn.microsoft.com/ja-jp/dotnet/standard/serialization/system-text-json/character-encoding
        jsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All); 
    });

builder.Build().Run();
