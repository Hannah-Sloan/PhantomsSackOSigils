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
        private NewAbility AddDesperate()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.powerLevel = 0; // Don't know what this really should be. Either way it's not high.
            info.rulebookName = "Desperate";
            info.rulebookDescription = "[creature]'s power is equal to its missing Health.";
            info.metaCategories = new List<AbilityMetaCategory> { AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part1Modular };
            info.opponentUsable = true; // I don't see why not.

            List<DialogueEvent.Line> lines = new List<DialogueEvent.Line>();
            DialogueEvent.Line line = new DialogueEvent.Line();
            line.text = "This creature is more powerful when weak.";
            lines.Add(line);
            info.abilityLearnedDialogue = new DialogueEvent.LineSet(lines);

            // Load Desperate sigal image.
            // MY GIRLFRIEND ALTHEA FRANK MADE THIS ART.
            byte[] imgBytes = System.IO.File.ReadAllBytes(Path.Combine(this.Info.Location.Replace("PhantomsSackOSigils.dll", ""), "Artwork/desperate.png"));
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imgBytes);

            NewAbility ability = new NewAbility(info, typeof(Desperate), tex, AbilityIdentifier.GetAbilityIdentifier("hannah.inscryption.phantomssackosigils", info.rulebookName));
            Desperate._ability = ability.ability;
            return ability;
        }

        public class Desperate : AbilityBehaviour
        {
            public override Ability Ability { get { return _ability; } }
            public static Ability _ability;

            private CardModificationInfo mod; //Idk why we need this if it's private, maybe AbilityBehaviour uses it.

            private void Start() 
            {
                mod = new CardModificationInfo();
                mod.attackAdjustment = 0;
            }

            // Ability activates whenever this card takes at least 1 damage.
            public override bool RespondsToTakeDamage(PlayableCard source)
            {
                return System.Math.Min(source.lastFrameAttack, base.Card.lastFrameHealth) > 0;
            }

            public override IEnumerator OnTakeDamage(PlayableCard source)
            {
                yield return base.PreSuccessfulTriggerSequence();

                // Teach this sigils gimmick if the player hasn't learned it yet.
                yield return new WaitForSeconds(.5f);
                yield return base.LearnAbility(0f);

                yield return new WaitForSeconds(.25f);
                base.Card.Anim.LightNegationEffect();

                // Set attack to blood loss
                var idealAttack = System.Math.Max(0, (Card.MaxHealth - Card.Health));
                mod.attackAdjustment = idealAttack - base.Card.Attack;
                base.Card.AddTemporaryMod(mod);
                base.Card.OnStatsChanged();


                yield break;
            }
        }
    }
}