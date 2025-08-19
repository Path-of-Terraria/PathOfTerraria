using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.UI.Armor;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Elements;
public class UICustomHoverImageItemSlot : UIHoverImageItemSlot
{
    private readonly int virtualSlot;
    private ExtraAccessoryModPlayer ModPlayer => Main.LocalPlayer.GetModPlayer<ExtraAccessoryModPlayer>();
    private bool IsVanitySlot => Context == ItemSlot.Context.EquipAccessoryVanity;
    private bool IsDyeSlot => Context == ItemSlot.Context.EquipDye;
    private static Item[] dummyArray = [new Item()];

    public UICustomHoverImageItemSlot(
        Asset<Texture2D> backgroundTexture,
        Asset<Texture2D> iconTexture,
        int virtualSlot,
        string key,
        int context = ItemSlot.Context.EquipAccessory
    ) : base(backgroundTexture, iconTexture, ref dummyArray, 0, key, context)
    {
        this.virtualSlot = virtualSlot;

        if (IsVanitySlot)
        {
            Predicate = (item, _) => item.accessory && item.wingSlot <= 0;
        }
        else if (IsDyeSlot)
        {
            Predicate = (item, _) => item.dye > 0;
        }
        else
        {
            Predicate = (item, _) => ModContent.GetInstance<AccessorySlotGlobalItem>().IsNormalAccessory(item);
        }
    }

    private new Item Item
    {
        get
        {
            if (IsDyeSlot)
                return ModPlayer.GetCustomDyeSlot(virtualSlot);
            else if (IsVanitySlot)
                return ModPlayer.GetCustomVanitySlot(virtualSlot);
            else
                return ModPlayer.GetCustomSlot(virtualSlot);
        }
        set
        {
            if (IsDyeSlot)
            {
                ModPlayer.SetCustomDyeSlot(virtualSlot, value);
            }
            else if (IsVanitySlot)
            {
                ModPlayer.SetCustomVanitySlot(virtualSlot, value);
            }
            else
            {
                ModPlayer.SetCustomSlot(virtualSlot, value);
            }
        }
    }

    protected override void UpdateInteraction()
    {
        if (!IsMouseHovering || PlayerInput.IgnoreMouseInterface)
        {
            return;
        }

        HandleTooltip();

        if (Main.mouseLeft && Main.mouseLeftRelease)
        {
            HandleLeftClick();
        }

        Main.LocalPlayer.mouseInterface = true;
    }
    
    protected override void UpdateIcon()
    {
        if (!Item.IsAir)
        {
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Rectangle frame = Main.itemAnimations[Item.type]?.GetFrame(texture) ?? texture.Frame();
            ItemSlot.DrawItem_GetColorAndScale(Item, Item.scale, ref Icon.Color, 24f, ref frame, out _, out float finalDrawScale);
            Icon.ImageScale = MathHelper.SmoothStep(Icon.ImageScale, finalDrawScale * (IsMouseHovering ? ActiveScale : InactiveScale), Smoothness);
        }
        else
        {
            Icon.Rotation = MathHelper.SmoothStep(Icon.Rotation, IsMouseHovering ? ActiveRotation : InactiveRotation, Smoothness);
            Icon.ImageScale = MathHelper.SmoothStep(Icon.ImageScale, IsMouseHovering ? ActiveScale : InactiveScale, Smoothness);
        }

        Icon.SetImage(GetIconToDraw());
    }

    private void HandleTooltip()
    {
        if (Key == null)
        {
            return;
        }
        Main.hoverItemName = Item.IsAir ? Language.GetTextValue(Key) : Item.HoverName;
        Main.HoverItem = Item.IsAir ? new Item() : Item.Clone();
        if (!Item.IsAir)
        {
            Main.HoverItem.tooltipContext = Context;
        }
    }

    private void HandleLeftClick()
    {
        if (!Main.mouseItem.IsAir && Predicate?.Invoke(Main.mouseItem, Item) == false)
        {
            Main.LocalPlayer.mouseInterface = true;
            return;
        }

        Item tempItem = Item;
        ItemSlot.Handle(ref tempItem, Context);
        Item = tempItem;
    }

    protected override Asset<Texture2D> GetIconToDraw()
    {
        if (Item.IsAir)
        {
            return IconTexture;
        }
        Main.instance.LoadItem(Item.type);
        return TextureAssets.Item[Item.type];
    }
}