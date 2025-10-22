using PathOfTerraria.Common.UI;
using ReLogic.Graphics;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.NPCs.Dialogue;

internal class MechanicsTagHandler : ITagHandler
{
	internal class MechanicsTextSnippet : TextSnippet
	{
		private readonly Mechanic mechanic;
		private readonly Color baseColor;

		public MechanicsTextSnippet(Mechanic mechanic, Color baseColor)
		{
			this.mechanic = mechanic;
			this.baseColor = baseColor;

			CheckForHover = true;
			Text = "";
		}

		public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1)
		{
			string tag = Language.GetTextValue("Mods.PathOfTerraria.Mechanics." + mechanic + ".Tag");
			size = ChatManager.GetStringSize(FontAssets.MouseText.Value, tag, new Vector2(scale));

			if (justCheckingString)
			{
				return true;
			}

			DynamicSpriteFont font = FontAssets.MouseText.Value;
			ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, tag, position, baseColor, Color.Black * 0.67f, 0f, Vector2.Zero, new Vector2(scale));
			return true;
		}

		public override float GetStringLength(DynamicSpriteFont font)
		{
			string tag = Language.GetTextValue("Mods.PathOfTerraria.Mechanics." + mechanic + ".Tag");
			Vector2 size = ChatManager.GetStringSize(FontAssets.MouseText.Value, tag, new Vector2(1f));
			return size.X;
		}

		public override void OnHover()
		{
			Tooltip.Create(new TooltipDescription
			{
				Identifier = "MechanicTag",
				SimpleSubtitle = Language.GetTextValue("Mods.PathOfTerraria.Mechanics." + mechanic + ".Description")
			});
		}
	}

	public enum Mechanic : byte
	{
		Error,
		Self,
		ElementalDamage,
		Fire,
		Cold,
		Lightning,
		Chaos,
		Ravencrest
	}

	public TextSnippet Parse(string text, Color baseColor = default, string options = null)
	{
		Mechanic display = Mechanic.Error;

		if (Enum.TryParse(typeof(Mechanic), text, out object parsed) && parsed is Mechanic mechanic)
		{
			display = mechanic;
		}

		return new MechanicsTextSnippet(display, baseColor);
	}
}

internal class MechanicsTagLocalizationGenerator : ILoadable
{
	public void Load(Mod mod)
	{
		foreach (MechanicsTagHandler.Mechanic enu in Enum.GetValues<MechanicsTagHandler.Mechanic>())
		{
			_ = Language.GetOrRegister("Mods.PathOfTerraria.Mechanics." + enu + ".Tag");
			_ = Language.GetOrRegister("Mods.PathOfTerraria.Mechanics." + enu + ".Description");
		}
	}

	public void Unload()
	{
	}
}
