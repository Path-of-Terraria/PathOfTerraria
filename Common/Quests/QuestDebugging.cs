using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.UI.Components;
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

internal sealed class QuestDebugging : ModSystem
{
	public override void PreUpdateEntities()
	{
#if DEBUG
		if (Main.keyState.IsKeyDown(Keys.NumPad1) && !Main.oldKeyState.IsKeyDown(Keys.NumPad1))
		{
			SmartUiLoader.GetUiState<QuestDebugState>().Toggle();
		}
#endif
	}
}

internal sealed class QuestDebugCommand : ModCommand
{
	public override string Command => "potQuestDebug";
	public override CommandType Type => CommandType.Chat;

	public override void Action(CommandCaller caller, string input, string[] args)
	{
#if !DEBUG
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
	public UIPanel Panel { get; private set; } = null!;
	public UIList QuestList { get; private set; } = null!;
	public UIScrollbar QuestScrollbar { get; private set; } = null!;

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
		});
#if DEBUG
		Panel.AddElement(new UIButton<string>("Refresh"), e =>
		{
			e.SetDimensions(x: (1.0f, -128 - 8), y: (0.00f, +0), width: (0.0f, +96), height: (0f, +32));
			e.OnLeftClick += (evt, self) => Rebuild();
		});
#endif
		Panel.AddElement(new UIButton<string>("X"), e =>
		{
			e.SetDimensions(x: (1.0f, -32), y: (0.00f, +0), width: (0.0f, +32), height: (0f, +32));
			e.OnLeftClick += (evt, self) => SetEnabled(false);
		});

		// List

		QuestList = Panel.AddElement(new UIList(), e =>
		{
			e.SetDimensions(x: (0.00f, +StartX), y: (0.00f, +StartY), width: (1.00f, -48), height: (1.00f, -64));
		});
		QuestScrollbar = Panel.AddElement(new UIScrollbar(), e =>
		{
			e.SetDimensions(x: (1.00f, -26), y: (0.00f, +StartY + 8), width: (0.00f, +20), height: (1.00f, -80));
		});
		QuestList.SetScrollbar(QuestScrollbar);

		int questIndex = -1;
		StringBuilder stringBuilder = new();
		QuestModPlayer playerQuests = Main.LocalPlayer.GetModPlayer<QuestModPlayer>();

		foreach (Quest quest in playerQuests.QuestsByName.Values.OrderBy(q => q.Name))
		{
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

			// Activation and advancement button.
			questPanel.AddElement(new UIButton<string>(""), e =>
			{
				e.BackgroundColor = e.HoverPanelColor = Color.IndianRed;
				e.AltPanelColor = e.AltHoverPanelColor = Color.YellowGreen;
				e.UseAltColors = () => quest.Active || quest.Completed;
				e.HoverText = e.AltHoverText = "Left Click to advance, Right click to backtrack.";

				e.SetDimensions(x: (0.0f, +buttonX), y: (0.0f, +24), width: (0.0f, +128), height: (0f, +32));
				e.AddComponent(new UIDynamicText(() => quest.Completed ? "Completed" : (quest.Active ? $"Stage {quest.CurrentStep + 1} of {quest.QuestSteps.Count}" : "Inactive")));

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
				};
				e.OnRightClick += (evt, self) =>
				{
					if (!quest.Active && !quest.Completed)
					{
						return;
					}

					if (quest.Completed || quest.CurrentStep > 0)
					{
						quest.Advance(Main.LocalPlayer, delta: -1);
					}
					else
					{
						quest.Reset();
					}
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
			questPanel.AddElement(new UIButton<string>("Force Reset"), e =>
			{
				e.SetDimensions(x: (0.0f, +buttonX), y: (0.0f, +24), width: (0.0f, +128), height: (0f, +32));
				e.OnLeftClick += (evt, self) => quest.Reset();

				buttonX += e.Width.Pixels + 8f;
			});

			// Lower text
			questPanel.AddElement(new UIText($"[{quest.Name}]"), e =>
			{
				e.SetDimensions(x: (0.0f, +0), y: (0.0f, +64), width: (0.0f, +0), height: (0f, +128));
				e.AddComponent(new UIDynamicText(() =>
				{
					stringBuilder.Clear();

					string? giverName = quest.NPCQuestGiver >= 0 ? NPCID.Search.GetName(quest.NPCQuestGiver).Split('/').Last() : null;

					stringBuilder.AppendLine($"Step: [c/{Color.Gold.ToHexRGB()}:{(quest.Active ? quest.ActiveStep : null)?.GetType().Name ?? "N/A"}]");
					stringBuilder.AppendLine($"Given By: [c/{Color.Gold.ToHexRGB()}:{(quest.NPCQuestGiver >= 0 ? giverName : "N/A")}]");

					return stringBuilder.ToString();
				}));
			});
		}
	}
}
