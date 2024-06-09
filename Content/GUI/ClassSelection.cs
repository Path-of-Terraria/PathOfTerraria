using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.ModPlayers;
using System.Collections.Generic;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Content.Items.Gear.Weapons.Staff;
using PathOfTerraria.Content.Items.Gear.Weapons.Sword;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

public class ClassSelection : SmartUIState
{
	public bool IsVisible = true;
	public override bool Visible => IsVisible;
	private bool _populated;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}
	
	private UIPanel _panel;
    private UITextPanel<string> _meleeButton;
    private UITextPanel<string> _rangedButton;
    private UITextPanel<string> _magicButton;
    private UITextPanel<string> _summonerButton;
    
    public override void Draw(SpriteBatch spriteBatch)
    {
	    if (Main.LocalPlayer != null)
	    {
		    ClassModPlayer mp = Main.LocalPlayer.GetModPlayer<ClassModPlayer>();
		    IsVisible = !mp.HasSelectedClass();
		    if (!IsVisible)
		    {
			    return;
		    }
	    }

	    if (!_populated)
	    {
		    _panel = new UIPanel();
		    _panel.SetPadding(10);
		    _panel.Width.Set(340f, 0f);
		    _panel.Height.Set(340f, 0f);
		    _panel.VAlign = 0.5f;
		    _panel.HAlign = 0.5f;
        
		    _meleeButton = new UITextPanel<string>("Melee", 0.8f, true);
		    _meleeButton.Width.Set(0, 1f);
		    _meleeButton.Height.Set(40f, 0f);
		    _meleeButton.Top.Set(80f, 0f);
		    _meleeButton.OnLeftClick += SelectMeleeClass;

		    _rangedButton = new UITextPanel<string>("Ranged", 0.8f, true);
		    _rangedButton.Width.Set(0, 1f);
		    _rangedButton.Height.Set(40f, 0f);
		    _rangedButton.Top.Set(140f, 0f);
		    _rangedButton.OnLeftClick += SelectRangedClass;

		    _magicButton = new UITextPanel<string>("Magic", 0.8f, true);
		    _magicButton.Width.Set(0, 1f);
		    _magicButton.Height.Set(40f, 0f);
		    _magicButton.Top.Set(200f, 0f);
		    _magicButton.OnLeftClick += SelectMagicClass;

		    _summonerButton = new UITextPanel<string>("Summoner", 0.8f, true);
		    _summonerButton.Width.Set(0, 1f);
		    _summonerButton.Height.Set(40f, 0f);
		    _summonerButton.Top.Set(260f, 0f);
		    _summonerButton.OnLeftClick += SelectSummonerClass;

		    _panel.Append(_meleeButton);
		    _panel.Append(_rangedButton);
		    _panel.Append(_magicButton);
		    _panel.Append(_summonerButton);

		    Append(_panel);
		    _populated = true;
	    }
	    
	    Recalculate();

	    base.Draw(spriteBatch);
	    Utils.DrawBorderStringBig(spriteBatch, "Select a Class", _panel.GetDimensions().ToRectangle().TopLeft() + new Vector2(170, 32), Color.White, 0.6f, 0.5f, 0.35f);
    }
    
    private void SelectMeleeClass(UIMouseEvent evt, UIElement listeningElement)
    {
        SetPlayerClass(PlayerClass.Melee);
    }

    private void SelectRangedClass(UIMouseEvent evt, UIElement listeningElement)
    {
        SetPlayerClass(PlayerClass.Ranged);
    }

    private void SelectMagicClass(UIMouseEvent evt, UIElement listeningElement)
    {
        SetPlayerClass(PlayerClass.Magic);
    }

    private void SelectSummonerClass(UIMouseEvent evt, UIElement listeningElement)
    {
        SetPlayerClass(PlayerClass.Summoner);
    }

    private void SetPlayerClass(PlayerClass playerClass)
    {
	    switch (playerClass)
	    {
		    case PlayerClass.Melee:
			    Gear.SpawnGear<WoodenSword>(Main.LocalPlayer.position, 1);
			    Main.NewText($"Selected Melee class");
			    break;
		    case PlayerClass.Magic:
			    Gear.SpawnGear<Staff>(Main.LocalPlayer.position, 1);
			    Main.NewText($"Selected Magic class");
			    break;
		    case PlayerClass.Ranged:
			    Main.NewText($"Selected Ranged class");
			    break;
		    case PlayerClass.Summoner:
			    Main.NewText($"Selected Summoner class");
			    break;
	    }
	    
	    Main.LocalPlayer.GetModPlayer<ClassModPlayer>().SelectedClass = playerClass;
	    IsVisible = false;
    }
}