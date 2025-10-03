using PathOfTerraria.Common.Conflux;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config;

#nullable enable

namespace PathOfTerraria.Content.Conflux;

internal enum ConfluxRiftKind : byte
{
	Ice,
	Flames,
	Lightning,
	Count,
}

internal sealed class ConfluxRift : ModProjectile, IRightClickableProjectile
{
	private static Asset<Texture2D> Highlight = null!;

	public ConfluxRiftKind Kind
	{
		get => (ConfluxRiftKind)(byte)Projectile.ai[0];
		set => Projectile.ai[0] = (byte)value;
	}

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.IsInteractable[Type] = true;

		if (!Main.dedServ)
		{
			Highlight = ModContent.Request<Texture2D>(Texture + "_Highlight");
		}
	}

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 2;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(120, 120);
		Projectile.Opacity = 0f;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override bool PreAI()
	{
		Projectile.timeLeft++;
		return base.PreAI();
	}

	public override void AI()
	{
		Projectile.rotation += 0.05f;
		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.25f);

		if (!Main.dedServ)
		{
			(int torchId, int dustId, _) = GetVisualParameters();

			if (Main.rand.NextBool(10))
			{
				Dust.NewDust(Projectile.position + new Vector2(8), Projectile.width - 16, Projectile.height - 16, dustId);
			}

			Lighting.AddLight(Projectile.Center, torchId);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Vector2 position = Projectile.Center - Main.screenPosition;

		(_, _, Color colorBase) = GetVisualParameters();

		for (int i = 0; i < 3; ++i)
		{
			float rotation = (Projectile.rotation * (i % 2 == 0 ? -1 : 1)) + (i * 1.5f);
			Color color = colorBase * ((3 - i) * 0.33f) * Projectile.Opacity;

			Main.spriteBatch.Draw(tex, position, null, color, rotation, tex.Size() / 2f, 1.25f - (i * 0.25f), SpriteEffects.None, 0);
		}

		this.DrawHighlightAndCheckRightClickInteraction(Highlight.Value, position, lightColor);

		return false;
	}

	private (int torchId, int dustId, Color colorBase) GetVisualParameters()
	{
		return Kind switch
		{
			ConfluxRiftKind.Ice => (TorchID.Ice, DustID.Firework_Blue, Color.AliceBlue),
			ConfluxRiftKind.Flames => (TorchID.Red, DustID.Firework_Red, Color.OrangeRed),
			ConfluxRiftKind.Lightning => (TorchID.Purple, DustID.WitherLightning, Color.MediumVioletRed),
			_ => throw new NotImplementedException(),
		};
	}

	bool IRightClickableProjectile.RightClick(Player player, bool mouseDirectlyOver)
	{
		if (Main.mouseRight && Main.mouseRightRelease)
		{
			ConfluxRifts.ActivateRift(Projectile);
			//Projectile.active = false;
			return true;
		}

		if (mouseDirectlyOver)
		{
			Tooltip.Create(new TooltipDescription
			{
				Identifier = "ConfluxRift",
				SimpleTitle = Language.GetTextValue($"Mods.{PoTMod.ModName}.Misc.Enter"),
			});
		}

		return false;
	}
}