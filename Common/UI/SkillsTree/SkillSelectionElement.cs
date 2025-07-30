using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.UI.Guide;
using PathOfTerraria.Common.UI.Hotbar;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class SkillSelectionElement : UIElement
{
	private readonly Skill _skill;
	private readonly SkillSelectionPanel _parentPanel;

	public SkillSelectionElement(Skill skill, int index, SkillSelectionPanel parentPanel)
	{
		_skill = skill;
		_parentPanel = parentPanel;
		_skill.LevelTo(1);
		Width.Set(skill.Size.X, 0);
		Height.Set(skill.Size.Y, 0);
		Top.Set(60, 0);
		Left.Set(25 + 75 * index, 0);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		Texture2D tex = ModContent.Request<Texture2D>(_skill.Texture).Value;

		if (tex == null)
		{
			return;
		}

		if (ContainsPoint(Main.MouseScreen))
		{
			if (_skill.CanEquipSkill(Main.LocalPlayer, out string failReason))
			{
				NewHotbar.DrawSkillHoverTooltips(_skill, null, true);
			}
			else
			{
				Tooltip.SetName(Language.GetTextValue("Mods.PathOfTerraria.Skills.CantEquip", failReason));
			}
		}

		Vector2 position = GetDimensions().Position() + new Vector2(Width.Pixels / 2, Height.Pixels / 2);
		spriteBatch.Draw(tex, position, null, _skill.CanEquipSkill(Main.LocalPlayer, out _) ? Color.White : Color.Gray, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);

		if (Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>().HasSkill(_skill.Name))
		{
			Vector2 texPos = position - new Vector2(0, Height.Pixels / 2f) * 0.8f;
			string text = $"{_skill.Level}/{_skill.MaxLevel}";
			Vector2 textSize = FontAssets.ItemStack.Value.MeasureString(text);
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, text, texPos, Color.White, 0f, textSize / 2f, Vector2.One);
		}
	}
	
	public override void LeftClick(UIMouseEvent evt)
	{
		SkillCombatPlayer skillCombatPlayer = Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>();

		if (skillCombatPlayer.TryAddSkill(_skill))
		{
			_parentPanel.SelectedSkill = _skill;
			_parentPanel.RebuildTree();

			Main.LocalPlayer.GetModPlayer<TutorialPlayer>().TutorialChecks.Add(TutorialCheck.SelectedSkill);
		}
	}

	public override void RightClick(UIMouseEvent evt)
	{
		SkillCombatPlayer skillCombatPlayer = Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>();
		skillCombatPlayer.TryRemoveSkill(_skill);
	}
}