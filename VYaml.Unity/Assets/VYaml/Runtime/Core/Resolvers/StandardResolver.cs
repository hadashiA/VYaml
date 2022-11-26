using VYaml.Formatters;

namespace VYaml.Resolvers
{
    public class StandardResolver : IYamlFormatterResolver
    {
        public static readonly StandardResolver Instance = new();

        // static class FormatterCache<T>
        // {
        //     public static readonly IYamlFormatter<T> Formatter;
        //
        //     static FormatterCache()
        //     {
        //         if (typeof(T) == typeof(object))
        //         {
        //             // final fallback
        //             Formatter = PrimitiveObjectResolver.Instance.GetFormatter<T>();
        //         }
        //         else
        //         {
        //             foreach (IFormatterResolver item in Resolvers)
        //             {
        //                 IMessagePackFormatter<T> f = item.GetFormatter<T>();
        //                 if (f != null)
        //                 {
        //                     Formatter = f;
        //                     return;
        //                 }
        //             }
        //         }
        //     }
        // }

        public IYamlFormatter<T> GetFormatter<T>() => throw new System.NotImplementedException();
    }
}