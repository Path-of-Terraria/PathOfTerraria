using PathOfTerraria.Common.Systems.Skills;
using Terraria.Localization;

namespace PathOfTerraria.Common.Mechanics;

/// <summary> The base class for skill specializations. Only one specialization can be selected per skill tree. </summary>
public abstract class SkillSpecial(SkillTree tree) : SkillNode(tree)
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/SkillSpecials/" + Name;
	public override string DisplayName => Language.GetTextValue("Mods.PathOfTerraria.SkillSpecials." + Name + ".Name");
	public override string Tooltip => Language.GetTextValue("Mods.PathOfTerraria.SkillSpecials." + Name + ".Description");

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
		Tree.Specialization = this;
	}

	public override void OnDeallocate(Player player)
	{
		Tree.Specialization = null;
	}

	/// <summary> Whether this skill specialization can be used. </summary>
	public override bool CanAllocate(Player player)
	{
		return Tree.Specialization is null && Connected();

		bool Connected()
		{
			bool value = true;

			foreach (SkillTree.Edge edge in Tree.Edges)
			{
				if (edge.Contains(this))
				{
					if (edge.Other(this) is not SkillPassive p || p.Level > 0)
					{
						return true;
					}

					value = false;
				}
			}

			return value;
		}
	}

	/// <summary> Whether this skill specialization can be refunded. </summary>
	public override bool CanDeallocate(Player player)
	{
		return Tree.Specialization == this;
	}
}