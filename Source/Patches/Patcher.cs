using System.Reflection;
using HarmonyLib;
using Verse;

namespace AnimalBiosculpter.Patches
{
    [StaticConstructorOnStartup]
    public class Patcher
    {
        public static Harmony instance;
        static Patcher()
        {
            instance = new Harmony("bodilpwnz.AnimalBiosculpter");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
