using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using SandBox;
using HarmonyLib;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Settings.Base.Global;

namespace PleaseDontDie
{
    public class PleaseDontDie_SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            var harmony = new Harmony("PleaseDontDie");
            harmony.PatchAll();
        }
    }

	class Settings : AttributeGlobalSettings<Settings>
    {
		public override string Id => "PleaseDontDie";
		public override string DisplayName => "Please Don't Die";
		public override string FolderName => "PleaseDontDie";
		public override string FormatType => "json2";

		[SettingPropertyInteger("Companion death probability", 0, 1, "0%", Order = 0, RequireRestart = false, HintText = "The chance a companion can die in battle after base game check.")]
		[SettingPropertyGroup("Please Don't Die")]
		public float CompanionDeathChance { get; set; } = 0;

		[SettingPropertyInteger("Player clan death probability", 0, 1, "0 %", Order = 0, RequireRestart = false, HintText = "The chance a hero from the player clan can die in battle after base game check.")]
		[SettingPropertyGroup("Please Don't Die")]
		public float ClanDeathChance { get; set; } = 0;

		[SettingPropertyInteger("Stranger death probability", 0, 1, "0 %", Order = 0, RequireRestart = false, HintText = "The chance a hero from another clan can die in battle after base game check.")]
		[SettingPropertyGroup("Please Don't Die")]
		public float StrangerDeathChance { get; set; } = 0;
	}

    [HarmonyPatch(typeof(BattleAgentLogic), "OnAgentRemoved")]
    static class PatchOnAgentRemoved
    {
        public static bool Prefix(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
        {
			float roll = MBRandom.RandomFloatRanged(0, 1);
			bool result;

			if (agentState != AgentState.Killed || affectedAgent == null)
            {
				result = true;
            }
			else
            {
				CharacterObject characterObject = (CharacterObject)affectedAgent.Character;
				// Character isn't a hero
				if (characterObject == null || !characterObject.IsHero)
				{
					result = true;
				}
				// Character is a player companion
				else if (characterObject.HeroObject.IsPlayerCompanion)
				{
					result = roll < Settings.Instance.CompanionDeathChance;
				}
				// Character is a hero belonging to the player's clan
				else if (characterObject.HeroObject.Clan == Hero.MainHero.Clan)
                {
					result = roll < Settings.Instance.ClanDeathChance;
                }
				// Character is a hero from another clan
				else
                {
					result = roll < Settings.Instance.StrangerDeathChance;
                }
            }

			if (!result)
            {
				affectedAgent.Origin.SetWounded();
            }

			return result;
		}
    }
}
