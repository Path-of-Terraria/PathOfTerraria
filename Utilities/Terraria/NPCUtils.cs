using System.ComponentModel;
using Newtonsoft.Json;
using ReLogic.Reflection;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;

#nullable enable

namespace PathOfTerraria.Utilities.Terraria;

public static class NPCUtils
{
	public static Entity? GetTargetEntity(this NPC npc)
	{
		if (npc.HasValidTarget)
		{
			if (npc.HasPlayerTarget) { return Main.player[npc.target]; }
			if (npc.HasNPCTarget) { return Main.npc[npc.TranslatedTargetIndex]; }
		}

		return null;
	}
}