using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Items.Currency;
using PathOfTerraria.Core.Sounds;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class AugmentSlotElement : UIElement
{
	public const int SquareSize = 180;
	public const int HoverTimeMax = 10;

	public readonly int Index;
	public readonly SkillNode Node;

	public int HoverTime;
	private bool _unlocked;

	public bool ContainsInner
	{
		get
		{
			const int region = 52;

			Vector2 center = GetDimensions().Center();
			var area = new Rectangle((int)(center.X - region / 2), (int)(center.Y - region / 2), region, region);

			return area.Contains(Main.MouseScreen.ToPoint());
		}
	}

	public AugmentSlotElement(int index, bool unlocked = false, SkillNode node = null)
	{
		Index = index;
		Node = node;

		if (Index >= SkillTree.Current.Augments.Count) //Failsafe
		{
			SkillTree.Current.Augments.Add(new(null, unlocked));
		}

		_unlocked = unlocked;

		Width.Set(SquareSize, 0);
		Height.Set(SquareSize, 0);
		Top.Set(100 + SquareSize * index, 0);
		Left.Set(100, 0);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (!_unlocked)
		{
			return;
		}

		if (ContainsInner || HoverTime > 0 && ContainsPoint(Main.MouseScreen))
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
		else if (!ContainsPoint(Main.MouseScreen))
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
		SkillAugment[] augments = [.. SkillTree.Current.Augments.Select(x => x.Augment)];
		Texture2D tex = ModContent.Request<Texture2D>($"{PoTMod.Instance.Name}/Assets/UI/AugmentFrame").Value;

		if (augments[Index] != null)
		{
			augments[Index].Draw(spriteBatch, center, Color.White);

			if (ContainsInner)
			{
				string name = augments[Index].DisplayName;
				string tooltip = augments[Index].Tooltip;

				Tooltip.SetName(name);
				Tooltip.SetTooltip(tooltip);
			}

			return;
		}

		spriteBatch.Draw(tex, center, null, Color.White, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);

		if (!_unlocked)
		{
			Texture2D lockIcon = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/LockIcon").Value;
			spriteBatch.Draw(lockIcon, center, null, Color.Gray, 0, lockIcon.Size() / 2, 1, default, 0);

			if (ContainsInner)
			{
				string common = $"Mods.{PoTMod.Instance.Name}.SkillAugments";
				string tooltip;

				if (Main.LocalPlayer.HasItem(ModContent.ItemType<AugmentationOrb>()))
				{
					tooltip = Language.GetTextValue(common + ".Unlock", ModContent.GetInstance<AugmentationOrb>().DisplayName.Value);
				}
				else
				{
					tooltip = Language.GetTextValue(common + ".CostLine", ModContent.GetInstance<AugmentationOrb>().DisplayName.Value);
				}

				Tooltip.SetName(Language.GetTextValue(common + ".SlotLine"));
				Tooltip.SetTooltip(tooltip);
			}
		}
	}

	private void AddRadial()
	{
		int index = 0;

		foreach (string key in SkillAugment.LoadedAugments.Keys)
		{
			SkillAugment augment = SkillAugment.LoadedAugments[key];

			if (SkillTree.Current.Augments.Any(x => x.Augment == augment) || !CanBeApplied(augment))
			{
				continue;
			}

			Append(new AugmentRadialElement(Vector2.Zero, augment, index));
			index++;
		}

		static bool CanBeApplied(SkillAugment augment)
		{
			Skill skill = Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>().HotbarSkills.Where(x => x is not null && x.GetType() == SkillTree.Current.ParentSkill).FirstOrDefault();
			return skill != default && augment.CanBeApplied(skill);
		}
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		if (_unlocked)
		{
			return;
		}

		Player p = Main.LocalPlayer;
		int orb = ModContent.ItemType<AugmentationOrb>();

		if (p.HasItem(orb))
		{
			p.ConsumeItem(orb);
			SkillTree.PackedAugment a = SkillTree.Current.Augments[Index];

			SkillTree.Current.Augments[Index] = new(a.Augment, true);
			_unlocked = true;

			TreeSoundEngine.PlaySoundForTreeAllocation(1, 1);
		}
	}

	public override void RightClick(UIMouseEvent evt)
	{
		bool hadAugment = SkillTree.Current.Augments[Index].Augment != null;
		SkillTree.PackedAugment a = SkillTree.Current.Augments[Index];

		SkillTree.Current.Augments[Index] = new(null, a.Unlocked);

		if (hadAugment)
		{
			SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath);
			
			if (Node?.CanDeallocate(Main.LocalPlayer) == true)
			{
				Node.OnDeallocate(Main.LocalPlayer);
			}
		}
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
		float distance = (Height.Pixels + 22) * Progress + (float)Math.Sin(Progress * Math.PI) * 30; //The total distance to move
		var newPos = (origin - (Vector2.UnitY * distance).RotatedBy(MathHelper.TwoPi / space * _index)).ToPoint();

		Left.Set(newPos.X - Width.Pixels / 2, 0.5f);
		Top.Set(newPos.Y - Height.Pixels / 2, 0.5f);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Texture2D aIcon = _augment.Texture.Value;
		spriteBatch.Draw(aIcon, GetDimensions().Center(), null, Color.White, 1f - Progress, aIcon.Size() / 2, Progress, default, 0);

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

		if (ContainsPoint(Main.MouseScreen) && Progress == 1)
		{
			string name = _augment.DisplayName;
			string tooltip = _augment.Tooltip;

			Tooltip.SetName(name);
			Tooltip.SetTooltip(tooltip);
		}
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		_flashTimer = 20;

		SkillTree.PackedAugment a = SkillTree.Current.Augments[Handler.Index];
		SkillTree.Current.Augments[Handler.Index] = new(_augment, a.Unlocked);

		TreeSoundEngine.PlaySoundForTreeAllocation(1, 0);

		if (Handler.Node?.CanAllocate(Main.LocalPlayer) == true)
		{
			Handler.Node.OnAllocate(Main.LocalPlayer);
		}
	}
}