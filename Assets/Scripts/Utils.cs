using System;
using System.Collections.Generic;
using Nodes;

public static class Utils
{
    public static Dictionary<string, System.Type> typeMap = new()
    {
        { "ConveyorBelt", typeof(ConveyorBelt) },
        { "CatapultNode", typeof(CatapultNode) }
    };
};