// Modding Inscryption
using BepInEx;

namespace PhantomsSackOSigils
{
    [BepInPlugin("hannah.inscryption.phantomssackosigils", "Phantom's Sack O' Sigils", "1.0.0")]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
    public partial class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {"hannah.inscryption.phantomssackosigils"} is loaded!");

            AddDesperate();
            AddDeathMarked();
            AddDrinkMe();
            AddEatMe();
        }
    }
}
