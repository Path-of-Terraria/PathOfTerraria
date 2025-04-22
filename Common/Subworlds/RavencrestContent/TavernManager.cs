using PathOfTerraria.Common.NPCs;
using SubworldLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.Core;
using Terraria.Utilities;

namespace PathOfTerraria.Common.Subworlds.RavencrestContent;

internal class TavernManager : ModSystem
{
	public const int SeatY = 154;

	public static readonly List<string> TavernNPCFullNames = [];

	private static readonly Point16[] Seats = [new Point16(505, SeatY), new Point16(509, SeatY), new Point16(511, SeatY), new Point16(515, SeatY)];

	public override void PostSetupContent()
	{
		TavernNPCFullNames.Clear();

		IEnumerable<Type> types = AssemblyManager.GetLoadableTypes(Mod.Code).Where(x => !x.IsAbstract && typeof(ITavernNPC).IsAssignableFrom(x));
		MethodInfo baseMethod = typeof(ModContent).GetMethod(nameof(ModContent.GetInstance));

		foreach (Type type in types)
		{
			MethodInfo genericMethod = baseMethod.MakeGenericMethod(type);
			var instance = genericMethod.Invoke(null, null) as ITavernNPC;

			TavernNPCFullNames.Add(instance.FullName);
		}
	}

	internal static void OneTimeCheck()
	{
		if (SubworldSystem.Current is not RavencrestSubworld)
		{
			return;
		}

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.ModNPC is ITavernNPC)
			{
				npc.active = false;
			}
		}

		WeightedRandom<int> entries = new(Main.rand);
		HashSet<int> guarantees = [];

		foreach (string npcName in TavernNPCFullNames)
		{
			NPC npc = ContentSamples.NpcsByNetId[ModContent.Find<ModNPC>(npcName).Type];
			var tavernNPC = npc.ModNPC as ITavernNPC;

			if (tavernNPC.ForceSpawnInTavern())
			{
				guarantees.Add(npc.type);
			}
			else if (Main.rand.NextFloat() < tavernNPC.SpawnChanceInTavern())
			{
				entries.Add(npc.type);
			}
		}

		if (entries.elements.Count == 0 && guarantees.Count == 0)
		{
			return;
		}

		Queue<int> types = new(guarantees);
		Queue<Point16> seatsToUse = [];

		for (int i = types.Count; i < Seats.Length; ++i)
		{
			if (entries.elements.Count > 0)
			{
				int entry = entries;
				types.Enqueue(entry);
				entries.elements.RemoveAll(x => x.Item1 == entry);
			}

			seatsToUse.Enqueue(Seats[i]);
		}

		for (int i = 0; i < types.Count; ++i)
		{
			int type = types.Dequeue();
			Point16 pos = seatsToUse.Dequeue();

			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), pos.X * 16, pos.Y * 16, type);
		}
	}
}
