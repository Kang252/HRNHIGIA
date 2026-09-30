using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using NHIGIA.Modern.Models;

namespace NHIGIA.Modern.Infrastructure;

public interface IOpenCvFaceMatcher
{
    bool IsEnabled { get; }
    Task<FaceMatchResult> CompareAsync(byte[] reference, byte[] candidate, CancellationToken cancellationToken);
}

// Local opt-in comparison only. A match is never evidence of liveness or an attendance decision.
public sealed class OpenCvFaceMatcher : IOpenCvFaceMatcher, IDisposable
{
    private readonly SemaphoreSlim _slot = new(1, 1);
    private readonly string _python;
    private readonly string _script;
    private readonly string _modelDirectory;
    private readonly double _threshold;
    public bool IsEnabled { get; }

    public OpenCvFaceMatcher(IConfiguration configuration, IWebHostEnvironment environment)
    {
        IsEnabled = string.Equals(configuration["HRM_OPENCV_ENABLED"], "true", StringComparison.OrdinalIgnoreCase)
            || configuration["HRM_OPENCV_ENABLED"] == "1";
        _python = configuration["HRM_OPENCV_PYTHON"] ?? (OperatingSystem.IsWindows() ? "python" : "python3");
        _script = Path.Combine(environment.ContentRootPath, "OpenCv", "worker.py");
        _modelDirectory = configuration["HRM_OPENCV_MODEL_PATH"] ?? Path.Combine(environment.ContentRootPath, "OpenCv", "models");
        _threshold = double.TryParse(configuration["HRM_OPENCV_THRESHOLD"], NumberStyles.Float, CultureInfo.InvariantCulture, out var threshold)
            && double.IsFinite(threshold) && threshold is >= .3 and <= .9 ? threshold : .45;
    }

    public async Task<FaceMatchResult> CompareAsync(byte[] reference, byte[] candidate, CancellationToken cancellationToken)
    {
        if (!IsEnabled) return Failure("DISABLED");
        if (!HrmDataStore.IsValidFaceEnrollmentPhoto(reference) || !HrmDataStore.IsValidFaceEnrollmentPhoto(candidate))
            return Failure("INVALID_IMAGE");
        if (cancellationToken.IsCancellationRequested) return Failure("CANCELLED");
        if (!await _slot.WaitAsync(0, cancellationToken)) return Failure("BUSY");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(12));
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _python, UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
                StandardInputEncoding = new UTF8Encoding(false), StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            }
        };
        process.StartInfo.ArgumentList.Add("-u");
        process.StartInfo.ArgumentList.Add(_script);
        process.StartInfo.ArgumentList.Add("--model-dir");
        process.StartInfo.ArgumentList.Add(_modelDirectory);
        process.StartInfo.Environment["OPENCV_LOG_LEVEL"] = "SILENT";
        process.StartInfo.Environment["OMP_NUM_THREADS"] = "1";
        process.StartInfo.Environment["OPENBLAS_NUM_THREADS"] = "1";
        try
        {
            if (!File.Exists(_script) || !process.Start()) return Failure("UNAVAILABLE");
            // Never put images in arguments, files or logs; the child gets only this bounded stdin payload.
            var stdout = ReadLimitedAsync(process.StandardOutput, timeout.Token);
            var stderr = DrainAsync(process.StandardError, timeout.Token);
            var request = JsonSerializer.Serialize(new
            {
                Reference = Convert.ToBase64String(reference), Candidate = Convert.ToBase64String(candidate), Threshold = _threshold
            });
            await process.StandardInput.WriteLineAsync(request.AsMemory(), timeout.Token);
            process.StandardInput.Close();
            await process.WaitForExitAsync(timeout.Token);
            var output = await stdout;
            await stderr;
            if (process.ExitCode != 0) return Failure("UNAVAILABLE");
            var result = JsonSerializer.Deserialize<FaceMatchResult>(output);
            if (result == null || !FaceMatchResult.IsTerminal(result.Status) && result.Status != "UNAVAILABLE"
                || result.Score.HasValue && (!double.IsFinite(result.Score.Value) || result.Score is < -1 or > 1)
                || result.ModelVersion?.Length > 128 || result.DetailCode?.Length > 80)
                return Failure("UNAVAILABLE");
            if (result.Status is "MATCH" or "NO_MATCH")
            {
                if (!result.Score.HasValue || string.IsNullOrWhiteSpace(result.ModelVersion)
                    || (result.Status == "MATCH") != (result.Score >= _threshold)) return Failure("UNAVAILABLE");
            }
            result.Threshold = _threshold;
            return result;
        }
        catch (OperationCanceledException) { return Failure(cancellationToken.IsCancellationRequested ? "CANCELLED" : "TIMEOUT"); }
        catch (Exception error) when (error is IOException or InvalidOperationException or System.ComponentModel.Win32Exception or JsonException)
        {
            return Failure("UNAVAILABLE");
        }
        finally
        {
            try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) { }
            catch (System.ComponentModel.Win32Exception) { }
            _slot.Release();
        }
    }

    private FaceMatchResult Failure(string status) => new() { Status = status, Threshold = _threshold };

    private static async Task<string> ReadLimitedAsync(StreamReader reader, CancellationToken token)
    {
        var builder = new StringBuilder();
        var buffer = new char[1024];
        int count;
        while ((count = await reader.ReadAsync(buffer.AsMemory(), token)) > 0)
        {
            if (builder.Length + count > 8192) throw new IOException("Worker output exceeds limit.");
            builder.Append(buffer, 0, count);
        }
        return builder.ToString();
    }

    private static async Task DrainAsync(StreamReader reader, CancellationToken token)
    {
        var buffer = new char[1024];
        while (await reader.ReadAsync(buffer.AsMemory(), token) > 0) { }
    }

    public void Dispose() => _slot.Dispose();
}
