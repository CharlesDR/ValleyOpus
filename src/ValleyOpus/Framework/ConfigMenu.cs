using StardewModdingAPI;

namespace ValleyOpus.Framework;

/// <summary>The Generic Mod Config Menu page (doc 16 §15), when GMCM is installed; config.json works without it.</summary>
internal static class ConfigMenu
{
    public static void Register(IModHelper helper, IManifest manifest)
    {
        IGenericModConfigMenuApi? gmcm = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (gmcm is null)
            return;
        ModConfig config = ModServices.Config;
        gmcm.Register(manifest, reset: () => ModServices.Config = new ModConfig(), save: () => helper.WriteConfig(ModServices.Config));
        gmcm.AddKeybind(manifest, () => ModServices.Config.CodexKey, v => ModServices.Config.CodexKey = v, () => ModServices.T("config.codex-key"), () => ModServices.T("config.codex-key.desc"));
        gmcm.AddBoolOption(manifest, () => ModServices.Config.ShowOverlay, v => ModServices.Config.ShowOverlay = v, () => ModServices.T("config.overlay"), () => ModServices.T("config.overlay.desc"));
        gmcm.AddTextOption(manifest, () => ModServices.Config.Toasts, v => ModServices.Config.Toasts = v, () => ModServices.T("config.toasts"), () => ModServices.T("config.toasts.desc"), new[] { "All", "FirstOnly", "None" });
        gmcm.AddBoolOption(manifest, () => ModServices.Config.MachineFeedback, v => ModServices.Config.MachineFeedback = v, () => ModServices.T("config.machine-feedback"), () => ModServices.T("config.machine-feedback.desc"));
        gmcm.AddNumberOption(manifest, () => ModServices.Config.ChainBudgetPerSecond, v => ModServices.Config.ChainBudgetPerSecond = v, () => ModServices.T("config.chain-budget"), () => ModServices.T("config.chain-budget.desc"), 20, 1000, 20);
        gmcm.AddNumberOption(manifest, () => ModServices.Config.DiffuserRadius, v => ModServices.Config.DiffuserRadius = v, () => ModServices.T("config.diffuser-radius"), () => ModServices.T("config.diffuser-radius.desc"), 3, 15, 1);
        gmcm.AddBoolOption(manifest, () => ModServices.Config.Verbose, v => ModServices.Config.Verbose = ModServices.Log.Verbose = v, () => ModServices.T("config.verbose"), () => ModServices.T("config.verbose.desc"));
    }
}

/// <summary>The subset of GMCM's API we use (copied signatures, as GMCM's docs recommend).</summary>
public interface IGenericModConfigMenuApi
{
    void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

    void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);

    void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name, Func<string>? tooltip = null, int? min = null, int? max = null, int? interval = null, Func<int, string>? formatValue = null, string? fieldId = null);

    void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name, Func<string>? tooltip = null, string[]? allowedValues = null, Func<string, string>? formatAllowedValue = null, string? fieldId = null);

    void AddKeybind(IManifest mod, Func<SButton> getValue, Action<SButton> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);
}
