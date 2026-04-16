using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using DontBeLazy.Ports.Outbound.Services;

namespace DontBeLazy.Infrastructure.Services;

/// <summary>
/// Giao tiếp với API của Google Gemini (Model: gemini-1.5-flash).
/// Yêu cầu phải set biến môi trường GEMINI_API_KEY hoặc truyền config.
/// </summary>
public class GeminiAiServices : IAiTaskAssistantPort, IAiGuiltTripPort, IAiProfileAssistantPort
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiAiServices(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "MOCK_KEY_FOR_NOW";
    }

    private async Task<string> GenerateTextAsync(string prompt)
    {
        if (_apiKey == "MOCK_KEY_FOR_NOW" || string.IsNullOrEmpty(_apiKey))
        {
            return $"[AI MOCK]: Please configure GEMINI_API_KEY in environment variables. You asked: {prompt}";
        }

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}", 
            requestBody);

        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        
        using var document = JsonDocument.Parse(responseString);

        if (document.RootElement.TryGetProperty("promptFeedback", out var promptFeedback) &&
            promptFeedback.TryGetProperty("blockReason", out var blockReason))
        {
            return $"[AI_WARNING]: Cấu hình hoặc chủ đề bị cấm bởi hệ thống an toàn Google: {blockReason.GetString()}";
        }

        if (document.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var candidate = candidates[0];

            if (candidate.TryGetProperty("content", out var contentElement) && 
                contentElement.TryGetProperty("parts", out var partsElement))
            {
                var text = partsElement[0].GetProperty("text").GetString();
                return text?.Trim() ?? string.Empty;
            }

            if (candidate.TryGetProperty("finishReason", out var finishReason) && finishReason.GetString() == "SAFETY")
            {
                return "[AI_WARNING]: Bị chặn bởi Google Safety vì nhạy cảm/bạo lực.";
            }
        }

        return string.Empty;
    }

    public async Task<string> AnalyzeAndSuggestTaskPriorityAsync(string dailyTasksDump)
    {
        var prompt = $"Analyze the following task list and suggest an optimal execution order based on priority. Return only JSON format:\n\n{dailyTasksDump}";
        return await GenerateTextAsync(prompt);
    }

    public async Task<string> BreakdownTaskAsync(string taskName)
    {
        var prompt = $"Break down the following task into 3-5 subtasks:\n\nTask: {taskName}";
        return await GenerateTextAsync(prompt);
    }

    public async Task<string> GenerateGuiltTripQuoteAsync(string taskName, string language)
    {
        var prompt = $"Write a harsh, guilt-tripping quote in {language} to motivate me not to quit working on this task: '{taskName}'. Keep it under 20 words, be very direct and stern.";
        return await GenerateTextAsync(prompt);
    }

    public async Task<string> GenerateSmartProfileAsync(string intent)
    {
        var prompt = $"I want to create a focus profile for this intent: '{intent}'. Give me a JSON list of extremely distracting website domains and desktop application names that I should DEFINITELY block for this task type. Do not wrap in markdown.";
        return await GenerateTextAsync(prompt);
    }
}
