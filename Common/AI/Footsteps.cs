using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;

namespace PathOfTerraria.Common.AI;

internal struct Footsteps
{
	public struct Context(NPC npc)
	{
		public Vector2 Velocity = npc.velocity;
		public Vector2 FootPosition = npc.Bottom;
		public (int Current, int Previous, int First, int Second)? Frame;
		public SoundStyle? StepSound;
		public SoundStyle? JumpSound;
		public SoundStyle? LandSound;
		public bool AllowScreenShake;
		public ushort MinTicksBetweenSteps = 10;
	}

	private Vector2 oldVelocity;
	private uint lastStepTick;

	public void Perform(in Context ctx)
	{
		uint tickTime = Main.GameUpdateCount;
		bool isOnGround = ctx.Velocity.Y == 0f;
		bool wasOnGround = oldVelocity.Y == 0f;
		bool jumped = !isOnGround && wasOnGround;
		bool landed = isOnGround && !wasOnGround;

		if ((tickTime - lastStepTick) < ctx.MinTicksBetweenSteps) { return; }

		if (landed || jumped || (isOnGround && ctx.Frame is { } f && ((f.Current == f.First && f.Previous != f.First) || (f.Current == f.Second && f.Previous != f.Second))))
		{
			lastStepTick = tickTime;
			SoundEngine.PlaySound((jumped ? ctx.JumpSound : null) ?? (landed ? ctx.LandSound : null) ?? ctx.StepSound, ctx.FootPosition);

			if (ctx.AllowScreenShake)
			{
				Main.instance.CameraModifiers.Add(new PunchCameraModifier(ctx.FootPosition, new Vector2(0f, -1f), 1f, 3f, 15, 700f, "Abominable"));
			}
		}

		oldVelocity = ctx.Velocity;
	}
}
