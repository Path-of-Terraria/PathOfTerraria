using PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;
using System.Collections.Generic;
using Terraria.GameContent;

namespace PathOfTerraria.Content.Projectiles.Melee;

public class GuardianAngelRay : ModProjectile
{
	public NPC Target => Main.npc[(int)Projectile.ai[0]];
	public ref float Time => ref Projectile.ai[1];

	public override string Texture => $"{PoTMod.ModName}/Assets/Misc/VFX/AngelRay";
        
	public override void SetDefaults()
	{
		Projectile.width = 52;
		Projectile.height = 52;
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.timeLeft = 80;
		Projectile.localNPCHitCooldown = -1;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.hide = true;
	}

	public override bool? CanHitNPC(NPC target)
	{
		return Projectile.timeLeft < 20 ? null : false;
	}

	public override void AI()
	{
		if (Projectile.timeLeft < 20)
		{
			int size = (int)(240 * (1 - Projectile.timeLeft / 20f));
			Projectile.Resize(size, size);
		}

		Projectile.Center = Target.Center;
	}

	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
	{
		behindNPCs.Add(index);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		float opacity = 1f;
		var scale = new Vector2(1f, 1f);

		if (Projectile.timeLeft > 20)
		{
			float factor = (Projectile.timeLeft - 20) / 60f;
			opacity = factor * factor * 0.5f;
		}
		else
		{
			if (Projectile.timeLeft <= 5)
			{
				opacity = Projectile.timeLeft / 5f;
			}

			Texture2D ring = GuardianAngel.AngelRingNPC.Ring.Value;
			float ringSize = 1 - Projectile.timeLeft / 20f;
			Main.spriteBatch.Draw(ring, Projectile.Center - Main.screenPosition, null, Color.White * (1 - ringSize), 0f, ring.Size() / 2f, ringSize * 4, SpriteEffects.None, 0);
		}

		Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * opacity, 0f, new Vector2(11, 186), scale, SpriteEffects.None, 0);

		return false;
	}
}