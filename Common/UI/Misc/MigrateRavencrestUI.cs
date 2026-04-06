using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Core.UI;
using SubworldLibrary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Misc;

internal class MigrateRavencrestUI : UIState
{
	/// <summary>
	/// Used solely to clear Ravencrest if it's outdated. Refer to <see cref="Subworlds.RavencrestContent.RavencrestSystem.RavencrestVersion"/> for the current version.<br/>
	/// If you are implementing an overhaul to Ravencrest, increment that value and this whole thing should work automatically.
	/// </summary>
	internal class MigrateRavencrestSystem : ModSystem
	{
		internal static bool ResetRavencrest = false;

		public override void PreUpdateEntities()
		{
			if (ResetRavencrest && SubworldSystem.Current is null)
			{
				string id = Main.ActiveWorldFileData.UniqueId.ToString();
				string path = Path.Combine(Main.WorldPath, id);
				string[] extensions = "bak bak2 twld wld".Split(' ');
				string[] files = Directory.GetFiles(path);
				List<string> removals = [];

				foreach (string file in files)
				{
					if (file.StartsWith(path) && extensions.Any(file.EndsWith))
					{
						removals.Add(file);
					}
				}

				foreach (string file in removals)
				{
					File.Delete(file);
				}

				ResetRavencrest = false;
				SubworldSystem.Enter<RavencrestSubworld>();
			}
		}
	}

	public const string Identifier = "Migrate Ravencrest";

	public override void OnInitialize()
	{
		UIPanel panel = new()
		{
			VAlign = 0.25f,
			HAlign = 0.5f,
			Width = StyleDimension.FromPixels(600),
			Height = StyleDimension.FromPixels(84),
		};
		Append(panel);

		UIText migrateText = new(Language.GetText("Mods.PathOfTerraria.UI.MigrateRavencrest.Title"))
		{
			VAlign = -0.9f,
		};
		panel.Append(migrateText);

		panel.Append(new UIText(Language.GetText("Mods.PathOfTerraria.UI.MigrateRavencrest.Question"), 0.9f));
		panel.Append(new UIText(Language.GetText("Mods.PathOfTerraria.UI.MigrateRavencrest.Note"), 0.9f) 
		{ 
			VAlign = 1, 
			HAlign = 1, 
			Top = StyleDimension.FromPixels(-16), 
			TextColor = new Color(200, 200, 200)
		});

		UIButton<string> yesButton = new(Language.GetText("Mods.PathOfTerraria.UI.MigrateRavencrest.Yes").Value)
		{
			Width = StyleDimension.FromPixels(80),
			Height = StyleDimension.FromPixels(36),
			VAlign = 1,
			TextColor = Color.Green
		};

		yesButton.OnLeftClick += DeleteRavencrest;
		panel.Append(yesButton);

		UIButton<string> noButton = new(Language.GetText("Mods.PathOfTerraria.UI.MigrateRavencrest.No").Value)
		{
			Width = StyleDimension.FromPixels(80),
			Height = StyleDimension.FromPixels(36),
			Left = StyleDimension.FromPixels(84),
			VAlign = 1,
			TextColor = Color.Red
		};

		noButton.OnLeftClick += (_, _) => UIManager.TryDisable(Identifier);
		panel.Append(noButton);
	}

	private void DeleteRavencrest(UIMouseEvent evt, UIElement listeningElement)
	{
		MigrateRavencrestSystem.ResetRavencrest = true;
		SubworldSystem.Exit();
	}

	public override void OnDeactivate()
	{
		RemoveAllChildren();
	}
}
