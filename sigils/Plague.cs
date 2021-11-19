// Using Inscryption
using UnityEngine;
using DiskCardGame;

// Modding Inscryption
using APIPlugin;

using System.Collections;
using System.Collections.Generic;
using System.IO; // Loading Sigil and Card art.
using System.Linq;

namespace PhantomsSackOSigils
{
    public partial class Plugin
    {
        private NewAbility AddPlague()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.powerLevel = 0; // Don't know what this really should be. Either way it's not high.
            info.rulebookName = "Plague";
            info.rulebookDescription = "[creature] dies after 3 rounds. It also infects adjacent cards.";
            info.metaCategories = new List<AbilityMetaCategory> { AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part1Modular };
            info.opponentUsable = true; // I don't see why not.

            List<DialogueEvent.Line> lines = new List<DialogueEvent.Line>();
            DialogueEvent.Line line = new DialogueEvent.Line();
            line.text = "It's contagious.";
            lines.Add(line);
            info.abilityLearnedDialogue = new DialogueEvent.LineSet(lines);

            // Load Plague sigal image.
            byte[] imgBytes = System.IO.File.ReadAllBytes(Path.Combine(this.Info.Location.Replace("PhantomsSackOSigils.dll", ""), "Artwork/plague.png"));
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imgBytes);

            NewAbility ability = new NewAbility(info, typeof(Plague), tex, AbilityIdentifier.GetAbilityIdentifier("hannah.inscryption.phantomssackosigils", info.rulebookName));
            Plague._ability = ability.ability;
            return ability;
        }

        public class Plague : AbilityBehaviour
        {
            public override Ability Ability { get { return _ability; } }
            public static Ability _ability;

            private int deathCountdown;

            private void Start()
            {
                deathCountdown = 3;
            }

            public override bool RespondsToPlayFromHand()
            {
                return true;
            }

            public override IEnumerator OnPlayFromHand()
            {
                yield return PreSuccessfulTriggerSequence();
                deathCountdown = 3;
                yield break;
            }

            public override bool RespondsToAttackEnded()
            {
                return true;
            }

            public override IEnumerator OnAttackEnded()
            {
                yield return PreSuccessfulTriggerSequence();

                // Plague
                deathCountdown--;
                if (deathCountdown <= 0) 
                {
                    yield return new WaitForSeconds(.25f);
                    LearnAbility();

                    // If this card was infected, it should remove the ability at the end of it's life.
                    if(Card.TemporaryMods.Any(s => s.HasAbility(_ability)))
                        Card.RemoveTemporaryMod(Card.TemporaryMods.Single(s => s.HasAbility(_ability)));

                    // If the game ends before an infected card was killed idk if it removes the ability...
                    // TODO

                    Card.Die(false);
                }

                // Contagious
                base.Card.Anim.LightNegationEffect();

                var a1 = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, true);
                PlayableCard left = a1 ? a1.Card : null;
                CardSlot a2 = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, false);
                PlayableCard right = a2 ? a2.Card : null;

                if (left && !left.Info.HasAbility(Plague._ability))
                {
                    yield return new WaitForSeconds(.25f);
                    Infect(left);
                    yield return left.Anim.FlipInAir();
                }
                if (right && !right.Info.HasAbility(Plague._ability))
                {
                    yield return new WaitForSeconds(.25f);
                    Infect(right);
                    yield return right.Anim.FlipInAir();
                }

                yield break;
            }

            private void Infect(PlayableCard cardToInfect) 
            {
                LearnAbility();
                var mod = new CardModificationInfo();
                mod.abilities = new List<Ability> { Plague._ability };
                // Infect it only as a temporary mod.
                cardToInfect.AddTemporaryMod(mod);
            }
        }
    }
}
