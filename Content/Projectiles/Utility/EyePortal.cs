using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Content.Items.Consumables.Maps;
using ReLogic.Content;
using SubworldLibrary;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class EyePortal : ModProjectile, ISaveProjectile, IRightClickableProjectile
{
	private static Asset<Texture2D> Highlight = null;

	private ref float Timer => ref Projectile.ai[0];
	private ref float Uses => ref Projectile.ai[1];
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
		if (NPC.downedBoss1)
		{
			Projectile.Kill();
			SpawnDust();
			return;
		}

		Main.CurrentFrameFlags.HadAnActiveInteractibleProjectile = true;

		Projectile.timeLeft++;
		Projectile.rotation += 0.15f;
		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.05f);
		Projectile.velocity *= 0.96f;

		if (MaxUses == 0)
		{
			MaxUses = Map.GetBossUseCount();
		}
		
		if (Timer++ == 48)
		{
			SpawnDust();
		}

		if (Main.rand.NextBool(14))
		{
			Dust.NewDust(Projectile.position + new Vector2(8), Projectile.width - 16, Projectile.height - 16, DustID.Firework_Red);
		}

		Lighting.AddLight(Projectile.Center, TorchID.Red);
	}

	private void SpawnDust()
	{
		for (int i = 0; i < 20; ++i)
		{
			Dust.NewDust(Projectile.position + new Vector2(8), Projectile.width - 16, Projectile.height - 16, DustID.Firework_Red);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Vector2 position = Projectile.Center - Main.screenPosition;

		for (int i = 0; i < 3; ++i)
		{
			float rotation = Projectile.rotation * (i % 2 == 0 ? -1 : 1);
			Color color = lightColor * ((3 - i) * 0.2f) * Projectile.Opacity;
			Main.spriteBatch.Draw(tex, position, null, color, rotation, tex.Size() / 2f, 1f - i * 0.2f, SpriteEffects.None, 0);
		}

		this.DrawHighlightAndCheckRightClickInteraction(Highlight.Value, position, lightColor);
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

	bool IRightClickableProjectile.RightClick(Player player, bool mouseDirectlyOver)
	{
		if (Main.mouseRight && Main.mouseRightRelease)
		{
			SubworldSystem.Enter<EyeDomain>();

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
