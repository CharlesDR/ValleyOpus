using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using ValleyOpus.Engine;
using ValleyOpus.Framework;
using ValleyOpus.Spikes;

namespace ValleyOpus;

/// <summary>
/// The SMAPI entry point. Test build alpha.1 (batch B1) holds the registry assets, the in-game validator and the
/// spike commands; the adapter modules of doc 09 §3 arrive in batch B2.
/// </summary>
public sealed class ModEntry : Mod
{
    private SmapiLog log = null!;
    private GameContent content = null!;
    private AutomateSpike automate = null!;

    public override void Entry(IModHelper helper)
    {
        this.log = new SmapiLog(this.Monitor);
        this.content = new GameContent(helper, this.log);

        var harmony = new Harmony(this.ModManifest.UniqueID);
        MachineSpike.Patch(harmony, this.Monitor);
        SkillSpike.Register();

        this.automate = new AutomateSpike(helper, harmony, this.Monitor);
        _ = new SpikeRunner(helper, this.Monitor, new ISpike[]
        {
            new MachineSpike(), this.automate, new BuildingSpike(), new HoldUpSpike(helper), new SortSpike(),
            new SatchelSpike(), new WarpSpike(helper), new SkillSpike(helper), new CategorySpike(), new RobinMenuSpike(helper),
        });

        helper.ConsoleCommands.Add("valleyopus_validate", "Reads every ValleyOpus registry from the loaded content packs and prints the validator's report.", this.OnValidate);
        helper.ConsoleCommands.Add("valleyopus_verbose", "Toggles verbose logging (every resolver decision at Debug level).", (_, _) =>
        {
            this.log.Verbose = !this.log.Verbose;
            this.Monitor.Log($"Verbose logging {(this.log.Verbose ? "on" : "off")}.", LogLevel.Info);
        });

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.Monitor.Log($"ValleyOpus {this.ModManifest.Version}: test build with spike commands. Type 'valleyopus_spike list'.", LogLevel.Info);
        this.automate.Register();
        SkillSpike.CreateIcons();
    }

    private void OnValidate(string name, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save first, so every content pack's edits are applied.", LogLevel.Error);
            return;
        }

        ContentPipeline pipeline = this.content.Load();
        var r = pipeline.Registry;
        this.Monitor.Log($"Registries: {r.Primes.Count} Primes, {r.Substances.Count} substances, {r.Recipes.Count} recipes, {r.Apparatus.Count} apparatus, {r.Conditions.Count} conditions.", LogLevel.Info);
        foreach (var finding in pipeline.Findings)
            this.Monitor.Log("  " + finding, LogLevel.Info);
        this.Monitor.Log(pipeline.Findings.Count == 0 ? "No findings." : $"{pipeline.Findings.Count} finding(s).", LogLevel.Info);
    }
}
