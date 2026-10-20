using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using ValleyOpus.Diagnostics;
using ValleyOpus.Effects;
using ValleyOpus.Framework;
using ValleyOpus.Machines;
using ValleyOpus.Routes;
using ValleyOpus.Satchel;
using ValleyOpus.Skill;
using ValleyOpus.Spikes;
using ValleyOpus.State;
using ValleyOpus.UI;

namespace ValleyOpus;

/// <summary>
/// The SMAPI entry point. Test build alpha.2 (batch B2, "the bench and the line"): the adapter modules of doc 09 §3
/// behind the engine, with the remaining spike commands (S3, S9, S15).
/// </summary>
public sealed class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        var log = new SmapiLog(this.Monitor);
        ModServices.Helper = helper;
        ModServices.Monitor = this.Monitor;
        ModServices.Manifest = this.ModManifest;
        ModServices.Log = log;
        ModServices.Config = helper.ReadConfig<ModConfig>();
        log.Verbose = ModServices.Config.Verbose;

        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded; // first, so the registries load before the services read them

        ModServices.Content = new GameContent(helper, log);
        ModServices.Farm = new FarmState(helper, log);
        ModServices.Skill = new AlchemySkill(log);
        ModServices.Machines = new MachineService(log);
        ModServices.Chains = new ChainService(helper, log);
        ModServices.Satchel = new SatchelService(helper, log);
        ModServices.Effects = new EffectService(helper, log);
        ModServices.World = new World.WorldService(helper, log);
        _ = new Effects.Shades(helper, log);
        _ = new Effects.Handlers(helper, log);
        _ = new HudOverlay(helper);
        _ = new Commands(helper, this.Monitor);
        _ = new SpikeRunner(helper, this.Monitor, new ISpike[] { new BuildingSpike(), new PerfSpike(helper), new EffectsSpike() });

        var harmony = new Harmony(this.ModManifest.UniqueID);
        MachineService.Patch(harmony, this.Monitor);
        SatchelService.Patch(harmony);
        EffectService.Patch(harmony);
        ProfileTooltip.Patch(harmony);
        World.Npcs.Patch(harmony);
        Effects.Handlers.Patch(harmony);

        AlchemySkill.Register();
        Neighbours.RegisterQueries();
        World.Workshop.Register(helper);
        World.FinchWagon.Register(helper);
        World.StarterQuests.Register(helper);
        ModServices.Content.Reloaded += Neighbours.ClearCache;

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.Monitor.Log($"ValleyOpus {this.ModManifest.Version}: test build. Type 'valleyopus_dev on' then 'valleyopus_dev kit' to start, or see TESTING.md.", LogLevel.Info);
        if (this.Helper.ModRegistry.IsLoaded("Pathoschild.Automate"))
            this.Monitor.Log("Automate is installed: it runs ValleyOpus apparatus through its own generic handler (D25).", LogLevel.Trace);
        ConfigMenu.Register(this.Helper, this.ModManifest);
        // Content Patcher fills our registries a few ticks after launch, so the professions wait for them (SOS Pass A:
        // registering at launch found none).
        this.Helper.Events.GameLoop.UpdateTicked += this.RegisterProfessionsWhenReady;
    }

    private void RegisterProfessionsWhenReady(object? sender, UpdateTickedEventArgs e)
    {
        if (e.Ticks % 10 != 0)
            return;
        bool giveUp = e.Ticks >= 600;
        try
        {
            var professions = Engine.Registry.RegistryLoader.Load(GameContent.ReadRaw()).Professions.All.ToList();
            if (professions.Count == 0 && !giveUp)
            {
                // Drop a cached empty read so the next try sees Content Patcher's rows.
                this.Helper.GameContent.InvalidateCache(asset => Engine.Model.RegistryNames.All.Any(r => asset.NameWithoutLocale.IsEquivalentTo(Engine.Model.RegistryNames.AssetName(r))));
                return;
            }
            AlchemySkill.RegisterProfessions(professions, ModServices.Log);
            if (professions.Count == 0)
                this.Monitor.Log("No alchemy professions loaded ten seconds after launch; is [CP] ValleyOpus installed?", LogLevel.Warn);
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"Couldn't register the alchemy professions: {ex}", LogLevel.Error);
        }
        AlchemySkill.CreateIcons();
        this.Helper.Events.GameLoop.UpdateTicked -= this.RegisterProfessionsWhenReady;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        _ = ModServices.Content.Load();
        this.Monitor.Log($"Resolver ready in {timer.ElapsedMilliseconds} ms.", LogLevel.Info);
    }
}
