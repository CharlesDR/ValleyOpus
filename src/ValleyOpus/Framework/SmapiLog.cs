using StardewModdingAPI;
using ValleyOpus.Engine.Logging;

namespace ValleyOpus.Framework;

/// <summary>
/// Forwards the engine's log to SMAPI. SMAPI prefixes every line with the mod name, which gives the one
/// <c>[ValleyOpus]</c> prefix of doc 11 §6; Trace lines are the verbose mode.
/// </summary>
internal sealed class SmapiLog : ILog
{
    private readonly IMonitor monitor;

    public SmapiLog(IMonitor monitor) => this.monitor = monitor;

    public bool Verbose { get; set; }

    public void Trace(string message) => this.monitor.Log(message, this.Verbose ? LogLevel.Debug : LogLevel.Trace);
    public void Info(string message) => this.monitor.Log(message, LogLevel.Info);
    public void Warn(string message) => this.monitor.Log(message, LogLevel.Warn);
    public void Error(string message) => this.monitor.Log(message, LogLevel.Error);
}
