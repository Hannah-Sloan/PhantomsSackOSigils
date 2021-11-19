// Using Inscryption
using UnityEngine;
using DiskCardGame;

// Modding Inscryption
using APIPlugin;

using System.Collections;
using System.Collections.Generic;
using System.IO; // Loading Sigil and Card art.

namespace PhantomsSackOSigils
{
    public partial class Plugin
    {
        private NewAbility AddEatMe()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.powerLevel = 0; // Don't know what this really should be. Either way it's not high.
            info.rulebookName = "Eat Me";
            info.rulebookDescription = "A creature loses 1 power and gains 1 health when summoned using [creature] as a sacrifice.";
            info.metaCategories = new List<AbilityMetaCategory> { AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part1Modular };
            info.opponentUsable = true; // I don't see why not.

            List<DialogueEvent.Line> lines = new List<DialogueEvent.Line>();
            DialogueEvent.Line line = new DialogueEvent.Line();
            line.text = "Makes you larger.";
            lines.Add(line);
            info.abilityLearnedDialogue = new DialogueEvent.LineSet(lines);

            // Load EatMe sigal image.
            byte[] imgBytes = System.IO.File.ReadAllBytes(Path.Combine(this.Info.Location.Replace("PhantomsSackOSigils.dll", ""), "Artwork/eatme.png"));
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imgBytes);

            NewAbility ability = new NewAbility(info, typeof(EatMe), tex, AbilityIdentifier.GetAbilityIdentifier("hannah.inscryption.phantomssackosigils", info.rulebookName));
            EatMe._ability = ability.ability;
            return ability;
        }

        public class EatMe : AbilityBehaviour
        {
            public override Ability Ability { get { return _ability; } }
            public static Ability _ability;

            private CardModificationInfo mod;

            private void Start()
            {
                mod = new CardModificationInfo();
                mod.healthAdjustment = (+1);
                mod.attackAdjustment = (-1);
            }

            public override bool RespondsToSacrifice()
            {
                return true;
            }

            public override IEnumerator OnSacrifice()
            {
                yield return base.PreSuccessfulTriggerSequence();
                if (Singleton<BoardManager>.Instance.currentSacrificeDemandingCard.Attack > 0) 
                {
                    Singleton<BoardManager>.Instance.currentSacrificeDemandingCard.AddTemporaryMod(this.mod);
                    Singleton<BoardManager>.Instance.currentSacrificeDemandingCard.OnStatsChanged();
                    yield return new WaitForSeconds(0.25f);
                    yield return base.LearnAbility(0.25f);
                }

                yield break;
            }
        }
    }
}