using System.Collections.Generic;
using AutoStar.Common;
using Microsoft.CodeAnalysis;

namespace AutoStar.EnumClass;

class SwitchStatementAnalyzer
{
    public SwitchStatementAnalyzer(
        Compilation contextCompilation,
        MarkerAttribute attribute,
        SwitchSyntaxReceiver switchSyntaxReceiver)
    {
            
    }

    public IEnumerable<ModelFailure> Analyze()
    {
        yield break;
    }
}