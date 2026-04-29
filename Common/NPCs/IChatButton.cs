using MonoMod.Cil;
using ReLogic.Graphics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.NPCs;

#nullable enable

public interface IChatButton
{
	public class ChatButton(string localization, Action<NPC> onClick, Action<NPC>? onHover = null, Func<bool>? displays = null)
	{
		public LocalizedText LocalizedText => Language.GetText(Localization);

		public readonly string Localization = localization;
		public readonly Action<NPC> OnClick = onClick;
		public readonly Action<NPC>? OnHover = onHover;
		public readonly Func<bool> CanDisplay = displays ?? (static () => true);

		public void Draw(Vector2 position, NPC self, out float width)
		{
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			Vector2 size = ChatManager.GetStringSize(font, LocalizedText.Value, Vector2.One);
			position.Y -= size.Y / 2f;
			Rectangle drawLocation = new((int)position.X - (int)(size.X / 2f), (int)position.Y - (int)(size.Y / 2f), (int)size.X, (int)size.Y);
			bool hovering = drawLocation.Contains(Main.MouseScreen.ToPoint());
			float mouseColorBase = (float)(int)Main.mouseTextColor / 255f;
			Color drawColor = new((byte)(224f * mouseColorBase), (byte)(201f * mouseColorBase), (byte)(92f * mouseColorBase), Main.mouseTextColor);
			Color backColor = Color.Black;
			float scale = 1f;

			width = size.X;

			if (hovering)
			{
				Player player = Main.LocalPlayer;
				player.mouseInterface = true;
				player.releaseUseItem = false;

				backColor = Color.Brown;
				scale = 1.2f;
				OnHover?.Invoke(self);

				if (Main.mouseLeft && !LastHadMouse)
				{
					OnClick(self);
				}
			}

			ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, LocalizedText.Value, position, drawColor, backColor, 0f, size / 2f, new Vector2(scale));
		}
	}

	public class ChatButtonHooks : ILoadable
	{
		public void Load(Mod mod)
		{
			On_Main.GUIChatDrawInner += DrawInnerChat;
		}

		private void DrawInnerChat(On_Main.orig_GUIChatDrawInner orig, Main self)
		{
			orig(self);

			if (Main.LocalPlayer.talkNPC < 0 && Main.LocalPlayer.sign == -1)
			{
				Main.npcChatText = "";
				return;
			}

			if (Main.LocalPlayer.TalkNPC.ModNPC is IChatButton button)
			{
				ChatButton[] buttons = button.ReportButtons();
				int numLines = (GetNumLines() + 1) * 30;
				Vector2 basePosition = new(Main.screenWidth / 2f + TextureAssets.ChatBack.Width() / 2 - 70, 130 + numLines);
				Vector2 drawPosition = basePosition;
				
				for (int i = 0; i < buttons.Length; ++i)
				{
					if (!buttons[i].CanDisplay())
					{
						continue;
					}

					buttons[i].Draw(basePosition, Main.LocalPlayer.TalkNPC, out float width);
					drawPosition.X -= width + 6;
				}
			}

			LastHadMouse = Main.mouseLeft;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static int GetNumLines()
		{
			FieldInfo? textDisplay = typeof(Main).GetField("_textDisplayCache", BindingFlags.NonPublic | BindingFlags.Instance);
			MethodInfo? lineAmount = typeof(Main).GetNestedType("TextDisplayCache", BindingFlags.NonPublic)?.GetProperty("AmountOfLines")?.GetGetMethod();

			if (lineAmount is null || textDisplay is null)
			{
				return 0;
			}

			return (int)(lineAmount.Invoke(textDisplay.GetValue(Main.instance), []) ?? 0);
		}

		public void Unload()
		{
		}
	}

	private static bool LastHadMouse = false;

	public ChatButton[] ReportButtons();
}
