// Using Inscryption
using UnityEngine;
using DiskCardGame;

// Modding Inscryption
using APIPlugin;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO; // Loading Sigil and Card art.

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

            // Load Plague3 sigal image.
            imgBytes = System.IO.File.ReadAllBytes(Path.Combine(this.Info.Location.Replace("PhantomsSackOSigils.dll", ""), "Artwork/plague3.png"));
            Texture2D tex3 = new Texture2D(2, 2);
            tex3.LoadImage(imgBytes);

            // Load Plague2 sigal image.
            imgBytes = System.IO.File.ReadAllBytes(Path.Combine(this.Info.Location.Replace("PhantomsSackOSigils.dll", ""), "Artwork/plague2.png"));
            Texture2D tex2 = new Texture2D(2, 2);
            tex2.LoadImage(imgBytes);

            // Load Plague1 sigal image.
            imgBytes = System.IO.File.ReadAllBytes(Path.Combine(this.Info.Location.Replace("PhantomsSackOSigils.dll", ""), "Artwork/plague1.png"));
            Texture2D tex1 = new Texture2D(2, 2);
            tex1.LoadImage(imgBytes);

            NewAbility ability = new NewAbility(info, typeof(Plague), tex3, AbilityIdentifier.GetAbilityIdentifier("hannah.inscryption.phantomssackosigils", info.rulebookName));
            Plague._ability = ability.ability;

            Plague.backup = tex;
            Plague.icon3 = tex3;
            Plague.icon2 = tex2;
            Plague.icon1 = tex1;

            return ability;
        }

        public class Plague : AbilityBehaviour
        {
            public override Ability Ability { get { return _ability; } }
            public static Ability _ability;

            #region Properties
            public static Texture2D icon3;
            public static Texture2D icon2;
            public static Texture2D icon1;
            public static Texture2D backup;

            private int deathCountdown;

            // Caching for clean code.
            WaitForSeconds waitShort = new WaitForSeconds(.25f);
            WaitForSeconds waitLong = new WaitForSeconds(.5f);
            #endregion

            #region Initialization
            private void Start()
            {
                Card.abilityIcons.abilityIcons.Single(s => s.Ability == _ability).SetIcon(icon3);
                Card.RenderInfo.OverrideAbilityIcon(_ability, icon3);
                Card.RenderCard();
                deathCountdown = 3;
                IShook = false;
            }

            public override bool RespondsToPlayFromHand() { return true; }
            public override IEnumerator OnPlayFromHand()
            {
                Card.abilityIcons.abilityIcons.Single(s => s.Ability == _ability).SetIcon(icon3);
                Card.RenderInfo.OverrideAbilityIcon(_ability, icon3);
                yield return PreSuccessfulTriggerSequence();
                deathCountdown = 3;
                IShook = false;
                yield break;
            }
            #endregion

            #region Contagious
            bool IShook;
            public override bool RespondsToTurnEnd(bool playerTurnEnd)
            {
                return base.Card.OpponentCard == !playerTurnEnd;
            }
            public override IEnumerator OnTurnEnd(bool playerTurnEnd)
            {
                yield return PreSuccessfulTriggerSequence();
                IShook = false;

                var a1 = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, true);
                PlayableCard left = a1 ? a1.Card : null;
                CardSlot a2 = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, false);
                PlayableCard right = a2 ? a2.Card : null;

                if (left && !(left.Info.HasAbility(Plague._ability) || left.TemporaryMods.Any(s => s.HasAbility(Plague._ability))))
                    yield return Infect(left);
                if (right && !(right.Info.HasAbility(Plague._ability) || right.TemporaryMods.Any(s => s.HasAbility(Plague._ability))))
                    yield return Infect(right);

                yield break;
            }

            private IEnumerator Infect(PlayableCard cardToInfect)
            {
                yield return waitShort;

                // Watch what is happening
                var saveView = ViewManager.Instance.CurrentView;
                ViewManager.Instance.SwitchToView(View.Board, false, false);
                yield return waitShort;

                var mod = new CardModificationInfo();
                mod.abilities = new List<Ability> { Plague._ability };
                if (!IShook)
                {
                    base.Card.Anim.StrongNegationEffect();
                    IShook = true;
                }
                yield return waitLong;
                // Infect it only as a temporary mod.
                cardToInfect.AddTemporaryMod(mod);
                cardToInfect.Anim.LightNegationEffect();
                yield return LearnAbility();

                yield return waitLong;
                yield return waitLong;
            }
            #endregion

            #region Plague
            public override bool RespondsToUpkeep(bool playerUpkeep)
            {
                return base.Card.OpponentCard == !playerUpkeep;
            }
            public override IEnumerator OnUpkeep(bool playerUpkeep)
            {
                yield return PreSuccessfulTriggerSequence();

                ViewManager.Instance.SwitchToView(View.Board, false, false);
                yield return waitShort;

                deathCountdown--;
                var newIcon = deathCountdown switch
                {
                    3 => icon3,
                    2 => icon2,
                    1 => icon1,
                    _ => backup
                };
                Card.RenderInfo.OverrideAbilityIcon(_ability, newIcon);
                Card.RenderCard();
                Card.Anim.LightNegationEffect();
                yield return waitLong;

                if (deathCountdown <= 0)
                {
                    // Watch it die!! ahahahahahaha!!!
                    ViewManager.Instance.SwitchToView(View.Board, false, false);
                    yield return waitShort;

                    LearnAbility();
                    yield return Card.Die(false);
                    yield return waitLong;
                }

                yield break;
            }
            #endregion
        }
    }
}