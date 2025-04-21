using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using System.Linq;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class AugmentSlotElement : UIElement
{
	public const int HoverTimeMax = 10;

	public readonly int Index;
	public int HoverTime;

	public AugmentSlotElement(int index)
	{
		const int squareSize = 160;
		Index = index;

		Width.Set(squareSize, 0);
		Height.Set(squareSize, 0);
		Top.Set(100 + squareSize * index, 0);
		Left.Set(100, 0);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (ContainsPoint(Main.MouseScreen))
		{
			if (HoverTime == 0)
			{
				AddRadial();
			}

			if (HoverTime < HoverTimeMax)
			{
				HoverTime++;
			}
		}
		else
		{
			if (HoverTime > 0)
			{
				HoverTime--;
			}
			else
			{
				RemoveAllChildren();
			}
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		Vector2 center = GetDimensions().Center();
		SkillAugment[] augments = SkillTree.Current.Augments;

		if (augments[Index] != null)
		{
			augments[Index].Draw(spriteBatch, center, Color.White);
			return;
		}

		Texture2D tex = ModContent.Request<Texture2D>($"{PoTMod.Instance.Name}/Assets/UI/AugmentFrame").Value;
		spriteBatch.Draw(tex, center, null, Color.White, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);
	}

	private void AddRadial()
	{
		int index = 0;
		foreach (string key in SkillAugment.LoadedAugments.Keys)
		{
			SkillAugment augment = SkillAugment.LoadedAugments[key];

			if (SkillTree.Current.Augments.Contains(augment))
			{
				continue;
			}

			Append(new AugmentRadialElement(Vector2.Zero, augment, index));
			index++;
		}
	}

	public override void RightClick(UIMouseEvent evt)
	{
		SkillTree.Current.Augments[Index] = null;
	}
}

internal class AugmentRadialElement : UIElement
{
	private readonly SkillAugment _augment;

	private readonly int _index;
	private int _flashTimer;
	private int _redFlashTimer;

	public AugmentSlotElement Handler
	{
		get
		{
			if (Parent is AugmentSlotElement e)
			{
				return e;
			}

			return new(0);
		}
	}

	private float Progress => (float)Handler.HoverTime / AugmentSlotElement.HoverTimeMax;
	private bool Unlocked => Main.LocalPlayer.GetModPlayer<SkillTreePlayer>().Unlocked(_augment);

	public AugmentRadialElement(Vector2 origin, SkillAugment augment, int index)
	{
		var size = augment.Texture.Size().ToPoint();
		_augment = augment;
		_index = index;

		Width.Set(size.X, 0);
		Height.Set(size.Y, 0);
		Left.Set(origin.X - Width.Pixels / 2, 0.5f);
		Top.Set(origin.Y - Height.Pixels / 2, 0.5f);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		int space = 8;

		Vector2 origin = Vector2.Zero;
		float distance = (Height.Pixels + 12) * Progress;
		var newPos = (origin - (Vector2.UnitY * distance).RotatedBy(MathHelper.TwoPi / space * _index)).ToPoint();

		Left.Set(newPos.X - Width.Pixels / 2, 0.5f);
		Top.Set(newPos.Y - Height.Pixels / 2, 0.5f);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_augment.Draw(spriteBatch, GetDimensions().Center(), Unlocked ? Color.White : Color.Gray);

		if (!Unlocked)
		{
			Texture2D lockIcon = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/LockIcon").Value;
			float scale = 1f + _redFlashTimer / 20f * .25f;

			spriteBatch.Draw(lockIcon, GetDimensions().Center(), null, Color.Gray, scale - 1, lockIcon.Size() / 2, scale, default, 0);
		}

		if (_flashTimer > 0)
		{
			Texture2D glow = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowAlpha").Value;
			Texture2D star = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/StarAlpha").Value;

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
			Texture2D glow = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowAlpha").Value;
			Texture2D star = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/StarAlpha").Value;

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

		if (ContainsPoint(Main.MouseScreen))
		{
			string name = _augment.Name;

			Tooltip.SetName(name);
			Tooltip.SetTooltip(_augment.Tooltip);
		}
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		if (!Unlocked)
		{
			_redFlashTimer = 20;
			//return; //Removed for debug
		}

		_flashTimer = 20;
		SkillTree.Current.Augments[Handler.Index] = _augment;
	}
}