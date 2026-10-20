using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Quests;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;
using ValleyOpus.Machines;
using SObject = StardewValley.Object;

namespace ValleyOpus.World;

/// <summary>
/// Oriel's starter chain (doc 11, Charles 2026-10-18): five vanilla quests, each appearing when the last is done, each
/// rewarding the material for the next step. Goals are remembered as they happen, so a quest whose goal is already met
/// completes the moment it appears and nobody waits on the log. Per player (each screen runs its own chain).
/// </summary>
internal static class StarterQuests
{
    private const string GoalsKey = Ids.KeyPrefix + "StarterGoals";
    private const string StartedKey = Ids.KeyPrefix + "StarterStarted";

    /// <summary>Quest local ID → reward (mirrors STARTER_QUESTS in tools/basepack/world_data.py).</summary>
    private static readonly (string Quest, string Reward, int Count)[] Chain =
    {
        ("Q1", "(O)" + Ids.RowPrefix + "Ignis", 4),
        ("Q2", "(O)" + Ids.RowPrefix + "Aqua", 4),
        ("Q3", "(O)382", 5),
        ("Q4", "(O)338", 10),
        ("Q5", "(O)382", 10),
    };

    public static void Register(IModHelper helper)
    {
        MachineService.RunStarted += OnRunStarted;
        MachineService.Discovered += (who, _) => Meet(who, "Q2");
        helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecond;
    }

    private static void OnRunStarted(SObject machine, string recipeId, Farmer who)
    {
        string local = machine.ItemId.StartsWith(Ids.RowPrefix, StringComparison.Ordinal) ? machine.ItemId[Ids.RowPrefix.Length..] : machine.ItemId;
        if (local == "Athanor" && recipeId == Ids.DecomposeRecipe)
            Meet(who, "Q1");
        if (local == "Alembic")
            Meet(who, "Q3");
        if (machine.Location?.ParentBuilding?.buildingType.Value?.StartsWith(Ids.RowPrefix + "LabT", StringComparison.Ordinal) == true)
            Meet(who, "Q5");
    }

    private static HashSet<string> Goals(Farmer who)
        => who.modData.TryGetValue(GoalsKey, out string? raw) && raw.Length > 0 ? new HashSet<string>(raw.Split('|'), StringComparer.Ordinal) : new HashSet<string>(StringComparer.Ordinal);

    private static void Meet(Farmer who, string goal)
    {
        if (!who.IsLocalPlayer)
            return;
        HashSet<string> goals = Goals(who);
        if (goals.Add(goal))
        {
            who.modData[GoalsKey] = string.Join("|", goals);
            ModServices.Log.Trace($"Starter chain: {who.Name} met goal {goal}.");
        }
    }

    private static void OnOneSecond(object? sender, OneSecondUpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady || !Context.IsPlayerFree || ModServices.World is null)
            return;
        try
        {
            Farmer who = Game1.player;
            if (!ModServices.World.IsUnlocked(WorldService.IntroUnlock))
                return;
            if (ModServices.World.IsRestored("FurnaceRoom"))
                Meet(who, "Q4");
            if (!who.modData.ContainsKey(StartedKey))
            {
                who.modData[StartedKey] = "1";
                who.addQuest(Ids.RowPrefix + Chain[0].Quest);
                return;
            }
            HashSet<string> goals = Goals(who);
            for (int i = 0; i < Chain.Length; i++)
            {
                string id = Ids.RowPrefix + Chain[i].Quest;
                if (who.questLog.FirstOrDefault(q => q.id.Value == id) is not Quest quest || quest.completed.Value || !goals.Contains(Chain[i].Quest))
                    continue;
                quest.questComplete();
                who.addItemByMenuIfNecessary(ItemRegistry.Create(Chain[i].Reward, Chain[i].Count));
                ModServices.Log.Info($"Starter chain: {who.Name} finished {Chain[i].Quest}.");
                if (i + 1 < Chain.Length)
                    who.modData[Ids.KeyPrefix + "StarterNext"] = Chain[i + 1].Quest;
            }
            // The next quest appears once the last is done; Tobin's (the last) waits for him to arrive.
            if (who.modData.TryGetValue(Ids.KeyPrefix + "StarterNext", out string? next) && next.Length > 0
                && (next != "Q5" || ModServices.World.IsUnlocked(WorldService.Full("TobinArrives"))))
            {
                who.modData[Ids.KeyPrefix + "StarterNext"] = "";
                who.addQuest(Ids.RowPrefix + next);
            }
        }
        catch (Exception ex)
        {
            ModServices.Log.Error($"The starter chain failed: {ex}");
        }
    }
}
