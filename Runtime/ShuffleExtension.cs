using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public static class ShuffleExtension
    {
        private static readonly System.Random RandomGenerator = new System.Random();

        public static void Shuffle<T>(this IList<T> shuffleList)
        {
            int count = shuffleList.Count;
            while (count > 1)
            {
                count--;
                int randomValue = RandomGenerator.Next(count + 1);
                T value = shuffleList[randomValue];
                shuffleList[randomValue] = shuffleList[count];
                shuffleList[count] = value;
            }
        }
    }
}
