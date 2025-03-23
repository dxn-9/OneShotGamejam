using System;
using System.Collections.Generic;
using Nodes;

public static class Utils
{
    public static Dictionary<string, System.Type> typeMap = new()
    {
        { "ConveyorBelt", typeof(ConveyorBelt) },
        { "CatapultNode", typeof(CatapultNode) },
        { "WindConveyorBelt", typeof(WindConveyorBelt) }
    };

    public static string GetNextLevel(string currentLevel)
    {
        if (currentLevel == "Level1") return "Level2";
        if (currentLevel == "Level2") return "Level3";
        if (currentLevel == "Level3") return "Level4";
        if (currentLevel == "Level4") return "Level5";
        if (currentLevel == "Level5") return "Level6";
        return "MenuScene";
    }
};