using System.Collections.Generic;
using System.Linq;
using System.Text;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.UI.Components;
using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Common.UI.Quests;
using PathOfTerraria.Core.UI;
using PathOfTerraria.Core.UI.SmartUI;
using PathOfTerraria.Utilities.Xna;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;

#nullable enable
#pragma warning disable IDE0053 // Use expression body for lambda expression

namespace PathOfTerraria.Common.Quests;

[Autoload(true, Side = ModSide.Client)]
internal sealed class QuestDebugging : ModSystem
{
#if DEBUG || STAGING
	private static ModKeybind keyToggleQuestDebugging = null!;

	public override void Load()
	{
		keyToggleQuestDebugging = KeybindLoader.RegisterKeybind(Mod, "ToggleQuestDebugging", Microsoft.Xna.Framework.Input.Keys.NumPad1);
	}

	public override void PreUpdateEntities()
	{
		if (keyToggleQuestDebugging.JustPressed)
		{
			SmartUiLoader.GetUiState<QuestDebugState>().Toggle();
		}
	}
#endif
}

internal sealed class QuestDebugCommand : ModCommand
{
	public override string Command => "potQuestDebug";
	public override CommandType Type => CommandType.Chat;

	public override void Action(CommandCaller caller, string input, string[] args)
	{
#if !DEBUG && !STAGING
		// Prevent crazy cheating in MP, unless the user is at least on a debug build.
		if (Main.netMode != NetmodeID.SinglePlayer)
		{
			throw new UsageException("This command is only available in singleplayer.");
		}
#endif

		SmartUiLoader.GetUiState<QuestDebugState>().Toggle();
	}
}

public sealed class QuestDebugState : SmartUiState
{
	private string? lastSearchString;

	public UIPanel Panel { get; private set; } = null!;
	public UIList QuestList { get; private set; } = null!;
	public UIScrollbar QuestScrollbar { get; private set; } = null!;
	public UIEditableText SearchBar { get; private set; } = null!;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Cursor")) - 1;
	}

	public void Toggle()
	{
		SetEnabled(!Visible);
	}

	public void SetEnabled(bool value)
	{
		if (Visible == value)
		{
			return;
		}

		Visible = value;

		if (Visible)
		{
			Rebuild();
		}
		else
		{
			RemoveAllChildren();
		}
	}

	public void Rebuild()
	{
		RemoveAllChildren();

		const float StartX = 8f;
		const float StartY = 40f;

		Panel = this.AddElement(new UIPanel(), e =>
		{
			e.MinWidth.Set(+512, 0f);
			e.MinHeight.Set(+300, 0f);

			if (Panel != null)
			{
				(e.Left, e.Top, e.Width, e.Height) = (Panel.Left, Panel.Top, Panel.Width, Panel.Height);
			}
			else
			{
				e.SetDimensions(x: (0.5f, -256), y: (0.10f, +0), width: (0.0f, +512), height: (0.80f, +0));
			}

			e.AddComponent(new UIMouseDrag(canMove: true, canResize: true));
		});

		// Header
		Panel.AddElement(new UIText("Quest Debugging"), e =>
		{
			e.SetDimensions(x: (0.0f, +StartX), y: (0.00f, +4));
			e.IgnoresMouseInteraction = true;
		});

		// Close button.
		Panel.AddElement(new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton")), e =>
		{
			e.SetDimensions(x: (1.0f, -35), y: (0.00f, -3), width: (0.0f, +38), height: (0f, +38));
			e.SetVisibility(1f, 0.8f);
			e.OnLeftClick += (evt, self) => SetEnabled(false);
		});

		// Search bar

		UIPanel searchBG = Panel.AddElement(new UIPanel(), e =>
		{
			e.SetDimensions(x: (0.0f, +160), y: (0.00f, +0), width: (1.0f, -200), height: (0f, +32));
		});
		SearchBar = Panel.AddElement(SearchBar ?? new UIEditableText(backingText: "Search..."), e =>
		{
			e.SetDimensions(x: (0.0f, +160 + 8f), y: (0.00f, +0), width: (1.0f, -200 - 16f), height: (0f, +32));

			if (SearchBar != null)
			{
				return;
			}

			e.CurrentValue = lastSearchString;
			e.OnUpdate += uiElement =>
			{
				if (uiElement is UIEditableText e && e.CurrentValue != lastSearchString)
				{
					Main.QueueMainThreadAction(Rebuild);
					lastSearchString = e.CurrentValue;
				}
			};
		});

		// List

		QuestList = Panel.AddElement(new UIList(), e =>
		{
			e.SetDimensions(x: (0.00f, +StartX), y: (0.00f, +StartY), width: (1.00f, -48), height: (1.00f, -64));
		});
		QuestScrollbar = Panel.AddElement(new FixedUIScrollbar(UserInterface), e =>
		{
			e.SetDimensions(x: (1.00f, -26), y: (0.00f, +StartY + 6), width: (0.00f, +20), height: (1.00f, -80));
		});
		QuestList.SetScrollbar(QuestScrollbar);

		// List entries

		int questIndex = -1;
		StringBuilder stringBuilder = new();
		QuestModPlayer playerQuests = Main.LocalPlayer.GetModPlayer<QuestModPlayer>();

		foreach (Quest quest in playerQuests.QuestsByName.Values.OrderBy(q => q.Name))
		{
			if (!string.IsNullOrEmpty(SearchBar.CurrentValue) && !quest.Name.Contains(SearchBar.CurrentValue, StringComparison.InvariantCultureIgnoreCase))
			{
				continue;
			}

			questIndex++;

			UIPanel questPanel = QuestList.AddElement(new UIPanel(), e =>
			{
				e.SetDimensions(x: (0.0f, +0), y: (0.1f, +128 * questIndex), width: (1.0f, +0), height: (0f, +132));
			});

			// Name
			questPanel.AddElement(new UIText($"[c/{Color.YellowGreen.ToHexRGB()}:{quest.Name}] ({quest.DisplayName})"), e =>
			{
				e.SetDimensions(x: (0.0f, +0), y: (0.0f, +0), width: (0.0f, +0), height: (0f, +128));
			});

			// Buttons

			float buttonX = 0f;

			// Backtrack '-' button.
			questPanel.AddElement(new UIButton<string>(""), e =>
			{
				e.AltHoverText = "Click to go back by a step.";
				e.AltPanelColor = e.AltHoverPanelColor = Color.IndianRed;
				e.UseAltColors = () => quest.Active && quest.CurrentStep > 0;
				// Make invisible when inactive:
				(e.AltBorderColor, e.AltHoverBorderColor) = (e.BorderColor, e.HoverBorderColor);
				e.HoverPanelColor = e.HoverBorderColor = e.BorderColor = e.BackgroundColor;

				e.SetDimensions(x: (0.0f, +buttonX), y: (0.0f, +24), width: (0.0f, +32), height: (0f, +32));
				e.AddComponent(new UIDynamicText(x => quest.Active && quest.CurrentStep > 0 ? "-" : ""));

				e.OnLeftClick += (evt, self) =>
				{
					if (!quest.Active && !quest.Completed)
					{
						return;
					}

					if (!quest.Completed && quest.CurrentStep <= 0)
					{
						return;
					}

					quest.Advance(Main.LocalPlayer, delta: -1);
					SmartUiLoader.GetUiState<QuestsUIState>().Refresh();
				};

				buttonX += e.Width.Pixels;
			});
			// State info panel. Button, but doesn't do anything.
			questPanel.AddElement(new UIButton<string>(""), e =>
			{
				e.BackgroundColor = e.HoverPanelColor = Color.IndianRed;
				e.AltPanelColor = e.AltHoverPanelColor = Color.Orange;
				e.AltHoverBorderColor = e.HoverBorderColor = e.BorderColor;
				e.UseAltColors = () => quest.Active || quest.Completed;

				e.SetDimensions(x: (0.0f, +buttonX), y: (0.0f, +24), width: (0.0f, +128), height: (0f, +32));
				e.AddComponent(new UIDynamicText(x => quest.Completed ? "Completed" : (quest.Active ? $"Stage {quest.CurrentStep + 1} of {quest.QuestSteps.Count}" : "Inactive")));

				buttonX += e.Width.Pixels;
			});
			// Advance '+' button.
			questPanel.AddElement(new UIButton<string>(""), e =>
			{
				e.AltHoverText = "Click to advance.";
				e.AltPanelColor = e.AltHoverPanelColor = Color.YellowGreen;
				e.UseAltColors = () => !quest.Completed;
				// Make invisible when inactive:
				(e.AltBorderColor, e.AltHoverBorderColor) = (e.BorderColor, e.HoverBorderColor);
				e.HoverPanelColor = e.HoverBorderColor = e.BorderColor = e.BackgroundColor;

				e.SetDimensions(x: (0.0f, +buttonX), y: (0.0f, +24), width: (0.0f, +32), height: (0f, +32));
				e.AddComponent(new UIDynamicText(x => !quest.Completed ? "+" : ""));

				e.OnLeftClick += (evt, self) =>
				{
					if (quest.Completed)
					{
						return;
					}

					if (!quest.Active)
					{
						Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest(quest.FullName);
					}
					else
					{
						quest.Advance(Main.LocalPlayer, delta: 1);
					}

					SmartUiLoader.GetUiState<QuestsUIState>().Refresh();
				};

				buttonX += e.Width.Pixels + 8f;
			});

			// 'Give Rewards' button.
			questPanel.AddElement(new UIButton<string>("Give Rewards"), e =>
			{
				e.SetDimensions(x: (0.0f, +buttonX), y: (0.0f, +24), width: (0.0f, +128), height: (0f, +32));
				e.OnLeftClick += (evt, self) => quest.GiveRewards(Main.LocalPlayer);

				buttonX += e.Width.Pixels + 8f;
			});

			// 'Force Reset' button.
			questPanel.AddElement(new UIButton<string>("Reset"), e =>
			{
				e.SetDimensions(x: (0.0f, +buttonX), y: (0.0f, +24), width: (0.0f, +80), height: (0f, +32));
				e.OnLeftClick += (evt, self) =>
				{
					quest.Reset();
					SmartUiLoader.GetUiState<QuestsUIState>().Refresh();
				};

				buttonX += e.Width.Pixels + 8f;
			});

			// Lower text
			questPanel.AddElement(new UIText($"[{quest.Name}]"), e =>
			{
				e.SetDimensions(x: (0.0f, +0), y: (0.0f, +64), width: (0.0f, +0), height: (0f, +128));
				e.AddComponent(new UIDynamicText(x =>
				{
					stringBuilder.Clear();

					string? giverName = quest.NPCQuestGiver >= 0 ? NPCID.Search.GetName(quest.NPCQuestGiver).Split('/').Last() : null;

					stringBuilder.AppendLine($"Step: [c/{Color.Gold.ToHexRGB()}:{(quest.Active ? quest.ActiveStep : null)?.GetType().Name ?? "N/A"}]");
					stringBuilder.AppendLine($"Given By: [c/{Color.Gold.ToHexRGB()}:{(quest.NPCQuestGiver >= 0 ? giverName : "N/A")}]");

					return stringBuilder.ToString();
				}));
			});
		}

		if (questIndex < 0)
		{
			QuestList.AddElement(new UITextPanel<string>("No quests found"), e =>
			{
				e.SetDimensions(x: (0.0f, +0), y: (0.1f, +128 * questIndex), width: (1.0f, +0), height: (0f, +40));
			});
		}

		Recalculate();
	}
}
