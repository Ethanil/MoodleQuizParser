using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class CalculatedQuestion : BaseMultipleQuestion
    {
        /// <value>Property <c>Synchronize</c> represents a magic Moodle-XML value.</value>
        public int Synchronize;
        /// <value>Property <c>DatasetDefinitions</c> represents the Dataset Definitions used in this Calculated Question.</value>
        public List<DatasetDefinition> DatasetDefinitions = new List<DatasetDefinition>();
        public List<CalculatedAnswer> Answers = new List<CalculatedAnswer>();

        public override int CalculateMaxPoints()
        {
            return Answers.Count;
        }
    }
}