using ReLogic.Utilities;
using Terraria.Audio;

namespace PathOfTerraria.Utilities;

public static class SoundUtils
{
	/// <summary> Maintains a looping sound instance, creating, updating, and destroying it depending on the provided value. </summary>
	public static void UpdateLoopingSound(ref SlotId slot, Vector2? position, float volume, float? pitch, in SoundStyle style)
	{
		SoundEngine.TryGetActiveSound(slot, out ActiveSound sound);

		if (volume > 0f)
		{
			if (sound == null)
			{
				slot = SoundEngine.PlaySound(in style, position);
				return;
			}

			sound.Position = position;
			sound.Volume = volume;

			if (pitch.HasValue)
			{
				sound.Pitch = pitch.Value;
			}
		}
		else if (sound != null)
		{
			sound.Stop();

			slot = SlotId.Invalid;
		}
	}

	/// <summary> Stops a sound matching the provided SlotId, setting it to Invalid. Returns true if the operation were performed. </summary>
	public static bool StopAndInvalidateSoundSlot(ref SlotId slot)
	{
		if (SoundEngine.TryGetActiveSound(slot, out ActiveSound sound))
		{
			sound.Stop();
			slot = SlotId.Invalid;
			return true;
		}

		return false;
	}
}
