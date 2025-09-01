using System.Collections.Generic;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI;

public abstract class CloseableSmartUi : SmartUiState
{
    public UICloseablePanel Panel;
    public UIImageButton CloseButton;
    public bool IsVisible;

    protected virtual int PointsAndExitPadding => 10;
    protected virtual int TopPadding => -400;
    protected virtual int PanelHeight => 800;
    protected virtual int LeftPadding => -450;
    protected virtual int PanelWidth => 900;
    protected virtual bool IsCentered => false;
    public override bool Visible => IsVisible;

    public override int InsertionIndex(List<GameInterfaceLayer> layers)
    {
        return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

#if DEBUG
        GUIDebuggingTools.DrawGuiBorder(spriteBatch, this, Color.Purple);
		GUIDebuggingTools.DrawGuiBorder(spriteBatch, Panel, Color.Yellow);
#endif
    }

    protected virtual void CreateMainPanel(bool showCloseButton = true, Point? panelSize = null, bool canResize = true, bool invisible = false)
    {
        panelSize ??= new Point(PanelWidth, PanelHeight);

        Panel = new UICloseablePanel(false, false, canResize, invisible);

        if (IsCentered)
        {
			Panel.VAlign = Panel.HAlign = 0.5f;
        }
        else
        {
            Panel.Left.Set(LeftPadding, 0);
            Panel.Top.Set(TopPadding, 0);
        }

        Panel.Width.Set(panelSize.Value.X, 0);
        Panel.Height.Set(panelSize.Value.Y, 0);
		Panel.Visible = !invisible;
        Append(Panel);

        if (showCloseButton)
        {
            AddCloseButton();
        }
    }
    
    protected virtual void DefaultClose()
    {
	    IsVisible = false;
	    SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
    }

    protected void AddCloseButton()
    {
        CloseButton = new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton"));
        CloseButton.Left.Set(-38 - PointsAndExitPadding, 1f);
        CloseButton.Top.Set(10, 0f);
        CloseButton.Width.Set(38, 0);
        CloseButton.Height.Set(38, 0);
        CloseButton.OnLeftClick += (a, b) => DefaultClose();
        CloseButton.SetVisibility(1, 1);
        Panel.Append(CloseButton);
    }
}
