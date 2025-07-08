using Microsoft.Xna.Framework.Input;
using MonoMod.RuntimeDetour;
using System.Reflection;
using Terraria.Chat;

namespace PathOfTerraria.Core.Commands;

internal class CacheLastCommandSystem : ModPlayer
{
	private static string LastMessage = null;
	private static Hook CommandLoaderHook = null;

	public override void Load()
	{
		CommandLoaderHook = new Hook(typeof(CommandLoader).GetMethod("HandleCommand", BindingFlags.Static | BindingFlags.NonPublic), CacheCommand);
	}

	public override void Unload()
	{
		CommandLoaderHook.Undo();
		CommandLoaderHook = null;
	}

	public static bool CacheCommand(Func<string, CommandCaller, bool> orig, string input, CommandCaller caller)
	{
		LastMessage = input;
		return orig(input, caller);
	}

	public override void PreUpdate()
	{
		if (Main.myPlayer != Player.whoAmI)
		{
			return;
		}

		if (Main.drawingPlayerChat && Keyboard.GetState().IsKeyDown(Keys.Up) && LastMessage is not null)
		{
			Main.chatText = LastMessage;
		}
	}
}
