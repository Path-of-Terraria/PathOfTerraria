using PathOfTerraria.Common.Systems.Runebound;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Common.Encounters;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.NPCs.Runebound;

internal sealed class BindingMonolith : ModNPC
{
	public int EncounterId => (int)NPC.ai[2];
	public RuneboundFamily Family => (RuneboundFamily)(int)NPC.ai[0];
	public RunestoneGrade Grade => (RunestoneGrade)(int)NPC.ai[1];

	public override string Texture => $"Terraria/Images/NPC_{NPCID.DD2EterniaCrystal}";

	public override void SetDefaults()
	{
		NPC.width = 48;
		NPC.height = 80;
		NPC.lifeMax = 1;
		NPC.friendly = true;
		NPC.immortal = true;
		NPC.dontTakeDamage = true;
		NPC.aiStyle = -1;
		NPC.knockBackResist = 0f;
		NPC.GetGlobalNPC<NPCDespawning>().NeverDespawn = true;
	}

	public override void AI()
	{
		NPC.velocity = Vector2.Zero;
		NPC.color = RuneboundSystem.GetFamilyColor(Family);

		if (Main.rand.NextBool(5))
		{
			Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Enchanted_Gold, Scale: 1.15f);
			dust.noGravity = true;
			dust.velocity *= 0.2f;
		}
	}

	public override bool CanChat()
	{
		return true;
	}

	public override string GetChat()
	{
		return Language.GetTextValue("Mods.PathOfTerraria.NPCs.BindingMonolith.Preview", Grade, Family);
	}

	public override void SetChatButtons(ref string button, ref string button2)
	{
		button = Language.GetTextValue("Mods.PathOfTerraria.NPCs.BindingMonolith.BreakBinding");
		button2 = string.Empty;
	}

	public override void OnChatButtonClicked(bool firstButton, ref string shopName)
	{
		if (!firstButton)
		{
			return;
		}

		// ActivateEncounter removes this NPC immediately in singleplayer. Close the chat while the
		// NPC is still a valid target so vanilla chat drawing cannot retain its index for another frame.
		Main.CloseNPCChatOrSign();

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			RuneboundActivateHandler.Send((short)NPC.whoAmI);
		}
		else
		{
			Activate(Main.LocalPlayer);
		}

		SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen, NPC.Center);
	}

	public void Activate(Player player)
	{
		RuneboundSystem.ActivateEncounter(EncounterId, player);
	}
}
