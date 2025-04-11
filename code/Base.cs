using BepInEx;
using R2API;
using RoR2;
using UnityEngine;

// Namespace for your plugin
namespace Reflector
{
    // BepInEx and R2API dependencies and plugin metadata
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Base : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "That.Coinflip";
        public const string PluginName = "ReflectorPlugin";
        public const string PluginVersion = "1.0.6";

        // Awake is called when the plugin is loaded
        public void Awake()
        {
            // Initialize the custom item
            Reflector.Init();

            Logger.LogInfo("Reflector item successfully initialized!");
        }

        private void Update()
        {
            // Detect F2 key press
            if (Input.GetKeyDown(KeyCode.F2))
            {
                // Get the player's position
                var playerTransform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                // Spawn the Reflector item near the player
                Logger.LogInfo($"Player pressed F2. Spawning Reflector item at coordinates {playerTransform.position}");
                PickupDropletController.CreatePickupDroplet(
                    PickupCatalog.FindPickupIndex(Reflector.reflector.itemIndex),
                    playerTransform.position,
                    playerTransform.forward * 20f
                );
            }
        }
    }
}