using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.BlockSystem;
using PathOfTerraria.Common.Systems.Charges;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.PlayerStats;

internal class PlayerStatInnerPanel : SmartUiElement
{
	private class PlayerStatUI : UIText
	{
		public readonly int Slot = 0;

		private readonly LocalizedText text = null;
		private readonly Func<Player, string> getValue = null;
		private readonly bool noColon = false;
		private readonly LocalizedText hover = null;

		public PlayerStatUI(LocalizedText text, Func<Player, string> getValue, float scale = 1f, bool big = false, bool noColon = false, 
			LocalizedText hover = null, bool isHeader = false) : base("", scale, big)
		{
			this.text = text;
			this.getValue = getValue;
			this.noColon = noColon;
			this.hover = hover;
			
			if (isHeader)
			{
				this.TextColor = Color.Gold;
				scale = Math.Max(scale, 1.2f);
				noColon = true; // Headers shouldn't have colons
        
				this.MarginTop = 10;
			}


			Slot = SlotNumber++;
			HAlign = 0.5f;
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			SetText(text.Value + (noColon ? "" : ": ") + getValue(Main.LocalPlayer));

			if (hover is not null && ContainsPoint(Main.MouseScreen))
			{
				Tooltip.SetName(text.Value);
				Tooltip.SetTooltip(hover.Value);
			}
		}
	}

	public static Asset<Texture2D> ChainTex = null;
	public static Asset<Texture2D> BackTex = null;

	private static int SlotNumber = 0;

	private UIElement Panel => Parent;

	public override string TabName => "PlayerStats";

	private UICharacter _drawDummy = null;
	private int _offset = 0;

	public PlayerStatInnerPanel()
	{
		ChainTex ??= ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/PlayerStatBackChain");
		BackTex ??= ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/PlayerStatBack");

		UIElement panel = new()
		{
			Width = StyleDimension.FromPixels(512),
			Height = StyleDimension.FromPixels(640),
			VAlign = 0.5f,
			HAlign = 0.5f
		};
		Append(panel);

		UIList list = new()
		{
			Width = StyleDimension.FromPixels(386),
			Height = StyleDimension.FromPixels(420),
			Left = StyleDimension.FromPixels(68),
			Top = StyleDimension.FromPixels(166),
			ListPadding = 12,
		};

		panel.Append(list);

		list.ManualSortMethod = (list) => list.Sort((x, y) =>
			{
				if (x is not PlayerStatUI xStat)
				{
					return -1;
				}
				else if (y is not PlayerStatUI yStat)
				{
					return 1;
				}
				else
				{
					return xStat.Slot.CompareTo(yStat.Slot);
				}
			});

		UIScrollbar bar = new()
		{
			Width = StyleDimension.FromPixels(24),
			Height = StyleDimension.FromPixels(420),
			HAlign = 1f,
			Left = StyleDimension.FromPixels(-36),
			Top = StyleDimension.FromPixels(166),
		};

		list.SetScrollbar(bar);

		SlotNumber = 0;

		list.Add(new UIElement() { Height = StyleDimension.FromPixels(4) }); // Stops the name text from being cut off
		list.Add(new PlayerStatUI(LocalizedText.Empty, player => player.name, 0.8f, true, true));
		
		list.Add(new PlayerStatUI(GetLocalization("CharacterHeader"), player => "", isHeader: true));
		list.Add(new PlayerStatUI(GetLocalization("Level"), player => player.GetModPlayer<ExpModPlayer>().Level.ToString()));
		list.Add(new PlayerStatUI(GetLocalization("Experience"), player =>
		{
			ExpModPlayer expPlayer = Main.LocalPlayer.GetModPlayer<ExpModPlayer>();
			float expPercent = expPlayer.Exp / (float)expPlayer.NextLevel * 100;
			return $"{expPlayer.Exp}/{expPlayer.NextLevel} ({expPercent:#0.##}%)";
		}));
		
		// Defense
		list.Add(new PlayerStatUI(GetLocalization("DefenseHeader"), player => "", isHeader: true));
		list.Add(new PlayerStatUI(GetLocalization("Life"), player =>
		{
			float lifePercent = Main.LocalPlayer.statLife / Main.LocalPlayer.statLifeMax2 * 100;
			return $"{Main.LocalPlayer.statLife}/{Main.LocalPlayer.statLifeMax2} ({lifePercent:#0.##}%)";
		}));
		list.Add(new PlayerStatUI(GetLocalization("Mana"), player =>
		{
			float manaPercent = Main.LocalPlayer.statMana / Main.LocalPlayer.statManaMax * 100;
			return $"{Main.LocalPlayer.statMana}/{Main.LocalPlayer.statManaMax2} ({manaPercent:#0.##}%)";
		}));
		list.Add(new PlayerStatUI(GetLocalization("DamageReduction"), player => $"{player.endurance * 100:#0.##}%"));
		list.Add(new PlayerStatUI(GetLocalization("BlockChance"), player => $"{player.GetModPlayer<BlockPlayer>().ActualBlockChance * 100:#0.##}%"));
		list.Add(new PlayerStatUI(GetLocalization("MaxBlock"), player => $"{player.GetModPlayer<BlockPlayer>().MaxBlockChance * 100:#0.##}%"));
		list.Add(new PlayerStatUI(GetLocalization("BlockCooldown"), player => $"{player.GetModPlayer<BlockPlayer>().BlockCooldown / 60:#0.##}s"));
		list.Add(new PlayerStatUI(GetLocalization("FireResistance"), player => $"{player.GetModPlayer<ElementalPlayer>().FireResistance * 100:#0.##}%"));
		list.Add(new PlayerStatUI(GetLocalization("ColdResistance"), player => $"{player.GetModPlayer<ElementalPlayer>().ColdResistance * 100:#0.##}%"));
		list.Add(new PlayerStatUI(GetLocalization("LightningResistance"), player => $"{player.GetModPlayer<ElementalPlayer>().LightningResistance * 100:#0.##}%"));
		// Offense  
		list.Add(new PlayerStatUI(GetLocalization("OffenseHeader"), player => "", isHeader: true));
		list.Add(new PlayerStatUI(GetLocalization("CriticalChance"), player => $"{player.GetTotalCritChance(DamageClass.Generic):#0.##}%"));
		
		// Attributes
		list.Add(new PlayerStatUI(GetLocalization("AttributesHeader"), player => "", isHeader: true));
		list.Add(new PlayerStatUI(GetLocalization("Strength"), player => $"{player.GetModPlayer<AttributesPlayer>().Strength:#0.##}", hover: GetHelp("Strength")));
		list.Add(new PlayerStatUI(GetLocalization("Dexterity"), player => $"{player.GetModPlayer<AttributesPlayer>().Dexterity:#0.##}", hover: GetHelp("Dexterity")));
		list.Add(new PlayerStatUI(GetLocalization("Intelligence"), player => $"{player.GetModPlayer<AttributesPlayer>().Intelligence:#0.##}", hover: GetHelp("Intelligence")));
		
		// Charges
		list.Add(new PlayerStatUI(GetLocalization("ChargesHeader"), player => "", isHeader: true));
		list.Add(new PlayerStatUI(GetLocalization("FrenzyChargeChance"), player => $"{player.GetModPlayer<FrenzyChargePlayer>().ChargeGainChance:#0.##}%"));
		list.Add(new PlayerStatUI(GetLocalization("PowerChargeChance"), player => $"{player.GetModPlayer<PowerChargePlayer>().ChargeGainChance:#0.##}%"));
		list.Add(new PlayerStatUI(GetLocalization("EnduranceChargeChance"), player => $"{player.GetModPlayer<EnduranceChargePlayer>().ChargeGainChance:#0.##}%"));
		
		// Misc
		list.Add(new PlayerStatUI(GetLocalization("MiscHeader"), player => "", isHeader: true));
		list.Add(new PlayerStatUI(GetLocalization("MaxMinions"), player => $"{player.maxMinions.ToString()}"));
		list.Add(new PlayerStatUI(GetLocalization("HealthPotions"), player =>
		{
			PotionPlayer potionPlayer = Main.LocalPlayer.GetModPlayer<PotionPlayer>();
			return $"{potionPlayer.HealingLeft}/{potionPlayer.MaxHealing}";
		}));
		list.Add(new PlayerStatUI(GetLocalization("ManaPotions"), player =>
		{
			PotionPlayer potionPlayer = Main.LocalPlayer.GetModPlayer<PotionPlayer>();
			return $"{potionPlayer.ManaLeft}/{potionPlayer.MaxMana}";
		}));


		static LocalizedText GetLocalization(string type)
		{
			return Language.GetText($"Mods.{PoTMod.ModName}.UI.StatUI." + type);
		}

		static LocalizedText GetHelp(string type)
		{
			return Language.GetText($"Mods.{PoTMod.ModName}.UI.StatUI.Help." + type);
		}
	}

	public override void SafeMouseOver(UIMouseEvent evt)
	{
		Main.blockMouse = true;
		Main.isMouseLeftConsumedByUI = true;
		Main.LocalPlayer.mouseInterface = true;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_offset = -3;
		
		DrawBack(spriteBatch);
		base.Draw(spriteBatch);
		SetAndDrawPlayer(spriteBatch);

#if DEBUG
		GUIDebuggingTools.DrawGuiBorder(spriteBatch, this, Color.LavenderBlush);
#endif
	}

	private void DrawBack(SpriteBatch spriteBatch)
	{
		Texture2D chain = ChainTex.Value;
		Texture2D tex = BackTex.Value;
		Vector2 origin = GetPanelRectangle().Center() + new Vector2(0, -8);

		for (int i = 0; i < 9; ++i)
		{
			Color color = Color.White;
			float yOff = i * chain.Height + tex.Height / 2.2f;

			if (i > 4)
			{
				color *= 1 - (i - 4) / 5f;
			}

			spriteBatch.Draw(chain, origin - new Vector2(180, yOff), null, color, 0f, chain.Size() / 2f, 1f, SpriteEffects.None, 0);
			spriteBatch.Draw(chain, origin - new Vector2(-180, yOff), null, color, 0f, chain.Size() / 2f, 1f, SpriteEffects.None, 0);
		}

		spriteBatch.Draw(tex, origin, null, Color.White, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
	}

	private void SetAndDrawPlayer(SpriteBatch spriteBatch)
	{
		if (_drawDummy == null)
		{
			_drawDummy = new UICharacter(Main.LocalPlayer, true, true, 1, true)
			{
				Width = StyleDimension.FromPixels(60),
				Height = StyleDimension.FromPixels(60),
				HAlign = 0.5f,
				Top = StyleDimension.FromPixels(70)
			};
			Append(_drawDummy);
			Recalculate();
		}

		// The correct approach at correctly rendering the player in the stat
		// panel would be the following (with drawingPlayer defined):
		/*private sealed class StatPanelRendererPlayer : ModPlayer
		{
			public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
			{
				base.DrawEffects(drawInfo, ref r, ref g, ref b, ref a, ref fullBright);

				// Force fullBright if rendering the player so hair draws
				// correctly.
				if (drawingPlayer)
				{
					fullBright = true;
				}
			}
		}*/
		// We aren't so lucky, though. Instead, we have to trick
		// Lighting::GetColor into thinking we're in the game menu so it always
		// returns White instead of the real color. This emulates full-bright
		// (ignores in-world lighting) until we can correctly set the
		// `fullBright` parameter in Player::DrawEffects (see: TML-4317).

		bool origGameMenu = Main.gameMenu;
		Main.gameMenu = true;
		_drawDummy.Draw(spriteBatch);
		Main.gameMenu = origGameMenu;
	}

	private void DrawSingleStat(SpriteBatch spriteBatch, string text)
	{
		Utils.DrawBorderStringBig(spriteBatch, text, GetPanelRectangle().Center() + new Vector2(0, -30 + 27 * _offset), Color.White, 0.45f, 0.5f, 0.35f);
		_offset++;
	}

	private Rectangle GetPanelRectangle()
	{
		return Panel.GetDimensions().ToRectangle();
	}
}