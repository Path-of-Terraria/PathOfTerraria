using PathOfTerraria.Common.Systems.Skills;
using Terraria.Localization;

namespace PathOfTerraria.Common.Mechanics;

/// <summary> The base class for skill specializations. Only one specialization can be selected per skill tree. </summary>
public abstract class SkillSpecial(SkillTree tree) : SkillNode(tree)
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/SkillSpecials/" + Name;
	public override string DisplayName => Language.GetOrRegister("Mods.PathOfTerraria.SkillSpecials." + Name + ".Name").Value;
	public override string DisplayTooltip
	{
		get
		{
			string tooltip = Language.GetOrRegister("Mods.PathOfTerraria.SkillSpecials." + Name + ".Description").Value;
			tooltip += "\n\n" + Language.GetTextValue($"Mods.{PoTMod.Instance.Name}.SkillSpecials.OneLine");

			return tooltip;
		}
	}

	public override void Draw(SpriteBatch spriteBatch, Vector2 position)
	{
		Texture2D texture = Texture.Value;
		Color color = Color.Gray;

		if (CanAllocate(Main.LocalPlayer))
		{
			color = Color.Lerp(Color.Gray, Color.White, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);
		}

		if (Tree.Specialization == this)
		{
			color = Color.White;
		}

		spriteBatch.Draw(texture, position, null, color, 0, texture.Size() / 2, 1, default, 0);

		if (Tree.Specialization == this)
		{
			float pulse = (float)Math.Sin(Main.timeForVisualEffects / 50f) * .25f + .5f;

			Texture2D glow = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowAlpha").Value;
			var glowColor = new Color(255, 230, 150) { A = 0 };
			spriteBatch.Draw(glow, position, null, glowColor * pulse, 0, glow.Size() / 2, 1, default, 0);

			Texture2D outline = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/SpecialFrame_Outline").Value;
			spriteBatch.Draw(outline, position, null, Color.White * pulse, 0, outline.Size() / 2, 1, default, 0);
		}
	}

	public override void OnAllocate(Player player)
	{
		base.OnAllocate(player);
		player.GetModPlayer<SkillTreePlayer>().SetSpecializationForSkill(Tree.ParentSkill, this);
		Tree.Specialization = this;
	}

	public override void OnDeallocate(Player player)
	{
		base.OnDeallocate(player);
		player.GetModPlayer<SkillTreePlayer>().SpecializationsBySkill.Remove(Tree.ParentSkill);
		Tree.Specialization = null;
	}

	/// <summary> Whether this skill specialization can be used. </summary>
	public override bool CanAllocate(Player player)
	{
		return Connections > 0 && Tree.Specialization is null;
	}

	/// <summary> Whether this skill specialization can be refunded. </summary>
	public override bool CanDeallocate(Player player)
	{
		return Connections < 2 && Tree.Specialization == this;
	}
}