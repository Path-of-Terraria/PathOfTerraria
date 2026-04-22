using System.Collections.Generic;
using PathOfTerraria.Common.NPCs.Components;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;

#nullable enable

namespace PathOfTerraria.Common.AI;

internal sealed class FootstepsData()
{
	public bool AutomaticLogic = true;
	/// <summary> Source animation indices, before spritesheet mapping. </summary>
	public Dictionary<string, int[]>? Frames = null;
	public SoundStyle? StepSound = null;
	public SoundStyle? JumpSound = null;
	public SoundStyle? LandSound = null;
	public bool NoLandSounds = false;
	public bool NoJumpSounds = false;
	public bool ScreenShake = false;
	public ushort MinTicksBetweenSteps = 10;
}

internal sealed class NPCFootsteps : NPCComponent<FootstepsData>
{
	public ref struct Ctx(NPC npc)
	{
		public Vector2 Velocity = npc.velocity;
		public Vector2 FootPosition = npc.Bottom;
		public NPCAnimations? Animations = npc.TryGetGlobalNPC(out NPCAnimations anims) ? anims : null;
	}

	private Vector2 oldVelocity;
	private uint lastStepTick;

	public override void PostAI(NPC npc)
	{
		if (!Enabled || !Data.AutomaticLogic) { return; }

		ManualUpdate(npc);
	}

	public void ManualUpdate(NPC npc, bool forceStep = false)
	{
		var ctx = new Ctx(npc);
		uint tickTime = Main.GameUpdateCount;
		bool isOnGround = ctx.Velocity.Y == 0f;
		bool wasOnGround = oldVelocity.Y == 0f;
		bool jumped = !isOnGround && wasOnGround && !Data.NoJumpSounds;
		bool landed = isOnGround && !wasOnGround && !Data.NoLandSounds;

		if ((tickTime - lastStepTick) < Data.MinTicksBetweenSteps) { return; }

		bool CheckAnimations(in Ctx ctx)
		{
			if (ctx.Animations is not { } anims) { return false; }

			if ((Data.Frames?.TryGetValue(anims.Current.Id ?? string.Empty, out int[]? frames)) != true)
			{
				return false;
			}

			foreach (int frame in frames!)
			{
				if (anims.CurrentFrame == frame && anims.PreviousFrame != frame)
				{
					return true;
				}
			}

			return false;
		}

		if (forceStep || landed || jumped || (isOnGround && CheckAnimations(in ctx)))
		{
			lastStepTick = tickTime;
			SoundEngine.PlaySound((jumped ? Data.JumpSound : null) ?? (landed ? Data.LandSound : null) ?? Data.StepSound, ctx.FootPosition);

			if (Data.ScreenShake)
			{
				Main.instance.CameraModifiers.Add(new PunchCameraModifier(ctx.FootPosition, new Vector2(0f, -1f), 1f, 3f, 15, 700f, "Footstep"));
			}
		}

		oldVelocity = ctx.Velocity;
	}
}
