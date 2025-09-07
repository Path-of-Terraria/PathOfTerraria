using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.UI.Guide;
using PathOfTerraria.Content.Passives;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.PassiveTree;

internal class PassiveElement : AllocatableElement
{
	public Passive Passive { get; }

	public PassiveElement(Passive passive) : base(passive)
	{
		float halfSizeX = passive.Size.X / 2;
		float halfSizeY = passive.Size.Y / 2;

		Passive = passive;
		Left.Set(passive.TreePos.X - halfSizeX, 0.5f);
		Top.Set(passive.TreePos.Y - halfSizeY, 0.5f);
		Width.Set(passive.Size.X, 0);
		Height.Set(passive.Size.Y, 0);

		// Anchor passive should always be "unlocked", thus this hardcoding
		if (Passive is AnchorPassive)
		{
			Passive.Level = 1;
		}
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		if (Passive.CanAllocate(Main.LocalPlayer) && CheckMouseContained())
		{
			Allocate(Passive, usedCost: 1);
		}
	}

	protected override void Allocate(Allocatable passive, int usedCost)
	{
		base.Allocate(passive, usedCost);

		Player p = Main.LocalPlayer;

		p.GetModPlayer<PassiveTreePlayer>().Points -= usedCost;
		p.GetModPlayer<PassiveTreePlayer>().SaveData([]); // Instantly save the result because _saveData is needed whenever the element reloads
		p.GetModPlayer<TutorialPlayer>().TutorialChecks.Add(TutorialCheck.AllocatedPassive);
	}

	public override void SafeRightClick(UIMouseEvent evt)
	{
		if (Passive.CanDeallocate(Main.LocalPlayer) && CheckMouseContained())
		{
			Deallocate(Passive, usedCost: 1);
		}
	}

	protected override void Deallocate(Allocatable passive, int usedCost)
	{
		base.Deallocate(passive, usedCost);

		Player p = Main.LocalPlayer;

		p.GetModPlayer<PassiveTreePlayer>().Points += usedCost;
		p.GetModPlayer<PassiveTreePlayer>().SaveData([]); // Instantly save the result because _saveData is needed whenever the element reloads
		p.GetModPlayer<TutorialPlayer>().TutorialChecks.Add(TutorialCheck.DeallocatedPassive);
		
		SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath);
	}
}
