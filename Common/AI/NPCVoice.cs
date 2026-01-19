using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Utilities;
using ReLogic.Utilities;
using Terraria.Audio;

namespace PathOfTerraria.Common.AI;

internal sealed class VoiceData
{
	// Config
	public (int ChanceX, SoundStyle Style)? PainSound;

	// State
	public SlotId Voice;
}

internal sealed class NPCVoice : NPCComponent<VoiceData>
{
	public override void HitEffect(NPC npc, NPC.HitInfo hit)
	{
		if (!Enabled || Main.dedServ) { return; }

		if (npc.life <= 0)
		{
			Stop();
			return;
		}

		if (hit.Damage > 1 && Data.PainSound is { } pain && Main.rand.NextBool(pain.ChanceX))
		{
			Play(npc, in pain.Style);
		}
	}

	public bool Play(NPC entity, in SoundStyle style, bool force = false)
	{
		if (force)
		{
			Stop();
		}
		else if (SoundEngine.TryGetActiveSound(Data.Voice, out ActiveSound _))
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

		Data.Voice = SoundEngine.PlaySound(style, entity.Center, UpdateSound);
	}

	public void Stop()
	{
		SoundUtils.StopAndInvalidateSoundSlot(ref Data.Voice);
	}
}
