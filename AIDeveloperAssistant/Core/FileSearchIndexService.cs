using AIDeveloperAssistant.Models;
using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AIDeveloperAssistant.Core;

internal class FileSearchIndexService
{
    private readonly string _endpoint;
    private readonly HttpClient _httpClient = new();
    private readonly string _statePath;// = Path.GetFullPath("Data\\file-search-state.json");

    public FileSearchIndexService()
    {
        string projectRoot =
    Directory.GetParent(AppContext.BaseDirectory)!
             .Parent!
             .Parent!
             .Parent!
             .FullName;

        _statePath =
            Path.Combine(projectRoot,
                         "Data",
                         "file-search-state.json");

        _endpoint = AppConfig.Instance.Endpoint.TrimEnd('/');
    }

    public async Task<string> GetOrCreateVectorStoreAsync(string pdfPath)
    {
        Directory.CreateDirectory("Data");

        FileSearchState state = LoadState();

        if (!string.IsNullOrWhiteSpace(state.VectorStoreId))
        {
            return state.VectorStoreId;
        }

        await AddAuthHeaderAsync();

        string fileId = await UploadFileAsync(pdfPath);
        string vectorStoreId = await CreateVectorStoreAsync(fileId);

        state.FileId = fileId;
        state.VectorStoreId = vectorStoreId;

        SaveState(state);

        return vectorStoreId;
    }

    private async Task AddAuthHeaderAsync()
    {
        DefaultAzureCredential credential = new();

        AccessToken token = await credential.GetTokenAsync(
            new TokenRequestContext(["https://ai.azure.com/.default"]));

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Token);
    }

    private async Task<string> UploadFileAsync(string filePath)
    {
        using MultipartFormDataContent form = new();

        form.Add(new StringContent("assistants"), "purpose");

        byte[] fileBytes = await File.ReadAllBytesAsync(filePath);

        ByteArrayContent fileContent = new(fileBytes);
        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue("application/pdf");

        form.Add(fileContent, "file", Path.GetFileName(filePath));

        HttpResponseMessage response =
            await _httpClient.PostAsync($"{_endpoint}/files", form);

        string json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"File upload failed: {json}");
        }

        using JsonDocument document = JsonDocument.Parse(json);

        return document.RootElement.GetProperty("id").GetString()
            ?? throw new Exception("File id not found.");
    }

    private async Task<string> CreateVectorStoreAsync(string fileId)
    {
        var body = new
        {
            name = "AI Developer Assistant Docs",
            file_ids = new[] { fileId }
        };

        string jsonBody = JsonSerializer.Serialize(body);

        HttpResponseMessage response =
            await _httpClient.PostAsync(
                $"{_endpoint}/vector_stores",
                new StringContent(jsonBody, Encoding.UTF8, "application/json"));

        string json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Vector store creation failed: {json}");
        }

        using JsonDocument document = JsonDocument.Parse(json);

        var vectorStoreId = document.RootElement.GetProperty("id").GetString()
            ?? throw new Exception("Vector store id not found.");

        await WaitForVectorStoreReadyAsync(vectorStoreId);

        return vectorStoreId;

    }

    private FileSearchState LoadState()
    {
        if (!File.Exists(_statePath))
        {
            return new FileSearchState();
        }

        string json = File.ReadAllText(_statePath);

        return JsonSerializer.Deserialize<FileSearchState>(json)
            ?? new FileSearchState();
    }

    private void SaveState(FileSearchState state)
    {
        string json = JsonSerializer.Serialize(
            state,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(_statePath, json);
    }

    private async Task WaitForVectorStoreReadyAsync(string vectorStoreId)
    {
        while (true)
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync(
                    $"{_endpoint}/vector_stores/{vectorStoreId}");

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            using JsonDocument document = JsonDocument.Parse(json);

            string status =
                document.RootElement.GetProperty("status").GetString()!;

            Console.WriteLine($"Status: {status}");

            if (status.Equals("completed",
                StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Vector Store is ready.");
                return;
            }

            if (status.Equals("failed",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Vector Store indexing failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}