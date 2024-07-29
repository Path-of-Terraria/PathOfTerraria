using Terraria.GameContent;

namespace PathOfTerraria.Helpers.Extensions;

/// <summary>
///		Provides basic <see cref="ModNPC"/> extensions.
/// </summary>
public static class ModNPCExtensions
{
	/// <summary>
	///		Creates a default town NPC profile.
	/// </summary>
	/// <param name="npc">The <see cref="ModNPC"/> instance for which to create the profile.</param>
	/// <returns>
	///		An <see cref="ITownNPCProfile"/> instance representing the default town NPC profile
	///		created for the given <see cref="ModNPC"/>.
	/// </returns>
	public static ITownNPCProfile GetDefaultTownProfile(this ModNPC npc)
	{
		return new Profiles.DefaultNPCProfile(npc.Texture, ModContent.GetModHeadSlot(npc.HeadTexture));
	}
}