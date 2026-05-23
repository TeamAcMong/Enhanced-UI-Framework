namespace EnhancedUI.Demo
{
    /// <summary>
    /// Constants for screen resource paths
    /// Used with PageContainer.Push(), ModalContainer.Push(), and SheetContainer.Register()
    ///
    /// Usage:
    /// pageContainer.Push<HomeContainerPage>(ScreenKeys.HomeContainer, options);
    /// sheetContainer.Register(ScreenKeys.HomeSheet);
    /// </summary>
    public static class ScreenKeys
    {
        // Base path for all screens in Resources folder
        private const string ResourcesBase = ""/*"Prefabs/Screens/"*/;
        private const string ModalsBase = ""/*"Prefabs/Modals/"*/;
        private const string SheetsBase = ""/*"Prefabs/Sheets/"*/;

        // Page screens (for PageContainer)
        public const string HomeContainer = ResourcesBase + "HomeContainer"; // Container page with tab sheets
        public const string LevelSelection = ResourcesBase + "LevelSelection";
        public const string Gameplay = ResourcesBase + "Gameplay";
        public const string RPGStage = ResourcesBase + "RPGStageScreen";

        // Tab sheet screens (for SheetContainer inside HomeContainer)
        public const string HomeSheet = SheetsBase + "HomeSheet";
        public const string BattleSheet = SheetsBase + "BattleSheet";
        public const string InventorySheet = SheetsBase + "InventorySheet";
        public const string ShopSheet = SheetsBase + "ShopSheet";

        // Legacy page screen paths (kept for backward compatibility if needed)
        public const string Home = ResourcesBase + "Home";
        public const string BattleArena = ResourcesBase + "Battle";
        public const string Shop = ResourcesBase + "Shop";
        public const string Inventory = ResourcesBase + "Inventory";

        // Modal screens
        public const string Settings = ModalsBase + "SettingsModal";

        /// <summary>
        /// Get full resource path for a screen
        /// </summary>
        public static string GetScreenPath(string screenName)
        {
            return screenName switch
            {
                "Home" => Home,
                "LevelSelection" => LevelSelection,
                "Gameplay" => Gameplay,
                "BattleArena" => BattleArena,
                "Shop" => Shop,
                "Inventory" => Inventory,
                "RPGStage" => RPGStage,
                "Settings" => Settings,
                _ => null
            };
        }
    }
}
