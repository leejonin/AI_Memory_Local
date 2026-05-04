using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ServerCommunication : MonoBehaviour
{
    // ──────────────────────────────────────────────
    //  Serialization Models
    // ──────────────────────────────────────────────

    [Serializable]
    private class DataPayload
    {
        public string time;
        public string dv_input;
        public string nin_response;
    }

    [Serializable]
    private class ResponsePayload
    {
        public bool success;
        public string message;
        public string summation;
        public string error;
    }

    /// <summary>
    /// /search request body
    /// </summary>
    [Serializable]
    private class SearchRequest
    {
        public string keyword;
    }

    /// <summary>
    /// /search response body (single match item)
    /// </summary>
    [Serializable]
    public class MemoryEntry
    {
        public string time;
        public string dv_input;
        public string nin_response;
        public string summation;
    }

    /// <summary>
    /// /search full response wrapper
    /// Unity JsonUtility cannot parse generic lists directly, 
    /// so the matches array is parsed manually (see ParseMatchesFromJson).
    /// </summary>
    [Serializable]
    public class SearchResponse
    {
        public bool success;
        public bool found;
        public string keyword;
        public string reason;
        public string gpt_answer;
    }

    /// <summary>
    /// SearchMemory callback result
    /// </summary>
    public class SearchResult
    {
        public bool success;       // HTTP communication success
        public bool found;         // Whether relevant memory exists
        public string keyword;       // Search keyword
        public string reason;        // Readable reason message
        public string gptAnswer;     // Information summarized by GPT
        public List<MemoryEntry> matches = new List<MemoryEntry>();
        public string errorMessage;  // Error details on failure
    }

    // ──────────────────────────────────────────────
    //  Inspector Fields
    // ──────────────────────────────────────────────

    [SerializeField] private string serverUrl = "http://127.0.0.1:5000";
    [SerializeField] private float dataCheckInterval = 1.0f;

    // ──────────────────────────────────────────────
    //  Internal State
    // ──────────────────────────────────────────────

    private Process pythonProcess;
    private bool serverRunning = false;
    private string pythonScriptPath;

    // ──────────────────────────────────────────────
    //  Unity Lifecycle
    // ──────────────────────────────────────────────

    void Start()
    {
        pythonScriptPath = System.IO.Path.Combine(
            Application.dataPath,
            "AI", "ForChat", "DateServer", "Sever.py"
        );

        if (!System.IO.File.Exists(pythonScriptPath))
        {
            UnityEngine.Debug.LogError($"Python script not found: {pythonScriptPath}");
            return;
        }

        StartPythonServer();
    }

    void OnApplicationQuit()
    {
        StopPythonServer();
    }

    // ──────────────────────────────────────────────
    //  Server Start / Stop
    // ──────────────────────────────────────────────

    private void StartPythonServer()
    {
        try
        {
            if (pythonProcess != null && !pythonProcess.HasExited)
                return;

            pythonProcess = new Process();
            pythonProcess.StartInfo.FileName = "python";
            pythonProcess.StartInfo.Arguments = pythonScriptPath;
            pythonProcess.StartInfo.UseShellExecute = false;
            pythonProcess.StartInfo.RedirectStandardOutput = true;
            pythonProcess.StartInfo.RedirectStandardError = true;
            pythonProcess.StartInfo.CreateNoWindow = true;

            pythonProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    UnityEngine.Debug.Log($"Python Server: {e.Data}");
            };

            pythonProcess.ErrorDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;

                var message = $"Python stderr: {e.Data}";
                if (e.Data.Contains("Traceback") || e.Data.Contains("Error") ||
                    e.Data.Contains("Exception") || e.Data.Contains("failed") || e.Data.Contains("FAILED"))
                    UnityEngine.Debug.LogError(message);
                else
                    UnityEngine.Debug.LogWarning(message);
            };

            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();

            serverRunning = true;
            UnityEngine.Debug.Log("Python Data Server Started");

            StartCoroutine(WaitForServerReady());
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Failed to start Python server: {e.Message}");
            serverRunning = false;
        }
    }

    private void StopPythonServer()
    {
        try
        {
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                pythonProcess.Kill();
                pythonProcess.WaitForExit(5000);
                pythonProcess.Dispose();
                UnityEngine.Debug.Log("Python Server Shutdown Complete");
            }
            serverRunning = false;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Error during Python server shutdown: {e.Message}");
        }
    }

    private IEnumerator WaitForServerReady()
    {
        float timeout = 10f;
        float elapsedTime = 0f;

        while (elapsedTime < timeout)
        {
            UnityWebRequest request = UnityWebRequest.Get($"{serverUrl}/health");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.Log("Server is ready!");
                yield break;
            }

            elapsedTime += 0.5f;
            yield return new WaitForSeconds(0.5f);
        }

        UnityEngine.Debug.LogWarning("Server connection timeout");
    }

    // ──────────────────────────────────────────────
    //  Data Transfer
    // ──────────────────────────────────────────────

    public void SendDataToServer(string time, string dvInput, string ninResponse)
    {
        StartCoroutine(SendDataCoroutine(time, dvInput, ninResponse));
    }

    private IEnumerator SendDataCoroutine(string time, string dvInput, string ninResponse)
    {
        UnityWebRequest request = null;
        try
        {
            var payload = new DataPayload { time = time, dv_input = dvInput, nin_response = ninResponse };
            string jsonData = JsonUtility.ToJson(payload);

            request = new UnityWebRequest($"{serverUrl}/data", "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.uploadHandler.contentType = "application/json";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Content-Length", bodyRaw.Length.ToString());

            UnityEngine.Debug.Log($"POST to {serverUrl}/data, length={bodyRaw.Length}, json={jsonData}");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Data transmission error: {e.Message}");
        }

        if (request == null) yield break;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            string responseText = TryGetResponseText(request);
            UnityEngine.Debug.LogError($"Failed to send data: {request.error} ({request.responseCode}) {responseText}");
        }
        else
        {
            string responseText = TryGetResponseText(request);
            try
            {
                var response = JsonUtility.FromJson<ResponsePayload>(responseText);
                if (response != null && !string.IsNullOrEmpty(response.summation))
                    UnityEngine.Debug.Log($"Data sent: {time} | Summary: {response.summation}");
                else
                    UnityEngine.Debug.Log($"Data sent: {time}");
            }
            catch
            {
                UnityEngine.Debug.Log($"Data sent: {time}");
            }
        }

        request.Dispose();
    }

    // ──────────────────────────────────────────────
    //  Memory Search
    // ──────────────────────────────────────────────

    /// <summary>
    /// Searches dialogue memory in YYDate.Json by keyword.
    /// </summary>
    public Task<SearchResult> SearchMemory(string keyword)
    {
        var tcs = new TaskCompletionSource<SearchResult>();
        StartCoroutine(SearchMemoryCoroutine(keyword, result => tcs.SetResult(result)));
        return tcs.Task;
    }

    public IEnumerator SearchMemoryCoroutine(string keyword, Action<SearchResult> callback)
    {
        var searchResult = new SearchResult { keyword = keyword };

        if (!serverRunning)
        {
            searchResult.success = false;
            searchResult.errorMessage = "Server is not running.";
            callback?.Invoke(searchResult);
            yield break;
        }

        string jsonBody = JsonUtility.ToJson(new SearchRequest { keyword = keyword });
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        var request = new UnityWebRequest($"{serverUrl}/search", "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.uploadHandler.contentType = "application/json";
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Accept", "application/json");
        request.SetRequestHeader("Content-Length", bodyRaw.Length.ToString());

        UnityEngine.Debug.Log($"[SearchMemory] Request: keyword=\"{keyword}\"");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            searchResult.success = false;
            searchResult.errorMessage = $"Server communication failed: {request.error} ({request.responseCode})";
            UnityEngine.Debug.LogError($"[SearchMemory] {searchResult.errorMessage}");
            request.Dispose();
            callback?.Invoke(searchResult);
            yield break;
        }

        string responseText = TryGetResponseText(request);
        request.Dispose();

        UnityEngine.Debug.Log($"[SearchMemory] Response: {responseText}");

        try
        {
            var parsed = JsonUtility.FromJson<SearchResponse>(responseText);

            searchResult.success = parsed.success;
            searchResult.found = parsed.found;
            searchResult.keyword = parsed.keyword;
            searchResult.reason = parsed.reason;
            searchResult.gptAnswer = parsed.gpt_answer ?? "";

            searchResult.matches = ParseMatchesFromJson(responseText);

            if (searchResult.found)
            {
                UnityEngine.Debug.Log(
                    $"[SearchMemory] Found -> {searchResult.reason}\n" +
                    $"GPT Summary: {searchResult.gptAnswer}\n" +
                    $"Match Count: {searchResult.matches.Count}"
                );
            }
            else
            {
                UnityEngine.Debug.Log($"[SearchMemory] No memory found -> {searchResult.reason}");
            }
        }
        catch (Exception e)
        {
            searchResult.success = false;
            searchResult.errorMessage = $"Failed to parse response: {e.Message}";
            UnityEngine.Debug.LogError($"[SearchMemory] {searchResult.errorMessage}");
        }

        callback?.Invoke(searchResult);
    }

    // ──────────────────────────────────────────────
    //  Batch Summation
    // ──────────────────────────────────────────────

    public void SendCustomData(string dvInputValue, string ninResponseValue)
    {
        string currentTime = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        SendDataToServer(currentTime, dvInputValue, ninResponseValue);
    }

    public void FillAllSummations()
    {
        StartCoroutine(FillSummationsCoroutine());
    }

    private IEnumerator FillSummationsCoroutine()
    {
        if (!serverRunning)
        {
            UnityEngine.Debug.LogWarning("Server is not running.");
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get($"{serverUrl}/fill_summations");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            UnityEngine.Debug.Log($"Batch summation fill complete: {request.downloadHandler.text}");
        else
            UnityEngine.Debug.LogError($"Failed to fill summations: {request.error}");

        request.Dispose();
    }

    // ──────────────────────────────────────────────
    //  Utilities
    // ──────────────────────────────────────────────

    public bool IsServerRunning => serverRunning;

    private static string TryGetResponseText(UnityWebRequest req)
    {
        try { return req.downloadHandler?.text ?? string.Empty; }
        catch { return string.Empty; }
    }

    private static List<MemoryEntry> ParseMatchesFromJson(string json)
    {
        var result = new List<MemoryEntry>();
        try
        {
            int matchesStart = json.IndexOf("\"matches\"", StringComparison.Ordinal);
            if (matchesStart < 0) return result;

            int arrayStart = json.IndexOf('[', matchesStart);
            if (arrayStart < 0) return result;

            int depth = 0;
            int objStart = -1;

            for (int i = arrayStart; i < json.Length; i++)
            {
                char c = json[i];
                if (c == ']' && depth == 0) break;

                if (c == '{')
                {
                    if (depth == 0) objStart = i;
                    depth++;
                }
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0 && objStart >= 0)
                    {
                        string objJson = json.Substring(objStart, i - objStart + 1);
                        var entry = JsonUtility.FromJson<MemoryEntry>(objJson);
                        if (entry != null) result.Add(entry);
                        objStart = -1;
                    }
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning($"Matches parsing warning: {e.Message}");
        }
        return result;
    }
}
