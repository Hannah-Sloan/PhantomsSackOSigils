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
        private NewAbility AddDeathMarked()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.powerLevel = 0; // Don't know what this really should be. Either way it's not high.
            info.rulebookName = "Death Marked";
            info.rulebookDescription = "This sigil is not for you, player.";
            info.metaCategories = new List<AbilityMetaCategory> { AbilityMetaCategory.Part1Rulebook, AbilityMetaCategory.Part1Modular };
            info.opponentUsable = false;

            // Intentionally ommited any lines relating to this sigil.

            // The artwork is just a transparent image.
            byte[] imgBytes = System.IO.File.ReadAllBytes(Path.Combine(this.Info.Location.Replace("PhantomsSackOSigils.dll", ""), "Artwork/deathmarked.png"));
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imgBytes);

            NewAbility ability = new NewAbility(info, typeof(DeathMarked), tex, AbilityIdentifier.GetAbilityIdentifier("hannah.inscryption.phantomssackosigils", info.rulebookName));
            DeathMarked._ability = ability.ability;
            return ability;
        }

        /// <summary>
        /// This is a quick fix I made to kill cards with 0 health. 
        /// </summary>
        public class DeathMarked : AbilityBehaviour
        {
            public override Ability Ability { get { return _ability; } }
            public static Ability _ability;

            public override bool RespondsToResolveOnBoard() { return true; }

            public override IEnumerator OnResolveOnBoard()
            {
                yield return base.PreSuccessfulTriggerSequence();
                // Need to remove itself otherwise every card that gets marked will stay marked!
                // That would kill it everytime it gets played!
                Card.RemoveTemporaryMod(Card.TemporaryMods.Single(s=>s.HasAbility(_ability)));
                yield return Card.Die(false);
                yield break;
            }
        }
    }
}
