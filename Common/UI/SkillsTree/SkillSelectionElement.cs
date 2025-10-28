using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.UI.Hotbar;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class SkillSelectionElement : UIElement
{
	public delegate void OnClick(UIElement parent, SkillSelectionElement self);

	internal readonly Skill Skill;

	private readonly OnClick _onClick;

	public SkillSelectionElement(Skill skill, OnClick click)
	{
		Skill = skill;
		_onClick = click;
		Skill.LevelTo(1);

		Width.Set(skill.Size.X, 0);
		Height.Set(skill.Size.Y, 0);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		Texture2D tex = ModContent.Request<Texture2D>(Skill.Texture).Value;
		SkillFailure fail = default;

		if (tex == null)
		{
			return;
		}

		if (ContainsPoint(Main.MouseScreen))
		{
			if (Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>().HasSkill(Skill.Name) || Skill.CanEquipSkill(Main.LocalPlayer, ref fail))
			{
				NewHotbar.DrawSkillHoverTooltips(Skill, null, true);
			}
			else
			{
				Tooltip.Create(new TooltipDescription
				{
					Identifier = "SkillSelection",
					SimpleTitle = Language.GetTextValue("Mods.PathOfTerraria.Skills.CantEquip", fail.Description),
				});
			}
		}

		Vector2 position = GetDimensions().Position() + new Vector2(Width.Pixels / 2, Height.Pixels / 2);
		Color color = Skill.CanEquipSkill(Main.LocalPlayer, ref fail) ? Color.White : new Color(50, 50, 50);
		spriteBatch.Draw(tex, position, null, color, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);

		if (Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>().HasSkill(Skill.Name))
		{
			Vector2 texPos = position - new Vector2(0, Height.Pixels / 2f) * 0.8f;
			string text = $"{Skill.Level}/{Skill.MaxLevel}";
			Vector2 textSize = FontAssets.ItemStack.Value.MeasureString(text);
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, text, texPos, Color.White, 0f, textSize / 2f, Vector2.One);
		}
	}
	
	public override void LeftClick(UIMouseEvent evt)
	{
		_onClick.Invoke(Parent, this);
	}

	public override void RightClick(UIMouseEvent evt)
	{
		SkillCombatPlayer skillCombatPlayer = Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>();
		skillCombatPlayer.TryRemoveSkill(Skill);
	}
}