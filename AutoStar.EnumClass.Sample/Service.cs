using System;

namespace AutoStar.EnumClass.Sample
{
    [EnumClass]
    internal partial class Service
    {
        partial class TypeA
        {
            public string PropA { get; set; }
        }

        partial class TypeB
        {
            public string PropB1 { get; set; }
            public string PropB2 { get; set; }
        }

        [EnumClass]
        partial class TypeC
        {
            partial class TypeD
            {
                public int PropD1 { get; set; }
            }
        }
    }
}