using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Systems.MapContent;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Content.Items.Consumables.Maps;
using ReLogic.Content;
using SubworldLibrary;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class MLPortal : ModProjectile, ISaveProjectile, IRightClickableProjectile, IMapIcon
{
	private static Asset<Texture2D> Highlight = null;

	private ref float MaxUses => ref Projectile.ai[2];

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.IsInteractable[Type] = true;

		Highlight = ModContent.Request<Texture2D>(Texture + "_Highlight");
	}

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 2;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(60);
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
		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.05f);
		Projectile.velocity *= 0.96f;
		Projectile.rotation += 0.12f;

		Main.CurrentFrameFlags.HadAnActiveInteractibleProjectile = true;

		Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.4f, 0.4f));

		if (MaxUses == 0)
		{
			MaxUses = Map.GetBossUseCount();
		}
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Color.White;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Vector2 position = Projectile.Center - Main.screenPosition;

		for (int i = 0; i < 3; ++i)
		{
			float rotation = Projectile.rotation * (i % 2 == 0 ? -1 : 1);
			Color color = lightColor * ((3 - i) * 0.2f) * Projectile.Opacity;
			Main.spriteBatch.Draw(tex, position, null, color, rotation, tex.Size() / 2f, 2f - i * 0.4f, SpriteEffects.None, 0);
		}

		this.DrawHighlightAndCheckRightClickInteraction(Highlight.Value, position, lightColor, 2f);
		return false;
	}

	bool IRightClickableProjectile.RightClick(Player player, bool mouseDirectlyOver)
	{
		if (Main.mouseRight && Main.mouseRightRelease)
		{
			SubworldSystem.Enter<MoonLordDomain>();

			Projectile.ai[1]++;
			Projectile.netUpdate = true;

			if (Projectile.ai[1] > Projectile.ai[2])
			{
				Projectile.Kill();
			}

			return true;
		}

		if (mouseDirectlyOver)
		{
			Tooltip.Create(new TooltipDescription
			{
				Identifier = "Portal",
				SimpleTitle = Language.GetTextValue($"Mods.{PoTMod.ModName}.Misc.Enter"),
			});
		}

		return false;
	}
}
