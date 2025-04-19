using PathOfTerraria.Common.Mechanics;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class AugmentSlotElement : UIElement
{
	public const int ClickTimeMax = 10;

	public readonly int Index;
	public int ClickTime;

	public AugmentSlotElement(int index)
	{
		const int squareSize = 140;
		Index = index;

		Width.Set(squareSize, 0);
		Height.Set(squareSize, 0);
		Top.Set(100 + squareSize * index, 0);
		Left.Set(100, 0);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (!ContainsPoint(Main.MouseScreen))
		{
			if (ClickTime < ClickTimeMax)
			{
				ClickTime++;
			}
			else
			{
				RemoveAllChildren();
			}
		}
		else
		{
			if (ClickTime > 0)
			{
				ClickTime--;
			}
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		Vector2 center = GetDimensions().Center();
		SkillAugment[] augments = Allocatable.ViewedSkill.Tree.Augments;

		if (augments[Index] != null)
		{
			augments[Index].Draw(spriteBatch, center);
			return;
		}

		Texture2D tex = ModContent.Request<Texture2D>($"{PoTMod.Instance.Name}/Assets/UI/PassiveFrame").Value;
		spriteBatch.Draw(tex, center, null, Color.White, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		RemoveAllChildren();
		AddRadial();

		ClickTime = ClickTimeMax;
	}

	private void AddRadial()
	{
		int index = 0;
		foreach (string key in SkillAugment.LoadedAugments.Keys)
		{
			Append(new AugmentRadialElement(Vector2.Zero, SkillAugment.LoadedAugments[key], index));
			index++;
		}
	}

	public override void RightClick(UIMouseEvent evt)
	{
		Allocatable.Tree.Augments[Index] = null;
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

	private float Progress => 1f - (float)Handler.ClickTime / AugmentSlotElement.ClickTimeMax;

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
		_augment.Draw(spriteBatch, GetDimensions().Center());

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

		//Recalculate();
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		_flashTimer = 20;
		Allocatable.Tree.Augments[Handler.Index] = _augment;
	}

	public override void RightClick(UIMouseEvent evt)
	{
		_redFlashTimer = 20;
	}
}