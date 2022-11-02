using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace AnimalBiosculpter
{
    public class AnimalBiosculpter : Mod
    {
        public static AnimalBiosculpter? Instance;

        public AnimalBiosculpter(ModContentPack content) : base(content)
        {
            Instance = this;
        }
    }
}
