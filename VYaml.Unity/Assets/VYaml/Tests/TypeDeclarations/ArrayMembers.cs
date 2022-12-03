using System.Collections.Generic;
using VYaml.Serialization;

namespace VYaml.Tests.TypeDeclarations
{
    [YamlObject]
    public partial class ArrayCheck
    {
        public int[]? Array1 { get; set; }
        public int?[]? Array2 { get; set; }
        public string[]? Array3 { get; set; }
        public string?[]? Array4 { get; set; }
    }

    [YamlObject]
    public partial class ArrayOptimizeCheck
    {
        public SimpleTypeTwo?[]? Array1 { get; set; }
        public List<SimpleTypeTwo?>? List1 { get; set; }
    }
}
