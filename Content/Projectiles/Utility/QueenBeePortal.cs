using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Content.Items.Consumables.Maps;
using SubworldLibrary;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class QueenBeePortal : ModProjectile
{
	private ref float Timer => ref Projectile.ai[0];
	private ref float Uses => ref Projectile.ai[1];
	private ref float MaxUses => ref Projectile.ai[2];

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 3;

		ClickableProjectilePlayer.RegisterProjectile(Type, static (proj, _) =>
		{
			if (Main.mouseRight && Main.mouseRightRelease)
			{
				SubworldSystem.Enter<QueenBeeDomain>();

				proj.ai[1]++;
				proj.netUpdate = true;

				if (proj.ai[1] > proj.ai[2])
				{
					proj.Kill();
				}
			}

			Tooltip.SetName(Language.GetTextValue($"Mods.{PoTMod.ModName}.Misc.Enter"));
		});
	}

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 2;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(80, 80);
		Projectile.Opacity = 0.5f;
		Projectile.netImportant = true;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		Projectile.timeLeft++;
		Projectile.rotation += 0.015f;
		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.05f);
		Projectile.velocity *= 0.96f;
		Projectile.frame = (int)Math.Abs(Timer-- * 0.15f % 3);

		if (NPC.downedQueenBee)
		{
			Projectile.Kill();

			for (int i = 0; i < 20; ++i)
			{
				Dust.NewDust(Projectile.position + new Vector2(8), Projectile.width - 16, Projectile.height - 16, DustID.Honey);
			}

			return;
		}

		if (Projectile.honeyWet)
		{
			Projectile.velocity.Y -= 0.05f;
		}

		if (MaxUses == 0)
		{
			MaxUses = Map.GetBossUseCount();
		}

		if (Main.rand.NextBool(14))
		{
			Dust.NewDust(Projectile.position + new Vector2(8), Projectile.width - 16, Projectile.height - 16, DustID.Honey);
		}

		Lighting.AddLight(Projectile.Center, TorchID.Yellow);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;

		for (int i = 0; i < 3; ++i)
		{
			float rotation = Projectile.rotation * (i % 2 == 0 ? -1 : 1);
			Vector2 position = Projectile.Center - Main.screenPosition;
			Color color = lightColor * ((3 - i) * 0.2f) * Projectile.Opacity;
			Rectangle frame = new Rectangle(0, 60 * Projectile.frame, 60, 58);
			Main.spriteBatch.Draw(tex, position, frame, color, rotation, frame.Size() / 2f, 1f - i * 0.2f, SpriteEffects.None, 0);
		}

		return false;
	}

	public void SaveData(TagCompound tag)
	{
		tag.Add("uses", Uses);
	}

	public void LoadData(TagCompound tag, Projectile projectile)
	{
		projectile.ai[0] = 49;
		projectile.ai[1] = tag.GetFloat("uses");
	}
}
