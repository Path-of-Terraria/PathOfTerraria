using SubworldLibrary;
using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.DeerDomain;

public class DeerclopsDoubleJumpPlayer : ModPlayer
{
	public int JumpsRemaining;

	public override void ResetEffects()
	{
		if (SubworldSystem.Current is DeerclopsDomain)
		{
			Player.GetJumpState<DeerclopsDoubleJump>().Enable();
		}
	}
}

public class DeerclopsDoubleJump : ExtraJump
{
	public override Position GetDefaultPosition()
	{
		return new Before(CloudInABottle);
	}

	public override float GetDurationMultiplier(Player player)
	{
		return 2f;
	}

	public override void UpdateHorizontalSpeeds(Player player)
	{
		player.runAcceleration *= 1.5f;
		player.maxRunSpeed *= 1.5f;
	}

	public override void OnRefreshed(Player player)
	{
		player.GetModPlayer<DeerclopsDoubleJumpPlayer>().JumpsRemaining = 2;
	}

	public override void OnStarted(Player player, ref bool playSound)
	{
		playSound = false;
		SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0, PitchVariance = 0.04f }, player.Bottom);

		ref int jumps = ref player.GetModPlayer<DeerclopsDoubleJumpPlayer>().JumpsRemaining;

		jumps--;

		if (jumps > 0)
		{
			player.GetJumpState(this).Available = true;
		}

		for (int i = 0; i < 30; i++)
		{
			Vector2 position = player.BottomLeft + new Vector2(Main.rand.NextFloat(player.width), 0);
			Vector2 velocity = -(player.velocity * new Vector2(0.5f, 1)).RotatedByRandom(0.9f) * Main.rand.NextFloat(0.8f, 1.5f);
			var dust = Dust.NewDustPerfect(position, Main.rand.NextBool() ? DustID.Ice : DustID.IceTorch, velocity, Scale: Main.rand.NextFloat(2, 3));
			dust.noGravity = true;
		}
	}
}