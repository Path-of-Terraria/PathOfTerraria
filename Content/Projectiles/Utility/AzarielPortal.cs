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
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class AzarielPortal : ModProjectile, ISaveProjectile, IRightClickableProjectile, IMapIcon
{
	public enum Domain
	{
		Cultist = 0,
		MoonLord = 1,
	}

	private static Asset<Texture2D> Highlight = null;
	private static Asset<Texture2D> CultistBack = null;
	private static Asset<Texture2D> MoonLordBack = null;
	private static Asset<Texture2D> PortalMask = null;

	private static Asset<Effect> PortalEffect = null;

	private Domain DomainTarget => (Domain)Projectile.ai[0];
	private ref float Uses => ref Projectile.ai[1];
	private ref float MaxUses => ref Projectile.ai[2];

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.IsInteractable[Type] = true;

		Highlight = ModContent.Request<Texture2D>(Texture + "_Highlight");
		CultistBack = ModContent.Request<Texture2D>(Texture + "CultistBack");
		MoonLordBack = ModContent.Request<Texture2D>(Texture + "MoonBack");
		PortalMask = ModContent.Request<Texture2D>(Texture + "Mask");
		PortalEffect = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/InvertAlphaMask");
	}

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 2;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(224, 224);
		Projectile.Opacity = 0f;
		Projectile.netImportant = true;
		Projectile.aiStyle = -1;

		AIType = -1;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		Projectile.timeLeft++;

		if (NPC.downedMoonlord)
		{
			Projectile.alpha += 3;

			if (Projectile.Opacity < 0.02f)
			{
				Projectile.Kill();
			}

			return;
		}
		else if (NPC.downedAncientCultist)
		{
			Projectile.ai[0] = 1;
		}

		Main.CurrentFrameFlags.HadAnActiveInteractibleProjectile = true;

		Projectile.rotation += 0.15f;
		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.002f);
		Projectile.velocity *= 0.96f;

		if (MaxUses == 0)
		{
			MaxUses = Map.GetBossUseCount();
		}
		
		Lighting.AddLight(Projectile.Center, TorchID.Red);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Vector2 position = Projectile.Center - Main.screenPosition;

		Color backFadedColor = Color.White * (MathF.Sin(Main.GameUpdateCount * 0.03f) * 0.2f + 0.4f) * Projectile.Opacity;
		Main.spriteBatch.Draw(tex, position, null, backFadedColor, Projectile.rotation, tex.Size() / 2f, 1.2f, SpriteEffects.None, 0);
		Color backPortalColor = Color.White * (MathF.Sin(Main.GameUpdateCount * 0.03f + MathHelper.PiOver2) * 0.2f + 0.6f) * Projectile.Opacity;
		Main.spriteBatch.Draw(tex, position, null, backPortalColor, Projectile.rotation, tex.Size() / 2f, 1.1f, SpriteEffects.None, 0);
		Main.spriteBatch.Draw(tex, position, null, Color.White * Projectile.Opacity, Projectile.rotation, tex.Size() / 2f, 1f, SpriteEffects.None, 0);

		Main.spriteBatch.End();

		Vector2 scale = DomainTarget == Domain.Cultist ? new Vector2(0.1f, 0.8f) : new Vector2(0.1f, 0.3f);
		Texture2D backTexture = (DomainTarget == Domain.Cultist ? CultistBack : MoonLordBack).Value;
		Vector2 weirdHardcodedOffset = DomainTarget == Domain.Cultist ? new Vector2(8, 45) : new Vector2(12, 27);

		Matrix trans = Main.GameViewMatrix.TransformationMatrix;
		Effect effect = PortalEffect.Value;
		effect.Parameters["scroll"].SetValue(new Vector2(Main.GameUpdateCount * 0.0008f, 0.22f));
		effect.Parameters["mask"].SetValue(PortalMask.Value);
		effect.Parameters["uvScale"].SetValue(scale);
		effect.Parameters["opacity"].SetValue(Projectile.Opacity);
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, trans);

		Vector2 innerPosition = Projectile.position - Main.screenPosition + weirdHardcodedOffset;
		Main.spriteBatch.Draw(backTexture, innerPosition, null, Color.White * Projectile.Opacity, 0f, tex.Size() / 2f, tex.Size() / backTexture.Size(), SpriteEffects.None, 0);

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, trans);

		this.DrawHighlightAndCheckRightClickInteraction(Highlight.Value, position, lightColor);
		return false;
	}

	public void SaveData(TagCompound tag)
	{
		tag.Add("target", (byte)DomainTarget);
		tag.Add("uses", Uses);
	}

	public void LoadData(TagCompound tag, Projectile projectile)
	{
		projectile.ai[0] = tag.GetByte("target");
		projectile.ai[1] = tag.GetFloat("uses");
	}

	bool IRightClickableProjectile.RightClick(Player player, bool mouseDirectlyOver)
	{
		if (Main.mouseRight && Main.mouseRightRelease)
		{
			if (DomainTarget == Domain.Cultist)
			{
				SubworldSystem.Enter<CultistDomain>();
			}
			else if (DomainTarget == Domain.MoonLord)
			{
				SubworldSystem.Enter<MoonLordDomain>();
			}

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
