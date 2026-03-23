using UnityEngine;
namespace TetrisTactic.Treasure
{
    [CreateAssetMenu(menuName = "Project/Treasure/Treasure Config", fileName = "TreasureConfig")]
    public sealed class TreasureConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int minTreasureCount = 1;
        [SerializeField, Min(1)] private int maxTreasureCount = 2;
        [SerializeField, Min(1)] private int treasureValue = 1;
        public int TreasureValue => treasureValue;
        public int GetTreasureCount(System.Random random, int maxAvailable)
        {
            if (maxAvailable <= 0)
            {
                return 0;
            }
            var clampedMin = Mathf.Min(Mathf.Max(0, minTreasureCount), maxAvailable);
            var clampedMax = Mathf.Min(Mathf.Max(clampedMin, maxTreasureCount), maxAvailable);
            if (clampedMax <= clampedMin)
            {
                return clampedMin;
            }
            return random.Next(clampedMin, clampedMax + 1);
        }
        public static TreasureConfig CreateDefault()
        {
            return CreateInstance<TreasureConfig>();
        }
    }
}
