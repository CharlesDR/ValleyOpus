using HarmonyLib;
using StardewValley;
using ValleyOpus.Engine.Model;
using ValleyOpus.Framework;

namespace ValleyOpus.World;

/// <summary>
/// Our villagers' extra talk (doc 08a, doc 14 §4): once an NPC has said their line for the day, talking again with
/// empty hands opens their shop, Dr. Sorenson's contracts, or one of their hints (Oriel, the Curator, Hesper,
/// Delta: doc 07 hint channels), at most one hint per NPC per day. Gifts and the day's first line stay vanilla.
/// </summary>
internal static class Npcs
{
    public const string Sorenson = Ids.RowPrefix + "Sorenson";
    private const string HintDayKey = Ids.KeyPrefix + "HintDay_";

    public static void Patch(Harmony harmony)
        => harmony.Patch(AccessTools.Method(typeof(NPC), nameof(NPC.checkAction)),
            prefix: new HarmonyMethod(typeof(Npcs), nameof(CheckActionPrefix)));

    private static bool CheckActionPrefix(NPC __instance, Farmer who, ref bool __result)
    {
        try
        {
            if (!__instance.Name.StartsWith(Ids.RowPrefix, StringComparison.Ordinal) || !who.IsLocalPlayer || who.ActiveObject != null
                || __instance.CurrentDialogue.Count > 0 || Game1.activeClickableMenu != null)
                return true;
            if (Talk(__instance, who))
            {
                __result = true;
                return false;
            }
        }
        catch (Exception ex)
        {
            ModServices.Log.Error($"Talking to {__instance.Name} failed: {ex}");
        }
        return true;
    }

    private static bool Talk(NPC npc, Farmer who)
    {
        if (npc.Name == Sorenson)
        {
            Contracts(npc, who);
            return true;
        }
        string local = WorldService.Local(npc.Name);
        if (TryHint(npc, who, local))
            return true;
        if (DataLoader.Shops(Game1.content).ContainsKey(npc.Name))
            return Utility.TryOpenShopMenu(npc.Name, npc.Name);
        return false;
    }

    /// <summary>One hint a day from an NPC channel, for a target the farm hasn't discovered.</summary>
    private static bool TryHint(NPC npc, Farmer who, string channel)
    {
        string key = HintDayKey + channel;
        int day = (int)Game1.stats.DaysPlayed;
        if (who.modData.TryGetValue(key, out string? last) && last == day.ToString())
            return false;
        List<HintRow> hints = ModServices.World.AvailableHints(channel);
        if (hints.Count == 0)
            return false;
        who.modData[key] = day.ToString();
        string text = ModServices.World.Reveal(who, hints[0]);
        npc.CurrentDialogue.Push(new Dialogue(npc, null, text));
        Game1.drawDialogue(npc);
        return true;
    }

    /// <summary>Dr. Sorenson's bulk contracts (doc 15 §6): deliver from the inventory for gold and the reward.</summary>
    private static void Contracts(NPC npc, Farmer who)
    {
        IReadOnlyList<ContractRow> open = ModServices.World.OpenContracts();
        if (open.Count == 0)
        {
            npc.CurrentDialogue.Push(new Dialogue(npc, null, ModServices.T("contract.none")));
            Game1.drawDialogue(npc);
            return;
        }
        var responses = open.Select(c => new Response(c.Id, ModServices.T("contract.line", new
        {
            name = c.NameKey != null ? ModServices.T(c.NameKey) : WorldService.Local(c.Id),
            count = c.Count,
            item = UI.MenuKit.ItemName(Ids.QualifyItem(c.Item)),
            gold = c.Gold,
        }))).Append(new Response("No", ModServices.T("contract.later"))).ToArray();
        who.currentLocation.createQuestionDialogue(ModServices.T("contract.ask"), responses, (farmer, answer) =>
        {
            if (ModServices.Content.Registry.Contracts.Get(answer) is not ContractRow contract)
                return;
            if (ModServices.World.Deliver(farmer, contract))
                Game1.drawObjectDialogue(ModServices.T("contract.done", new { gold = contract.Gold }));
            else
                Game1.drawObjectDialogue(ModServices.T("contract.short", new { count = contract.Count, item = UI.MenuKit.ItemName(Ids.QualifyItem(contract.Item)) }));
        }, npc);
    }
}
