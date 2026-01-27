using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.ModPlayers;

public interface IGrabberNPC
{
	void OnGrab(int playerGrabbed);

	void UpdateGrabbing(int playerGrabbed, float attemptedPlayerMoveMagnitude, bool grappling) { }

	Vector2 GetGrabOffset(int playerGrabbed)
	{
		return Vector2.Zero;
	}

	bool LetGo()
	{
		return false;
	}
}

public class GrabbedPlayer : ModPlayer
{
	public int BeingGrabbed = -1;

	public override void Load()
	{
		On_Player.GrappleMovement += ResetThePlayerToGrabberAgain;
		On_Player.SlopingCollision += StopSlopingWhenGrabbed;
	}

	private static void StopSlopingWhenGrabbed(On_Player.orig_SlopingCollision orig, Player self, bool fallThrough, bool ignorePlats)
	{
		if (self.GetModPlayer<GrabbedPlayer>().BeingGrabbed != -1)
		{
			return;
		}

		orig(self, fallThrough, ignorePlats);
	}

	private static void ResetThePlayerToGrabberAgain(On_Player.orig_GrappleMovement orig, Player self)
	{
		orig(self);

		self.GetModPlayer<GrabbedPlayer>().UpdateGrabbed();
	}

	public void UpdateGrabbed()
	{
		if (Player.dead)
		{
			BeingGrabbed = -1;
		}

		if (BeingGrabbed >= 0)
		{
			NPC grabber = Main.npc[BeingGrabbed];

			if (!grabber.active || grabber.ModNPC is not IGrabberNPC grabbingNpc || grabbingNpc.LetGo())
			{
				BeingGrabbed = -1;
				return;
			}

			Player.Center = grabber.Center + grabbingNpc.GetGrabOffset(Player.whoAmI);

			float speed = Player.velocity.Length() * 2;

			if (Player.grappling[0] > -1)
			{
				speed /= 20f;
			}

			grabbingNpc.UpdateGrabbing(Player.whoAmI, speed, Player.grappling[0] > -1);
			Player.velocity = Vector2.Zero;
			Player.gfxOffY = 0;
		}
	}

	public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
	{
		BeingGrabbed = -1;
	}
}