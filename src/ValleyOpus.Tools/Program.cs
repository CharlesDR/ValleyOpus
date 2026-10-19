using System.IO.Compression;
using ValleyOpus.Engine;
using ValleyOpus.Engine.Model;
using ValleyOpus.Engine.Packs;
using ValleyOpus.Engine.Registry;

namespace ValleyOpus.Tools;

/// <summary>Console tools over the engine (doc 09 §12).</summary>
public static class Program
{
    private const string Usage = """
        ValleyOpus tools

          validate <pack folder>... [--has-mod <id>]...
              Reads the packs in load order, runs the validator and prints its report.
              Exit code 1 if anything was found.

          package --mod <built mod folder> --content <content folder> --out <zip> [--with-spikes]
              Writes a test-build zip: the C# mod plus the base and integration packs
              (and the spike pack with --with-spikes). The test fixture pack is never included.
        """;

    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine(Usage);
            return 2;
        }

        try
        {
            return args[0] switch
            {
                "validate" => Validate(args.Skip(1).ToList()),
                "package" => Package(args.Skip(1).ToList()),
                _ => Fail("unknown command: " + args[0]),
            };
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return Fail(ex.Message);
        }
    }

    private static int Validate(List<string> args)
    {
        var packs = new List<string>();
        var mods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < args.Count; i++)
        {
            if (args[i] == "--has-mod" && i + 1 < args.Count)
                mods.Add(args[++i]);
            else
                packs.Add(args[i]);
        }
        if (packs.Count == 0)
            return Fail("validate needs at least one pack folder");

        PackReadResult read = ContentPackReader.Read(packs, mods);
        ContentPipeline pipeline = ContentPipeline.Build(read.Registries, read.Items, catalogIsComplete: false);
        List<Finding> findings = read.Findings.Concat(pipeline.Findings).ToList();

        Console.WriteLine($"Read {packs.Count} pack(s): {pipeline.Registry.Primes.Count} Primes, {pipeline.Registry.Substances.Count} substances, "
            + $"{pipeline.Registry.Recipes.Count} recipes, {pipeline.Registry.Apparatus.Count} apparatus, {read.Items.All.Count()} authored items.");
        foreach (Finding finding in findings)
            Console.WriteLine("  " + finding);
        Console.WriteLine(findings.Count == 0 ? "No findings." : $"{findings.Count} finding(s).");
        return findings.Count == 0 ? 0 : 1;
    }

    private static int Package(List<string> args)
    {
        string? mod = Option(args, "--mod"), content = Option(args, "--content"), output = Option(args, "--out");
        if (mod is null || content is null || output is null)
            return Fail("package needs --mod, --content and --out");
        bool withSpikes = args.Contains("--with-spikes");
        if (!File.Exists(Path.Combine(mod, "manifest.json")))
            return Fail($"no manifest.json in {mod}; build the mod project first");

        string staging = Path.Combine(Path.GetTempPath(), "valleyopus-package-" + Guid.NewGuid().ToString("N"));
        string root = Path.Combine(staging, "ValleyOpus");
        try
        {
            CopyFolder(mod, Path.Combine(root, "ValleyOpus"));
            foreach (string pack in Directory.GetDirectories(content))
            {
                string name = Path.GetFileName(pack);
                if (name.Contains("Test Fixtures", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (name.Contains("Spikes", StringComparison.OrdinalIgnoreCase) && !withSpikes)
                    continue;
                CopyFolder(pack, Path.Combine(root, name));
            }

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
            if (File.Exists(output))
                File.Delete(output);
            ZipFile.CreateFromDirectory(staging, output);
            Console.WriteLine($"Wrote {output} ({string.Join(", ", Directory.GetDirectories(root).Select(Path.GetFileName))}).");
            return 0;
        }
        finally
        {
            if (Directory.Exists(staging))
                Directory.Delete(staging, recursive: true);
        }
    }

    private static void CopyFolder(string from, string to)
    {
        foreach (string file in Directory.GetFiles(from, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(from, file);
            if (relative.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase) && !relative.Contains(Ids.ModId))
                continue;
            string target = Path.Combine(to, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: true);
        }
    }

    private static string? Option(List<string> args, string name)
    {
        int i = args.IndexOf(name);
        return i >= 0 && i + 1 < args.Count ? args[i + 1] : null;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        Console.Error.WriteLine();
        Console.Error.WriteLine(Usage);
        return 2;
    }
}
