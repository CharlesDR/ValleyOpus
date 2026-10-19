namespace ValleyOpus.Engine.Registry;

public enum Severity
{
    /// <summary>Reported; the row stays enabled.</summary>
    Warning,

    /// <summary>The row is disabled with this reason (doc 12: "disable the row, never crash").</summary>
    Error
}

/// <summary>One validator or loader finding: a Warn line in the SMAPI log and a line in the validator report.</summary>
public sealed record Finding(Severity Severity, string Registry, string RowId, string Message)
{
    public override string ToString() => $"{(this.Severity == Severity.Error ? "disabled" : "warning")} {this.Registry}/{this.RowId}: {this.Message}";
}
