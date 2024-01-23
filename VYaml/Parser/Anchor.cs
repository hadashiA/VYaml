#nullable enable
using System;

namespace VYaml.Parser
{
    public class Anchor : IEquatable<Anchor>
    {
        public string Name { get; }
        public int Id { get; }

        public Anchor(string name, int id)
        {
            Name = name;
            Id = id;
        }

        public bool Equals(Anchor? other) => other != null && Id == other.Id;
        public override bool Equals(object? obj) => obj is Anchor other && Equals(other);
        public override int GetHashCode() => Id;

        public override string ToString() => $"{Name} Id={Id}";
    }
}
