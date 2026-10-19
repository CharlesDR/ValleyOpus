using System.Text.Json.Nodes;

namespace ValleyOpus.Engine;

internal static class JsonUtil
{
    /// <summary>A deep copy; <c>JsonNode.DeepClone</c> only exists from .NET 8, and SMAPI runs on .NET 6.</summary>
    public static T Clone<T>(T node) where T : JsonNode => (T)JsonNode.Parse(node.ToJsonString())!;
}
