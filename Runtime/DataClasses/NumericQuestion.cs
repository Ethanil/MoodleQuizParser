using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class NumericQuestion : Question
    {
        public List<NumericAnswer> Answers = new List<NumericAnswer>();
        /// <value>Property <c>UnitGradingType</c> represents to which extend the Type should be used in Grading(Not Used At All, Type is Optional and if used the value gets converted to the correct type, Type is mandatory).</value>
        public int UnitGradingType;
        /// <value>Property <c>UnitPenalty</c> represents the amount of penalty that should be deducted if applicable.</value>
        public float UnitPenalty;
        /// <value>Property <c>ShowUnits</c> represents how the Units will be shown (Free Text, Multiple Choice, Drop Down).</value>
        public int ShowUnits;
        /// <value>Property <c>UnitsLeft</c> indicates wether the Unit is left of the value or on the right.</value>
        public bool UnitsLeft;
        /// <value>Property <c>Units</c> represents the Units used in this Calculated Question.</value>
        public List<Unit> Units = new List<Unit>();

        public override int CalculateMaxPoints()
        {
            return Answers.Count;
        }
    }
}