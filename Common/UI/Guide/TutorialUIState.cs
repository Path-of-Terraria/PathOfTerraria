using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.UI.PlayerStats;
using PathOfTerraria.Content.NPCs.Town;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using ReLogic.Graphics;
using SubworldLibrary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.Social.Steam;
using PathOfTerraria.Common.UI.Quests;
using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Common.UI.Guide;

internal class TutorialUIState : UIState
{
	public static Asset<Texture2D> SmallBack;
	public static Asset<Texture2D> BigBack;

	public bool Visible => _opacity > 0;

	internal static int StoredStep = 0;

	public int Step { get; private set; }

	private float _opacity = 0;
	private float _displayTextLength = 0;
	private float _baseYDivisor = 4; 
	
	public TutorialUIState()
	{
		BigBack ??= ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Guide/LargeBack");
		SmallBack ??= ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Guide/SkipBack");

		Step = StoredStep;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		_opacity = MathHelper.Lerp(_opacity, Step > 13 ? 0 : 1, 0.05f);
		_baseYDivisor = MathHelper.Lerp(_baseYDivisor, Step is 9 or 10 ? 8 : 4, 0.05f);

		Vector2 pos = new Vector2(Main.screenWidth, Main.screenHeight) / new Vector2(2, _baseYDivisor);

		string text = Language.GetText($"Mods.{PoTMod.ModName}.UI.Guide." + Math.Min(Step, 13)).Value;
		DrawBacked(spriteBatch, pos, text, false);

		bool canGoNext = CanGotoNextStep();
		DrawBacked(spriteBatch, pos + new Vector2(-110, 110), "Next", true, !canGoNext ? null : new Action(IncrementStep), !canGoNext);
		DrawBacked(spriteBatch, pos + new Vector2(0, 110), "Skip Step", true, new Action(IncrementStep));
		DrawBacked(spriteBatch, pos + new Vector2(110, 110), "Skip Guide", true, () => 
		{
			Step = 13;
			Main.LocalPlayer.GetModPlayer<ExpModPlayer>().Exp += Main.LocalPlayer.GetModPlayer<ExpModPlayer>().NextLevel + 1;

			IncrementStep();
		});
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		HashSet<TutorialCheck> checks = Main.LocalPlayer.GetModPlayer<TutorialPlayer>().TutorialChecks;

		if (Step == 1 && SmartUiLoader.GetUiState<TreeState>().Visible)
		{
			IncrementStep();
		}
		else if (Step == 2 && checks.Contains(TutorialCheck.AllocatedPassive) && checks.Contains(TutorialCheck.DeallocatedPassive))
		{
			IncrementStep();
		}
		else if (Step == 3 && SmartUiLoader.GetUiState<TreeState>().Visible && SmartUiLoader.GetUiState<TreeState>().Panel.ActiveTab == "SkillTree")
		{
			IncrementStep();
		}
		else if (Step == 4 && checks.Contains(TutorialCheck.SelectedSkill))
		{
			IncrementStep();
		}
		else if (Step == 5 && !SmartUiLoader.GetUiState<TreeState>().Visible)
		{
			IncrementStep();
		}
		else if (Step == 6 && checks.Contains(TutorialCheck.UsedASkill))
		{
			IncrementStep();
		}
		else if (Step == 8 && checks.Contains(TutorialCheck.SwappedWeapon))
		{ 
			IncrementStep();
		}
		else if (Step == 9 && !SmartUiLoader.GetUiState<PlayerStatUIState>().Visible && checks.Contains(TutorialCheck.OpenedCharSheet))
		{
			IncrementStep();
		}
		else if (Step == 10 && !SmartUiLoader.GetUiState<QuestsUIState>().Visible && checks.Contains(TutorialCheck.OpenedQuestBook))
		{
			IncrementStep();
		}
		else if (Step == 11 && SubworldSystem.Current is RavencrestSubworld)
		{
			IncrementStep();
		}
		else if (Step == 12 && SubworldSystem.Current is null)
		{
			IncrementStep();
		}
	}

	private void IncrementStep()
	{
		Step++;

		if (Step <= 13)
		{
			_displayTextLength = 0;
		}

		Player plr = Main.LocalPlayer;
		plr.GetModPlayer<TutorialPlayer>().TutorialStep = (byte)Step;
		StoredStep = Step;

		if (Step == 1)
		{
			plr.GetModPlayer<ExpModPlayer>().Exp += plr.GetModPlayer<ExpModPlayer>().NextLevel + 1;
		}
		else if (Step == 10)
		{
			Main.LocalPlayer.GetModPlayer<QuestModPlayer>().StartQuest<FirstQuest>();
		}
		else if (Step == 11)
		{
			if (!NPC.AnyNPCs(ModContent.NPCType<RavenNPC>()))
			{
				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					NPC.NewNPC(Entity.GetSource_NaturalSpawn(), (int)plr.Center.X, (int)plr.Center.Y - 200, ModContent.NPCType<RavenNPC>());
				}
				else if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					SpawnNPCOnServerHandler.Send((short)ModContent.NPCType<RavenNPC>(), plr.Center - new Vector2(0, 200));
				}
			}
		}
		else if (Step == 13)
		{
			Main.LocalPlayer.GetModPlayer<TutorialPlayer>().TutorialChecks.Add(TutorialCheck.FinishedTutorial);
		}
	}

	private bool CanGotoNextStep()
	{
		return Step switch
		{
			0 or 7 or 13 => true,
			_ => false
		};
	}

	private void DrawBacked(SpriteBatch spriteBatch, Vector2 pos, string text, bool isPrimaryPanel, Action onClick = null, bool grayOut = false)
	{
		bool hasClick = onClick is not null;
		Texture2D tex = (isPrimaryPanel ? SmallBack : BigBack).Value;
		bool hover = false;
		Rectangle bounds = new((int)pos.X - tex.Width / 2, (int)pos.Y - tex.Height / 2, tex.Width, tex.Height);

		if (bounds.Contains(Main.MouseScreen.ToPoint()))
		{
			hover = hasClick;
			Main.LocalPlayer.mouseInterface = true;
		}

		if (!isPrimaryPanel)
		{
			if (Step <= 14)
			{
				_displayTextLength = Math.Min(_displayTextLength + 0.9f, text.Length);
				text = text[.. (int)_displayTextLength];
			}
			else
			{
				_displayTextLength = Math.Max(_displayTextLength - 0.9f, 0);
				text = text[..(int)_displayTextLength];
			}
		}

		Color drawColor = (hover || grayOut ? Color.Gray : Color.White) * _opacity;
		spriteBatch.Draw(tex, pos, null, drawColor, 0f, tex.Size() / 2f, 1, SpriteEffects.None, 0);

		DynamicSpriteFont font = FontAssets.ItemStack.Value;

		if (!text.Contains('\n'))
		{
			Vector2 size = ChatManager.GetStringSize(font, text, Vector2.One);
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, pos, drawColor, 0f, size / 2f, Vector2.One);
		}
		else
		{
			string[] snippets = text.Split('\n');
			Vector2 offsetPos = pos - new Vector2(0, 40);

			for (int i = 0; i < snippets.Length; ++i)
			{
				string str = snippets[i];
				Vector2 size = ChatManager.GetStringSize(font, str, Vector2.One);
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, str, offsetPos, drawColor, 0f, size / 2f, Vector2.One);

				offsetPos.Y += 24;
			}
		}

		if (hover && Main.mouseLeft && Main.mouseLeftRelease)
		{
			onClick();
		}
	}
}
