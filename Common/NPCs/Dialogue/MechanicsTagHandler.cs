using PathOfTerraria.Common.UI;
using PathOfTerraria.Utilities;
using ReLogic.Graphics;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.NPCs.Dialogue;

#nullable enable

public enum Mechanic : byte
{
	Error,
	Self,
	ElementalDamage,
	Fire,
	Cold,
	Lightning,
	Chaos,
	Ravencrest,
	Mapping, 
	Blocking,
	Affixes,
	MapAffixes,
	ItemAffixes,
	EnemyAffixes,
	AreaLevel,
	ItemRarity,
	GrimoirePickups,
	Questing,
	DamageOverTime,
	Nearby,
	Far,
}

internal class MechanicsTagHandler : ITagHandler
{
	internal class Loader : ILoadable
	{
		public static Dictionary<Mechanic, HashSet<Mechanic>> AssociatedMechanics = [];

		public void Load(Mod mod)
		{
			foreach (Mechanic enu in Enum.GetValues<Mechanic>())
			{
				_ = Language.GetOrRegister("Mods.PathOfTerraria.Mechanics." + enu + ".Tag");
				_ = Language.GetOrRegister("Mods.PathOfTerraria.Mechanics." + enu + ".Description");
			}

			AssociatedMechanics.Add(Mechanic.Fire, [Mechanic.ElementalDamage, Mechanic.DamageOverTime]);
			AssociatedMechanics.Add(Mechanic.Cold, [Mechanic.ElementalDamage]);
			AssociatedMechanics.Add(Mechanic.Lightning, [Mechanic.ElementalDamage]);
			AssociatedMechanics.Add(Mechanic.Chaos, [Mechanic.DamageOverTime]);
			AssociatedMechanics.Add(Mechanic.Affixes, [Mechanic.ItemRarity]);
			AssociatedMechanics.Add(Mechanic.MapAffixes, [Mechanic.Affixes]);
			AssociatedMechanics.Add(Mechanic.ItemAffixes, [Mechanic.Affixes]);
			AssociatedMechanics.Add(Mechanic.EnemyAffixes, [Mechanic.Affixes]);
			AssociatedMechanics.Add(Mechanic.ItemRarity, [Mechanic.Affixes]);

			ChatManager.Register<MechanicsTagHandler>("mechanic");
		}

		public void Unload()
		{
		}
	}

	internal class MechanicsTextSnippet : TextSnippet
	{
		public string Tag
		{
			get
			{
				if (lowercase)
				{
					return Language.GetTextValue("Mods.PathOfTerraria.Mechanics." + mechanic + ".Tag").ToLower();
				}

				return Language.GetTextValue("Mods.PathOfTerraria.Mechanics." + mechanic + ".Tag");
			}
		}

		internal static int CurrentSlot = 0;

		private static bool ShiftHoverDisplay = false;

		private bool AllowShift => shiftIndex >= 0;

		private readonly Mechanic mechanic;
		private readonly Color baseColor;
		private readonly bool lowercase;
		private readonly int shiftIndex;
		private readonly int shiftMax;

		public MechanicsTextSnippet(Mechanic mechanic, Color baseColor, bool lowercase, int shiftIndex, int shiftMax)
		{
			this.mechanic = mechanic;
			this.baseColor = baseColor;
			this.lowercase = lowercase;
			this.shiftIndex = shiftIndex;
			this.shiftMax = shiftMax;

			CheckForHover = true;
			Text = "";
		}

		public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1)
		{
			size = ChatManager.GetStringSize(FontAssets.MouseText.Value, Tag, new Vector2(scale));

			if (justCheckingString)
			{
				return true;
			}

			if (color.R == 0 && color.G == 0 && color.B == 0)
			{
				return true;
			}

			DynamicSpriteFont font = FontAssets.MouseText.Value;
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)position.X - 2, (int)position.Y + (int)size.Y - 10, (int)size.X + 4, 6), Color.Black);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)position.X, (int)position.Y + (int)size.Y - 8, (int)size.X, 2), GetVisibleColor());
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, Tag, position, GetVisibleColor(), Color.Black, 0f, Vector2.Zero, new Vector2(scale), spread: 2f);

			if (AllowShift && !ItemSlot.ShiftInUse)
			{
				Vector2 sc = new Vector2(scale) * 0.7f;
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, "(shift)", position + new Vector2(0, 24), Color.Gray, Color.Black, 0f, Vector2.Zero, sc, spread: 2f);
			}

			return true;
		}

		public override Color GetVisibleColor()
		{
			return Color.Yellow;
		}

		public override float GetStringLength(DynamicSpriteFont font)
		{
			Vector2 size = ChatManager.GetStringSize(FontAssets.MouseText.Value, Tag, new Vector2(1f));
			return size.X;
		}

		public override void Update()
		{
			if (ItemSlot.ShiftInUse && AllowShift && shiftIndex == (int)Math.Abs(CurrentSlot % shiftMax))
			{
				using var _ = ValueOverride.Create(ref ShiftHoverDisplay, true);
				OnHover();
			}
		}

		public override void OnHover()
		{
			string sub = Language.GetTextValue("Mods.PathOfTerraria.Mechanics." + mechanic + ".Description");
			bool hasMechs = Loader.AssociatedMechanics.ContainsKey(mechanic);

			if (hasMechs && !ShiftHoverDisplay)
			{
				if (Main.keyState.PressingShift())
				{
					sub += "\n";

					foreach (Mechanic mech in Loader.AssociatedMechanics[mechanic])
					{
						string desc = Language.GetTextValue("Mods.PathOfTerraria.Mechanics." + mech + ".Description");
						sub += $"\n[c/{GetVisibleColor().Hex3()}:{Language.GetTextValue("Mods.PathOfTerraria.Mechanics." + mech + ".Tag")}:]\n{desc}\n";
					}
				}
				else
				{
					sub += $"\n[c/888888:{Language.GetTextValue($"Mods.{PoTMod.ModName}.Mechanics.Shift")}]";
				}
			}

			string title = Language.GetTextValue("Mods.PathOfTerraria.Mechanics.Mechanic") + ": "
				+ $"[c/FFFF00:{Language.GetTextValue("Mods.PathOfTerraria.Mechanics." + mechanic + ".Tag")}]";

			if (shiftMax > 1)
			{
				title += $" [c/999999:{Language.GetTextValue("Mods.PathOfTerraria.Mechanics.Scroll", shiftIndex + 1, shiftMax)}]";
			}

			Tooltip.Create(new TooltipDescription
			{
				Identifier = "MechanicTag",
				SimpleSubtitle = sub,
				SimpleTitle = ShiftHoverDisplay ? title : null,
				Stability = 0,
			});
		}

		public override string ToString()
		{
			return $"Mech: " + mechanic + " Lowecase: " + lowercase;
		}
	}

	public TextSnippet Parse(string text, Color baseColor = default, string? options = null)
	{
		Mechanic display = Mechanic.Error;

		if (Enum.TryParse(typeof(Mechanic), text, true, out object? parsed) && parsed is Mechanic mechanic)
		{
			display = mechanic;
		}

		int shiftIndex = -1;
		int max = -1;

		if (!string.IsNullOrEmpty(options))
		{
			string[] split = options.Split(',');

			foreach (string parameter in split)
			{
				if (parameter.StartsWith("shift", StringComparison.CurrentCultureIgnoreCase))
				{
					string[] splits = parameter.Split(['=', ';']);

					if (splits.Length < 2)
					{
						shiftIndex = 0;
						break;
					}
					
					if (splits.Length > 1)
					{
						if (int.TryParse(splits[1], out int value))
						{
							shiftIndex = value;
						}
					}

					if (splits.Length > 2)
					{
						if (int.TryParse(splits[2], out int value))
						{
							max = value;
						}
					}

					break;
				}
			}
		}

		return new MechanicsTextSnippet(display, baseColor, text == display.ToString().ToLower(), shiftIndex, max);
	}
}

public class ScrollerSystem : ModSystem
{
	public override void PreUpdateDusts()
	{
		if (Main.dedServ)
		{
			return;
		}

		if (PlayerInput.ScrollWheelDelta > 0)
		{
			MechanicsTagHandler.MechanicsTextSnippet.CurrentSlot++;
		}
		else if (PlayerInput.ScrollWheelDelta < 0)
		{
			MechanicsTagHandler.MechanicsTextSnippet.CurrentSlot--;
		}
	}
}