using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;
using PathOfTerraria.Content.NPCs.Mapping.Forest;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.NPCs;

internal class NoGearDropNPC : GlobalNPC
{
	internal static HashSet<int> NoDropTypes = [];

	public override bool InstancePerEntity => true;

	/// <summary>
	/// Disables gear drops for this specific instance.<br/>
	/// If you need to disable drops for an entire type, use <see cref="NoDropTypes"/>.
	/// </summary>
	public bool InstancedNoDrop = false;

	public static bool NoDrops(NPC npc)
	{
		return NoDropTypes.Contains(npc.type) || npc.GetGlobalNPC<NoGearDropNPC>().InstancedNoDrop;
	}

	public static void AddDroplessType(int type)
	{
		NoDropTypes.Add(type);
	}

	public static void AddDroplessType<T>() where T : ModNPC
	{
		NoDropTypes.Add(ModContent.NPCType<T>());
	}

	public override void SetStaticDefaults()
	{
		AddDroplessType<CanopyBird>();
		AddDroplessType<WormLightning>();
		AddDroplessType(NPCID.SlimeSpiked);
	}
}
