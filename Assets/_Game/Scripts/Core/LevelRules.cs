namespace SmashFest.Core
{
    
    public static class LevelRules
    {
       

        private const int SuperHardInterval = 10;
        private const int HardInterval = 4;

        private const int NormalReward = 20;
        private const int HardReward = 50;
        private const int SuperHardReward = 100;


        public static LevelType GetLevelType(int levelNumber)
        {
            if (levelNumber % SuperHardInterval == 0)
            {
                return LevelType.SuperHard;
            }

            if (levelNumber % HardInterval == 0)
            {
                return LevelType.Hard;
            }

            return LevelType.Normal;
        }

        public static int GetReward(LevelType levelType)
        {
            switch (levelType)
            {
                case LevelType.SuperHard:
                    return SuperHardReward;
                case LevelType.Hard:
                    return HardReward;
                default:
                    return NormalReward;
            }
        }
    }
}
