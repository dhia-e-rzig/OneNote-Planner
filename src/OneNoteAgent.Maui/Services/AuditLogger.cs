using System.Collections.Concurrent;
using OneNoteAgent.Maui.Interfaces;
using OneNoteAgent.Maui.Models;

namespace OneNoteAgent.Maui.Services;

/// <summary>
/// In-memory audit logger for operations.
/// Logs operation metadata only.
/// Thread-safe implementation using ConcurrentQueue.
/// </summary>
public sealed class AuditLogger : IAuditLogger
{
    private const int MaxEntries = 1000;
    private readonly ConcurrentQueue<AuditLogEntry> _entries = new();
    private readonly SemaphoreSlim _trimLock = new(1, 1);

    public Task LogOperationAsync(
        AuditOperation operation,
        CancellationToken cancellationToken = default)
    {
        var entry = new AuditLogEntry(
            Id: Guid.NewGuid(),
            Timestamp: DateTimeOffset.UtcNow,
            Operation: operation,
            Success: true,
            ErrorMessage: null);

        EnqueueAndTrim(entry);
        return Task.CompletedTask;
    }

    public Task LogToolOperationAsync(
        AuditOperation operation,
        string toolName,
        string? toolInput = null,
        string? toolOutput = null,
        CancellationToken cancellationToken = default)
    {
        var entry = new AuditLogEntry(
            Id: Guid.NewGuid(),
            Timestamp: DateTimeOffset.UtcNow,
            Operation: operation,
            Success: true,
            ErrorMessage: null,
            ToolName: toolName,
            ToolInput: toolInput,
            ToolOutput: toolOutput);

        EnqueueAndTrim(entry);
        return Task.CompletedTask;
    }

    public Task LogFailureAsync(
        AuditOperation operation,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        var entry = new AuditLogEntry(
            Id: Guid.NewGuid(),
            Timestamp: DateTimeOffset.UtcNow,
            Operation: operation,
            Success: false,
            ErrorMessage: errorMessage);

        EnqueueAndTrim(entry);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditLogEntry>> GetRecentEntriesAsync(
        int count = 50,
        CancellationToken cancellationToken = default)
    {
        var entries = _entries
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .ToList();

        return Task.FromResult<IReadOnlyList<AuditLogEntry>>(entries);
    }

    public Task ClearLogsAsync(CancellationToken cancellationToken = default)
    {
        _entries.Clear();
        return Task.CompletedTask;
    }

    private void EnqueueAndTrim(AuditLogEntry entry)
    {
        _entries.Enqueue(entry);

        // Trim if over limit (non-blocking best effort)
        if (_entries.Count > MaxEntries && _trimLock.Wait(0))
        {
            try
            {
                while (_entries.Count > MaxEntries)
                {
                    _entries.TryDequeue(out _);
                }
            }
            finally
            {
                _trimLock.Release();
            }
        }
    }
}
