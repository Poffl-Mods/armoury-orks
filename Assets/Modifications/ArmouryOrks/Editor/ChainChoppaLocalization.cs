using Newtonsoft.Json.Linq;

namespace ArmouryOrks.Editor
{
    internal static partial class ChoppaGenerator
    {
        private static void WriteChainLocalization(JObject strings)
        {
            string[] flavors =
            {
                "'Da teef do da work. Mind yer fingers.'", "'Mek sez da extra bolts make it safe.'",
                "'If it gets stuck, pull harder!'", "'It ain't broken. It'z finkin'.'",
                "'Red button? Wot red button?'", "'If ya can still hear it, it ain't loud enough!'"
            };
            for (int i = 0; i < 6; i++)
            {
                string key = $"chain-choppa-v{i + 1}";
                strings[key + "-name"] = Entry(ChainNames[i]);
                strings[key + "-description"] = Entry("An Ork chainsword whose motor is almost as dangerous as its teeth. "
                    + "Standard strike and cleave remain reliable; its special attacks can kick back, jam or throw the wielder to the ground. "
                    + $"Base damage: {ChainDamageMin[i]}–{ChainDamageMax[i]}. Armour penetration: {ChainPenetration[i]}%. "
                    + "Automatically upgrades outside combat at party levels 16, 26, 36, 44 and 50.");
                strings[key + "-flavor"] = Entry(flavors[i]);
            }
            strings["chain-chain-name"] = Entry("CHAAAAAAAAAAIN!");
            strings["chain-chain-description"] = Entry("Overrev the motor for an attack with 150% weapon damage with normal character modifiers. "
                + "Before the attack, there is a 20% chance of kickback: instead of attacking the target, deal half your tier's base weapon damage to yourself. "
                + "Self-damage cannot critically hit, gains no offensive damage bonuses and is reduced by your defences. "
                + "Cost: 2 AP. Cooldown: 2 rounds from use, including kickback and misses.");
            strings["chain-bite-name"] = Entry("DON'T LET GO!");
            strings["chain-bite-description"] = Entry("Make a normal attack. On a hit, attempt up to three additional attacks against the same target, "
                + "each with 35% weapon damage with normal character modifiers. A miss ends the sequence. "
                + "After each successful additional hit there is a 25% chance the chain jams: the sequence ends and you cannot parry for 1 round. "
                + "Cost: 2 AP for the whole sequence. Cooldown: 3 rounds from use, including misses.");
            strings["chain-red-name"] = Entry("DA BUTTON WUZ RED!");
            strings["chain-red-description"] = Entry("Make a reckless circular attack with 150% weapon damage with normal character modifiers against everyone within 1 cell, including allies but excluding yourself. "
                + "Then roll once: equal chances of no mishap, a jammed motor for 1 round, or being knocked prone for 1 round. "
                + "A jammed motor blocks all three Chain Choppa special abilities; normal strike and cleave remain usable. "
                + "Cost: 3 AP. Cooldown: 4 rounds from use.");
            strings["chain-jam-name"] = Entry("DA MOTOR'S DED!");
            strings["chain-jam-description"] = Entry("The motor is jammed for 1 round. CHAAAAAAAAAAIN!, DON'T LET GO! and DA BUTTON WUZ RED! cannot be used. Normal strike and cleave remain available.");
            strings["chain-no-parry-name"] = Entry("CHAIN'S STUCK!");
            strings["chain-no-parry-description"] = Entry("The chain has caught and the follow-up sequence ended. You cannot parry for 1 round.");
        }
    }
}
