using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.TreeSystem;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

internal class PassiveElement : SmartUIElement
{
	private readonly Passive _passive;
	public Vector2 Root;

	private int _flashTimer;
	private int _redFlashTimer;

	public PassiveElement(Passive passive)
	{
		_passive = passive;
		Left.Set(passive.TreePos.X - passive.Width / 2, 0);
		Top.Set(passive.TreePos.Y - passive.Height / 2, 0);
		Width.Set(passive.Width, 0);
		Height.Set(passive.Height, 0);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_passive.Draw(spriteBatch, GetDimensions().Center());

		if (_flashTimer > 0)
		{
			Texture2D glow = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GlowAlpha").Value;
			Texture2D star = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/StarAlpha").Value;

			float prog = _flashTimer / 20f;

			var glowColor = new Color(255, 230, 150)
			{
				A = 0
			};

			glowColor *= prog * 0.5f;

			spriteBatch.Draw(glow, GetDimensions().Center(), null, glowColor, 0, glow.Size() / 2f, 1 + (1f - prog), 0, 0);
			spriteBatch.Draw(star, GetDimensions().Center(), null, glowColor * 0.5f, 0, star.Size() / 2f, 1 + (1f - prog), 0, 0);

			_flashTimer--;
		}

		if (_redFlashTimer > 0)
		{
			Texture2D glow = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GlowAlpha").Value;
			Texture2D star = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/StarAlpha").Value;

			float prog = _redFlashTimer / 20f;

			var glowColor = new Color(255, 60, 60)
			{
				A = 0
			};

			glowColor *= prog * 0.5f;

			spriteBatch.Draw(glow, GetDimensions().Center(), null, glowColor, 0, glow.Size() / 2f, 1 + (1f - prog), 0, 0);
			spriteBatch.Draw(star, GetDimensions().Center(), null, glowColor * 0.5f, 0, star.Size() / 2f, 1 + prog, 0, 0);

			_redFlashTimer--;
		}

		if (IsMouseHovering)
		{
			string name = _passive.Name;

			if (_passive.MaxLevel > 1)
			{
				name += $" ({_passive.Level}/{_passive.MaxLevel})";
			}

			Tooltip.SetName(name);
			Tooltip.SetTooltip(_passive.Tooltip);
		}

		Recalculate();
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		if (!_passive.CanAllocate(Main.LocalPlayer))
		{
			return;
		}

		_passive.Level++;
		Main.LocalPlayer.GetModPlayer<TreePlayer>().Points--;

		_flashTimer = 20;

		switch (_passive.MaxLevel)
		{
			case 1:
				switch (_passive.Level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
				}

				return;
			case 2:
				switch (_passive.Level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier2")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
				}

				return;
			case 3:
				switch (_passive.Level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier3")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
				}

				return;
			case 5:
				switch (_passive.Level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier2")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier3")); break;
					case 4: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier4")); break;
					case 5: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
				}

				return;
			case 6:
				switch (_passive.Level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier2")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier3")); break;
					case 4: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier4")); break;
					case 5: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
					case 6: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier1")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
				}

				return;
			case 7:
				switch (_passive.Level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier2")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier3")); break;
					case 4: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier4")); break;
					case 5: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
					case 6: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier2")); break;
					case 7: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier1")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
				}

				return;
			default:
				switch (_passive.Level)
				{
					case 1: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier1")); break;
					case 2: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier2")); break;
					case 3: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier3")); break;
					case 4: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier4")); break;
					case 5: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
					default: SoundEngine.PlaySound(new SoundStyle($"{PathOfTerraria.ModName}/Sounds/Tier5")); break;
				}

				break;
		}
	}

	public override void SafeRightClick(UIMouseEvent evt)
	{
		if (!_passive.CanDeallocate(Main.LocalPlayer))
		{
			return;
		}

		_passive.Level--;
		Main.LocalPlayer.GetModPlayer<TreePlayer>().Points++;

		_redFlashTimer = 20;

		SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath);
	}
}