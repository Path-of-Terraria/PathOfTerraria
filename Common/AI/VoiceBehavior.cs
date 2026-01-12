using PathOfTerraria.Utilities;
using ReLogic.Utilities;
using Terraria.Audio;

namespace PathOfTerraria.Common.AI;

internal struct VoiceBehavior
{
	public SlotId Voice;

	public bool Play(NPC entity, in SoundStyle style, bool force = false)
	{
		if (force)
		{
			Stop();
		}
		else if (SoundEngine.TryGetActiveSound(Voice, out ActiveSound _))
		{
			return false;
		}

		Play(entity, style);

		return true;
	}

	private void Play(NPC entity, in SoundStyle style)
	{
		bool UpdateSound(ActiveSound sound)
		{
			sound.Position = entity.Center;

			return entity.active && Main.npc[entity.whoAmI] == entity;
		}

		Voice = SoundEngine.PlaySound(style, entity.Center, UpdateSound);
	}

	public void Stop()
	{
		SoundUtils.StopAndInvalidateSoundSlot(ref Voice);
	}
}
