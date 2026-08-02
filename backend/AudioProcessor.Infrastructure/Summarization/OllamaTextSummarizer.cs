using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AudioProcessor.Application.Interfaces;

namespace AudioProcessor.Infrastructure.Summarization;

public class OllamaTextSummarizer : ITextSummarizer
{
    private const string Model = "llama3.2:1b";
    private readonly HttpClient _httpClient;

    public OllamaTextSummarizer(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> SummarizeAsync(string text, int maxLength)
    {
        var prompt =
            $"Resume el siguiente texto en español en un máximo de {maxLength} caracteres. " +
            $"Responde únicamente con el resumen, sin comillas ni texto adicional:\n\n{text}";

        var response = await _httpClient.PostAsJsonAsync("/api/generate", new OllamaGenerateRequest(Model, prompt, false));
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>();
        var summary = result?.Response?.Trim() ?? string.Empty;

        return summary.Length > maxLength ? summary[..maxLength] : summary;
    }

    private record OllamaGenerateRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("stream")] bool Stream);

    private record OllamaGenerateResponse(
        [property: JsonPropertyName("response")] string? Response);
}
