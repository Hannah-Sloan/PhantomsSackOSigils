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
        private NewAbility AddDrinkMe()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.powerLevel = 0; // Don't know what this really should be. Either way it's not high.
            info.rulebookName = "Drink Me";
            info.rulebookDescription = "A creature gains 1 power and loses 1 health when summoned using [creature] as a sacrifice.";
            info.metaCategories = new List<AbilityMetaCategory> { AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part1Modular };
            info.opponentUsable = true; // I don't see why not.

            List<DialogueEvent.Line> lines = new List<DialogueEvent.Line>();
            DialogueEvent.Line line = new DialogueEvent.Line();
            line.text = "Makes you smaller.";
            lines.Add(line);
            info.abilityLearnedDialogue = new DialogueEvent.LineSet(lines);

            // Load DrinkMe sigal image.
            byte[] imgBytes = System.IO.File.ReadAllBytes(Path.Combine(this.Info.Location.Replace("PhantomsSackOSigils.dll", ""), "Artwork/drinkme.png"));
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imgBytes);

            NewAbility ability = new NewAbility(info, typeof(DrinkMe), tex, AbilityIdentifier.GetAbilityIdentifier("hannah.inscryption.phantomssackosigils", info.rulebookName));
            DrinkMe._ability = ability.ability;
            return ability;
        }

        public class DrinkMe : AbilityBehaviour
        {
            public override Ability Ability { get { return _ability; } }
            public static Ability _ability;

            private CardModificationInfo mod;

            private void Start()
            {
                mod = new CardModificationInfo();
                mod.healthAdjustment = (-1);
                mod.attackAdjustment = (+1);
            }

            public override bool RespondsToSacrifice()
            {
                return true;
            }

            public override IEnumerator OnSacrifice()
            {
                yield return base.PreSuccessfulTriggerSequence();
                Singleton<BoardManager>.Instance.currentSacrificeDemandingCard.AddTemporaryMod(this.mod);
                Singleton<BoardManager>.Instance.currentSacrificeDemandingCard.OnStatsChanged();
                yield return new WaitForSeconds(0.25f);
                yield return base.LearnAbility(0.25f);

                if (Singleton<BoardManager>.Instance.currentSacrificeDemandingCard.Health <= 0)
                {
                    // In the case the card has 0 health, it should be killed once it is played.
                    // This was harder to implement than I thought, so my quick fix is DeathMarked.
                    // It's an invisible sigil that gets temporarily added:  once the card is played DeathMarked kills it.
                    var mod = new CardModificationInfo();
                    mod.abilities = new List<Ability> { DeathMarked._ability };

                    Singleton<BoardManager>.Instance.currentSacrificeDemandingCard.AddTemporaryMod(mod);

                    yield return new WaitForSeconds(0.25f);
                }
                yield break;
            }
        }
    }
}