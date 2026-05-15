namespace YG
{
    public partial class SavesYG
    {
        public int gold = 0;
        public int totalClicks = 0;
        public int currentLevel = 1;
        public int clicksForNextLevel = 800;
        public int goldPerClick = 1;
        public float goldPerSecond = 0f;
        public int[] upgradeLevels = new int[0];

        public int GetUpgradeLevel(int index)
        {
            if (index < 0 || index >= upgradeLevels.Length)
                return 0;
            return upgradeLevels[index];
        }

        public void SetUpgradeLevel(int index, int level)
        {
            if (index < 0) return;

            if (index >= upgradeLevels.Length)
                System.Array.Resize(ref upgradeLevels, index + 1);

            upgradeLevels[index] = level;
        }
    }
}