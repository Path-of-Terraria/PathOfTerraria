using Terraria.GameContent;

namespace PathOfTerraria.Common.Utilities.Extensions;

public static class NPCExtensions
{
	public static ITownNPCProfile DefaultProfile(this ModNPC npc)
	{
		return new Profiles.DefaultNPCProfile(npc.Texture, ModContent.GetModHeadSlot(npc.HeadTexture));
	}
}