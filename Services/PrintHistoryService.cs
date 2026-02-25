using System.Text.Json;
using EposBridge.Models;

namespace EposBridge.Services;

public class PrintHistoryService
{
    private const int MaxHistory = 50;
    private readonly string _filePath;
    private List<PrintJob> _history = new();
    private readonly object _lock = new();

    public PrintHistoryService()
    {
        string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EposBridge");
        Directory.CreateDirectory(appData);
        _filePath = Path.Combine(appData, "history.json");
        LoadHistory();
    }

    public void AddJob(PrintJob job)
    {
        lock (_lock)
        {
            // Add to top
            _history.Insert(0, job);

            // FIFO: Remove oldest if over limit
            while (_history.Count > MaxHistory)
            {
                _history.RemoveAt(_history.Count - 1);
            }

            SaveHistory();
        }
    }

    public List<PrintJob> GetHistory()
    {
        lock (_lock)
        {
            return new List<PrintJob>(_history);
        }
    }

    public PrintJob? GetJob(string id)
    {
        lock (_lock)
        {
            return _history.FirstOrDefault(j => j.Id == id);
        }
    }

    private void LoadHistory()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                _history = JsonSerializer.Deserialize<List<PrintJob>>(json) ?? new List<PrintJob>();
            }
        }
        catch { /* Ignore load errors */ }
    }

    private void SaveHistory()
    {
        try
        {
            string json = JsonSerializer.Serialize(_history, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch { /* Ignore save errors */ }
    }
}
