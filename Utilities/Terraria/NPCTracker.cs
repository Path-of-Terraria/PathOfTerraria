using Terraria.Audio;

#nullable enable

namespace PathOfTerraria.Utilities.Terraria;

internal sealed class NPCTracker(NPC npc)
{
	private readonly int type = npc.type;
	private readonly int index = npc.whoAmI;

	public NPC? Npc()
	{
		return (!Main.gameMenu && Main.npc[index] is NPC { active: true } npc && npc.type == type) ? npc : null;
	}

	public Vector2? Center()
	{
		return Npc()?.Center;
	}

	public bool AudioCallback(ActiveSound sound)
	{
		if (Npc() is not NPC npc)
		{
			return false;
		}

		sound.Position = npc.Center;
		return true;
	}
}
