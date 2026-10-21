using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using ValleyOpus.Engine;
using ValleyOpus.Engine.Codex;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;
using ValleyOpus.Machines;
using ValleyOpus.Satchel;
using ValleyOpus.Skill;
using SObject = StardewValley.Object;

namespace ValleyOpus.Diagnostics;

/// <summary>
/// The <c>Diagnostics</c> module (doc 11 §6): inspect, verbose, validate and report for everyone; developer commands
/// behind <c>valleyopus_dev on</c> for test builds.
/// </summary>
internal sealed class Commands
{
    private readonly IMonitor monitor;
    private bool devMode;

    public Commands(IModHelper helper, IMonitor monitor)
    {
        this.monitor = monitor;
        helper.ConsoleCommands.Add("valleyopus_validate", "Prints the validator's report for every loaded ValleyOpus registry.", this.Validate);
        helper.ConsoleCommands.Add("valleyopus_verbose", "Toggles verbose logging (every resolver decision, chaining transfer and Automate load).", (_, _) =>
        {
            ModServices.Config.Verbose = ModServices.Log.Verbose = !ModServices.Log.Verbose;
            this.Info($"Verbose logging {(ModServices.Log.Verbose ? "on" : "off")}.");
        });
        helper.ConsoleCommands.Add("valleyopus_inspect", "Prints the faced or hovered machine's tray, recipe memory, pin, neighbour state, lock, fuel and kindling.", this.Inspect);
        helper.ConsoleCommands.Add("valleyopus_report", "Prints this farm's codex, skill and Satchel numbers.", this.Report);
        helper.ConsoleCommands.Add("valleyopus_dev", "Developer commands for test builds. 'valleyopus_dev on', then: kit | give <name> [count] | discover all | xp <amount> | place50 | prices | off", this.Dev);
    }

    private void Info(string message) => this.monitor.Log(message, LogLevel.Info);

    private static bool Ready(IMonitor monitor)
    {
        if (Context.IsWorldReady)
            return true;
        monitor.Log("Load a save first.", LogLevel.Error);
        return false;
    }

    private void Validate(string name, string[] args)
    {
        if (!Ready(this.monitor))
            return;
        ContentPipeline pipeline = ModServices.Content.Load();
        var r = pipeline.Registry;
        this.Info($"Registries: {r.Primes.Count} Primes, {r.Substances.Count} substances, {r.Recipes.Count} recipes, {r.Apparatus.Count} apparatus, {r.Conditions.Count} conditions, {r.Products.Count} products.");
        foreach (var finding in pipeline.Findings)
            this.Info("  " + finding);
        this.Info(pipeline.Findings.Count == 0 ? "No findings." : $"{pipeline.Findings.Count} finding(s).");
    }

    private static SObject? Target()
    {
        Vector2 cursor = Game1.currentCursorTile;
        return Game1.currentLocation.getObjectAtTile((int)cursor.X, (int)cursor.Y) is SObject hovered && ModServices.Content.ApparatusOf(hovered) != null
            ? hovered
            : Game1.currentLocation.getObjectAtTile((int)Game1.player.GetGrabTile().X, (int)Game1.player.GetGrabTile().Y);
    }

    private void Inspect(string name, string[] args)
    {
        if (!Ready(this.monitor))
            return;
        SObject? machine = Target();
        if (machine is null || ModServices.Content.ApparatusOf(machine) is not ApparatusRow apparatus)
        {
            this.Info("Face or hover a ValleyOpus apparatus first.");
            return;
        }
        var state = MachineService.State(machine);
        var snapshot = Neighbours.Read(machine, apparatus, Game1.player);
        this.Info($"{machine.DisplayName} ({apparatus.Id}) at {machine.Location?.NameOrUniqueName} {machine.TileLocation}");
        this.Info($"  tray: {string.Join(", ", MachineService.TrayItems(machine).Select(i => $"{i.QualifiedItemId} x{i.Stack} q{i.Quality}"))}");
        this.Info($"  recipe memory: {state.RecipeMemory ?? "(not set)"}; pinned: {state.Pinned}; owner: {state.Owner}");
        this.Info($"  running: {state.Recipe ?? "nothing"}; locked: {state.Locked}; held in store: {MachineService.HasHeldOutput(machine)}; extras: {Engine.Machines.MachineData.FormatExtras(state.Extras)}");
        this.Info($"  held: {machine.heldObject.Value?.QualifiedItemId ?? "nothing"}; ready: {machine.readyForHarvest.Value}; minutes left: {machine.MinutesUntilReady}");
        this.Info($"  neighbour state: {snapshot.NeighbourState ?? "(none)"}; kindled: {state.Kindled}; fuel: {state.Fuel}; neighbours with tags: {snapshot.Neighbours.Count}");
        foreach ((string query, bool value) in snapshot.Queries)
            this.Info($"  query {query}: {value}");
    }

    private static void LearnRecipe(Farmer who, string id)
    {
        if (!who.craftingRecipes.ContainsKey(id))
            who.craftingRecipes.Add(id, 0);
    }

    private void Report(string name, string[] args)
    {
        if (!Ready(this.monitor))
            return;
        var codex = ModServices.Farm.Codex;
        var registry = ModServices.Content.Registry;
        int recipes = registry.Recipes.All.Count(r => codex.IsDiscovered(r.Id));
        int tried = codex.Data.Keys.Keys.Count(k => k.StartsWith(CodexKeys.TriedPrefix, StringComparison.Ordinal));
        Farmer who = Game1.player;
        this.Info($"Codex: {recipes} of {registry.Recipes.Count} recipes discovered, {tried} tried pairings, {codex.Data.Keys.Count} keys in all.");
        this.Info($"Alchemy: level {ModServices.Skill.Level(who)}, {SpaceCore.Skills.GetExperienceFor(who, AlchemySkill.SkillId)} XP.");
        this.Info($"Satchel: {ModServices.Satchel.Inventory(who).Count(i => i != null)} of {ModServices.Satchel.Capacity(who)} types.");
    }

    private void Dev(string name, string[] args)
    {
        string command = args.FirstOrDefault() ?? "";
        if (command == "on" || command == "off")
        {
            this.devMode = command == "on";
            this.Info($"Developer commands {(this.devMode ? "on" : "off")}.");
            return;
        }
        if (!this.devMode)
        {
            this.Info("Developer commands are off. Type 'valleyopus_dev on' first (test builds only: they can spoil a real playthrough).");
            return;
        }
        if (!Ready(this.monitor))
            return;

        Farmer who = Game1.player;
        switch (command)
        {
            case "kit":
                foreach (string itemId in new[] { SatchelService.SatchelItemId, SatchelService.LensItemId })
                    who.addItemByMenuIfNecessary(ItemRegistry.Create(itemId));
                foreach (string local in new[] { "ExperimentTable", "Athanor", "Alembic", "Retort", "HermeticVessel", "Calcinator" })
                {
                    who.addItemByMenuIfNecessary(ItemRegistry.Create("(BC)" + Ids.RowPrefix + local));
                    LearnRecipe(who, Ids.RowPrefix + local);
                }
                LearnRecipe(who, Ids.RowPrefix + "Satchel");
                LearnRecipe(who, Ids.RowPrefix + "Lens");
                who.addItemByMenuIfNecessary(ItemRegistry.Create("(O)" + Ids.RowPrefix + "Ignis", 4));
                who.addItemByMenuIfNecessary(ItemRegistry.Create("(O)382", 10));
                this.Info("Gave the slice kit: Satchel, Lens, the six apparatus (recipes learned), 4 Ignis and 10 Coal.");
                break;

            case "give" when args.Length >= 2:
                int count = args.Length >= 3 && int.TryParse(args[2], out int n) ? n : 1;
                string id = args[1].StartsWith('(') ? args[1] : "(O)" + Ids.RowPrefix + args[1];
                Item? item = ItemRegistry.Create(id, count, allowNull: true);
                if (item is null)
                    this.Info($"No item {id}.");
                else
                {
                    who.addItemByMenuIfNecessary(item);
                    this.Info($"Gave {count} {item.DisplayName}.");
                }
                break;

            case "discover" when args.ElementAtOrDefault(1) == "all":
                List<string> keys = ModServices.Content.Registry.Recipes.All.Select(r => CodexKeys.Recipe(r.Id)).ToList();
                this.Info($"Recorded {ModServices.Farm.Record(who, keys).Count} recipe discoveries (no XP).");
                break;

            case "xp" when args.Length >= 2 && int.TryParse(args[1], out int amount):
                SpaceCore.Skills.AddExperience(who, AlchemySkill.SkillId, amount);
                this.Info($"Alchemy: level {ModServices.Skill.Level(who)}, {SpaceCore.Skills.GetExperienceFor(who, AlchemySkill.SkillId)} XP.");
                break;

            case "place50":
                this.Info($"Placed {PlaceFifty(who)} Retorts with chests below them.");
                break;

            case "prices":
                foreach ((string itemId, int price) in ModServices.Content.Prices.OrderBy(p => p.Key))
                    this.Info($"  {itemId}: {price}g");
                break;

            default:
                this.Info("Unknown dev command. Try: kit | give <name> [count] | discover all | xp <amount> | place50 | prices | off");
                break;
        }
    }

    /// <summary>
    /// Places up to 50 Retorts in rows near the player, each with a chest of Terra below it and its recipe memory
    /// set to Saxum (discovered), so chaining or Automate keeps them busy (S9, doc 09 §8).
    /// </summary>
    public static int PlaceFifty(Farmer who)
    {
        GameLocation location = who.currentLocation;
        string retort = "(BC)" + Ids.RowPrefix + "Retort";
        string saxum = Ids.RowPrefix + "Saxum";
        ModServices.Farm.Record(who, new[] { CodexKeys.Recipe(saxum) });
        int placed = 0;
        Vector2 start = who.Tile + new Vector2(-12, 2);
        for (int row = 0; row < 5 && placed < 50; row++)
        {
            for (int col = 0; col < 10 && placed < 50; col++)
            {
                var tile = start + new Vector2(col * 2, row * 3);
                var below = tile + new Vector2(0, 1);
                if (!location.CanItemBePlacedHere(tile) || !location.CanItemBePlacedHere(below))
                    continue;
                var machine = (SObject)ItemRegistry.Create(retort);
                machine.TileLocation = tile;
                location.objects.Add(tile, machine);
                var state = MachineService.State(machine);
                state.RecipeMemory = saxum;
                state.Owner = who.UniqueMultiplayerID;
                MachineService.Save(machine, state);
                var chest = new Chest(playerChest: true, below);
                chest.Items.Add(ItemRegistry.Create("(O)" + Ids.RowPrefix + "Terra", 200));
                location.objects.Add(below, chest);
                placed++;
            }
        }
        return placed;
    }
}
