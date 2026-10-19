using StardewModdingAPI;
using StardewValley;

namespace ValleyOpus.Spikes;

/// <summary>One platform question from doc 09 §14, set up and checked by <c>valleyopus_spike &lt;id&gt;</c>.</summary>
internal interface ISpike
{
    string Id { get; }

    /// <summary>One line for <c>valleyopus_spike list</c>.</summary>
    string Summary { get; }

    void Run(SpikeLog log, string[] args);
}

/// <summary>Writes spike results as <c>[S11] PASS ...</c>, <c>FAIL</c> or <c>LOOK</c> (when only Charles's eyes can judge).</summary>
internal sealed class SpikeLog
{
    private readonly IMonitor monitor;
    private readonly string id;

    public SpikeLog(IMonitor monitor, string id)
    {
        this.monitor = monitor;
        this.id = id;
    }

    public void Pass(string message) => this.monitor.Log($"[{this.id}] PASS {message}", LogLevel.Info);
    public void Fail(string message) => this.monitor.Log($"[{this.id}] FAIL {message}", LogLevel.Warn);
    public void Look(string message) => this.monitor.Log($"[{this.id}] LOOK {message}", LogLevel.Info);
    public void Note(string message) => this.monitor.Log($"[{this.id}] {message}", LogLevel.Info);
    public void Error(string message) => this.monitor.Log($"[{this.id}] ERROR {message}", LogLevel.Error);
}

/// <summary>The <c>valleyopus_spike</c> console command (doc 22 §3, spike build only).</summary>
internal sealed class SpikeRunner
{
    private readonly IMonitor monitor;
    private readonly Dictionary<string, ISpike> spikes;

    public SpikeRunner(IModHelper helper, IMonitor monitor, IEnumerable<ISpike> spikes)
    {
        this.monitor = monitor;
        this.spikes = spikes.ToDictionary(s => s.Id, StringComparer.OrdinalIgnoreCase);
        helper.ConsoleCommands.Add("valleyopus_spike", "Runs a ValleyOpus spike test. Usage: valleyopus_spike list | valleyopus_spike <id> [args]. See TESTING.md.", this.OnCommand);
    }

    private void OnCommand(string name, string[] args)
    {
        if (args.Length == 0 || args[0].Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            foreach (ISpike spike in this.spikes.Values.OrderBy(s => int.Parse(s.Id.Substring(1))))
                this.monitor.Log($"{spike.Id,-4} {spike.Summary}", LogLevel.Info);
            return;
        }

        if (!this.spikes.TryGetValue(args[0], out ISpike? found))
        {
            this.monitor.Log($"No spike '{args[0]}'. Try 'valleyopus_spike list'.", LogLevel.Error);
            return;
        }
        if (!Context.IsWorldReady)
        {
            this.monitor.Log("Load a save first.", LogLevel.Error);
            return;
        }

        var log = new SpikeLog(this.monitor, found.Id);
        try
        {
            found.Run(log, args.Skip(1).ToArray());
        }
        catch (Exception ex)
        {
            log.Error($"crashed on day {Game1.Date}: {ex}");
        }
    }

    /// <summary>The tile the player faces, for spikes that place something.</summary>
    public static Microsoft.Xna.Framework.Vector2 FacingTile() => Game1.player.GetGrabTile();
}
