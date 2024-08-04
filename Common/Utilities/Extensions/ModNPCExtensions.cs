using Terraria.GameContent;

namespace PathOfTerraria.Common.Utilities.Extensions;

/// <summary>
///		Provides basic <see cref="NPC"/> extension methods.
/// </summary>
public static class ModNPCExtensions
{
	public static ITownNPCProfile GetDefaultProfile(this ModNPC npc)
	{
		return new Profiles.DefaultNPCProfile(npc.Texture, ModContent.GetModHeadSlot(npc.HeadTexture));
	}
}