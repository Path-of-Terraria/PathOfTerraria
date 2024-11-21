using SubworldLibrary;
using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.BoCDomain;

public class BrainJumpPlayer : ModPlayer
{
	public int JumpsRemaining;

	public override void ResetEffects()
	{
		if (SubworldSystem.Current is BrainDomain)
		{
			Player.GetJumpState<BrainDoubleJump>().Enable();
		}
	}
}

public class BrainDoubleJump : ExtraJump
{
	public override Position GetDefaultPosition()
	{
		return new Before(CloudInABottle);
	}

	public override float GetDurationMultiplier(Player player)
	{
		return 1.2f;
	}

	public override void UpdateHorizontalSpeeds(Player player)
	{
		player.runAcceleration *= 1.5f;
		player.maxRunSpeed *= 1.5f;
	}

	public override void OnRefreshed(Player player)
	{
		player.GetModPlayer<BrainJumpPlayer>().JumpsRemaining = 3;
	}

	public override void OnStarted(Player player, ref bool playSound)
	{
		playSound = false;
		SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0, PitchVariance = 0.04f }, player.Bottom);

		ref int jumps = ref player.GetModPlayer<BrainJumpPlayer>().JumpsRemaining;

		jumps--;

		if (jumps > 0)
		{
			player.GetJumpState(this).Available = true;
		}

		for (int i = 0; i < 30; i++)
		{
			Vector2 position = player.BottomLeft + new Vector2(Main.rand.NextFloat(player.width), 0);
			Vector2 velocity = -(player.velocity * new Vector2(0.5f, 1)).RotatedByRandom(0.9f) * Main.rand.NextFloat(0.8f, 1.5f);
			var dust = Dust.NewDustPerfect(position, Main.rand.NextBool() ? DustID.Crimslime : DustID.Crimson, velocity, Scale: Main.rand.NextFloat(2, 3));
			dust.noGravity = true;
		}
	}
}