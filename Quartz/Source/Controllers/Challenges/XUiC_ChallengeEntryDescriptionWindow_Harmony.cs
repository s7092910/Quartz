using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[HarmonyPatch(typeof(XUiC_ChallengeEntryDescriptionWindow))]
public static class XUiC_ChallengeEntryDescriptionWindowPatch
{

    [HarmonyPostfix]
    [HarmonyPatch("SetChallenge")]
    public static void SetChallenge(global::XUiC_ChallengeEntryDescriptionWindow __instance, XUiC_ChallengeEntry challengeEntry)
    {
        if (__instance is Quartz.XUiC_ChallengeEntryDescriptionWindow descriptionWindow)
        {
            descriptionWindow.SetObjectives();
        }
    }
}
