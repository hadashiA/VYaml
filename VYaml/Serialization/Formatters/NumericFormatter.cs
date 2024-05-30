using System;
using System.Numerics;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class BigIntegerFormatter : IYamlFormatter<BigInteger>
    {
        public static readonly BigIntegerFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, BigInteger value, YamlSerializationContext context)
        {
            emitter.WriteString(value.ToString());
        }

        public BigInteger Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }

            var stringValue = parser.ReadScalarAsString();
            return BigInteger.Parse(stringValue!);
        }
    }

    public class ComplexFormatter : IYamlFormatter<Complex>
    {
        public static readonly ComplexFormatter Instance = new();

        public void Serialize(ref Utf8YamlEmitter emitter, Complex value, YamlSerializationContext context)
        {
            emitter.WriteString($"{value.Real}+{value.Imaginary}i");
        }

        public Complex Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            if (parser.IsNullScalar())
            {
                return default;
            }
            var stringValue = parser.ReadScalarAsString()!;
            var separatorIndex = stringValue.IndexOf('+');
            var real = double.Parse(stringValue.AsSpan(0, separatorIndex));
            var imaginary = double.Parse(stringValue.AsSpan(separatorIndex + 1, stringValue.Length - separatorIndex - 2));
            return new Complex(real, imaginary);
        }
    }
}
