using PathOfTerraria.Common.Systems.Runebound;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Common.Encounters;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.NPCs.Runebound;

internal sealed class BindingMonolith : ModNPC
{
	private const string BreakBindingIconPath = $"{nameof(PathOfTerraria)}/Assets/NPCs/Runebound/BindingBreakIcon";
	private const float InteractionRange = 320f;

	private bool activateNextTick;

	public int EncounterId => (int)NPC.ai[2];
	public RuneboundFamily Family => (RuneboundFamily)(int)NPC.ai[0];
	public RunestoneGrade Grade => (RunestoneGrade)(int)NPC.ai[1];
	public bool UsesQuestDialog => NPC.ai[3] == 1f;

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
		if (activateNextTick)
		{
			activateNextTick = false;
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
			return;
		}

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
		return UsesQuestDialog;
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

		// Vanilla continues reading the current talk NPC after this callback returns. Defer closing
		// chat and removing the monolith until the next update so that index remains valid for this draw.
		activateNextTick = true;
	}

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (UsesQuestDialog || activateNextTick || Main.netMode == NetmodeID.Server)
		{
			return;
		}

		Player player = Main.LocalPlayer;
		Texture2D icon = ModContent.Request<Texture2D>(BreakBindingIconPath).Value;
		Vector2 iconCenter = NPC.Top + new Vector2(0f, -22f);
		Rectangle interactionArea = new((int)(iconCenter.X - icon.Width / 2f), (int)(iconCenter.Y - icon.Height / 2f), icon.Width, icon.Height);
		bool inRange = player.active && !player.dead && player.DistanceSQ(NPC.Center) <= InteractionRange * InteractionRange;
		bool hovering = inRange && interactionArea.Contains(Main.MouseWorld.ToPoint());
		float scale = hovering ? 1.12f : 1f;
		Color color = inRange ? Color.White : Color.White * 0.45f;

		spriteBatch.Draw(icon, iconCenter - screenPos, null, color, 0f, icon.Size() / 2f, scale, SpriteEffects.None, 0f);

		if (!hovering)
		{
			return;
		}

		player.mouseInterface = true;
		player.releaseUseItem = false;
		Main.hoverItemName = Language.GetTextValue("Mods.PathOfTerraria.NPCs.BindingMonolith.BreakBinding");

		bool leftClick = Main.mouseLeft && Main.mouseLeftRelease;
		bool rightClick = Main.mouseRight && Main.mouseRightRelease;
		if (!leftClick && !rightClick)
		{
			return;
		}

		Main.mouseLeftRelease &= !leftClick;
		Main.mouseRightRelease &= !rightClick;
		activateNextTick = true;
	}

	public void Activate(Player player)
	{
		RuneboundSystem.ActivateEncounter(EncounterId, player);
	}
}
