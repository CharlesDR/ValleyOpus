namespace ValleyOpus.Engine.Logging;

/// <summary>
/// The engine's log sink. The adapter forwards it to SMAPI's monitor under the one <c>[ValleyOpus]</c> prefix;
/// tests collect it. Levels follow doc 11 §6: Info for discoveries, unlocks and migrations, Warn for disabled rows
/// and validator findings, Error for anything caught; Trace is the verbose mode.
/// </summary>
public interface ILog
{
    void Trace(string message);
    void Info(string message);
    void Warn(string message);
    void Error(string message);
}

/// <summary>A log that drops everything.</summary>
public sealed class NullLog : ILog
{
    public static readonly NullLog Instance = new();
    public void Trace(string message) { }
    public void Info(string message) { }
    public void Warn(string message) { }
    public void Error(string message) { }
}

/// <summary>A log that keeps every line, for tests and tools.</summary>
public sealed class MemoryLog : ILog
{
    public List<(string Level, string Message)> Lines { get; } = new();
    public void Trace(string message) => this.Lines.Add(("TRACE", message));
    public void Info(string message) => this.Lines.Add(("INFO", message));
    public void Warn(string message) => this.Lines.Add(("WARN", message));
    public void Error(string message) => this.Lines.Add(("ERROR", message));
}
