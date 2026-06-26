using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace VYaml.SourceGenerator;

// Equatable, compilation-independent representation of a diagnostic.
// Holding Diagnostic/Location directly in the incremental pipeline would root the
// Compilation and break caching, so we capture only value-typed data here and
// reconstruct the Diagnostic at the RegisterSourceOutput stage.
sealed class DiagnosticInfo : IEquatable<DiagnosticInfo>
{
    public DiagnosticDescriptor Descriptor { get; }
    public LocationInfo? Location { get; }
    public EquatableArray<string> MessageArgs { get; }

    DiagnosticInfo(DiagnosticDescriptor descriptor, LocationInfo? location, EquatableArray<string> messageArgs)
    {
        Descriptor = descriptor;
        Location = location;
        MessageArgs = messageArgs;
    }

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, Location? location, params string[] messageArgs)
    {
        return new DiagnosticInfo(descriptor, LocationInfo.CreateFrom(location), new EquatableArray<string>(messageArgs));
    }

    public Diagnostic ToDiagnostic()
    {
        var args = new object?[MessageArgs.Count];
        for (var i = 0; i < args.Length; i++)
        {
            args[i] = MessageArgs[i];
        }
        return Diagnostic.Create(Descriptor, Location?.ToLocation(), args);
    }

    public bool Equals(DiagnosticInfo? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Descriptor.Equals(other.Descriptor) &&
               Equals(Location, other.Location) &&
               MessageArgs.Equals(other.MessageArgs);
    }

    public override bool Equals(object? obj) => obj is DiagnosticInfo other && Equals(other);

    public override int GetHashCode()
    {
        var hash = Descriptor.GetHashCode();
        hash = unchecked(hash * 397) ^ (Location?.GetHashCode() ?? 0);
        hash = unchecked(hash * 397) ^ MessageArgs.GetHashCode();
        return hash;
    }
}

// Equatable, compilation-independent representation of a source Location.
sealed class LocationInfo : IEquatable<LocationInfo>
{
    public string FilePath { get; }
    public TextSpan TextSpan { get; }
    public LinePositionSpan LineSpan { get; }

    LocationInfo(string filePath, TextSpan textSpan, LinePositionSpan lineSpan)
    {
        FilePath = filePath;
        TextSpan = textSpan;
        LineSpan = lineSpan;
    }

    public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);

    public static LocationInfo? CreateFrom(Location? location)
    {
        if (location is null || location.SourceTree is null)
        {
            return null;
        }
        return new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
    }

    public bool Equals(LocationInfo? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return FilePath == other.FilePath &&
               TextSpan.Equals(other.TextSpan) &&
               LineSpan.Equals(other.LineSpan);
    }

    public override bool Equals(object? obj) => obj is LocationInfo other && Equals(other);

    public override int GetHashCode()
    {
        var hash = FilePath.GetHashCode();
        hash = unchecked(hash * 397) ^ TextSpan.GetHashCode();
        hash = unchecked(hash * 397) ^ LineSpan.GetHashCode();
        return hash;
    }
}
