using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class DatasetDefinition
    {
        /// <value>Property <c>Status</c> represents the Status of the DataSet(private, public).</value>
        public string Status;
        /// <value>Property <c>Name</c> represents the Name of the DataSet(i.e. the name of the variable).</value>
        public string Name;
        /// <value>Property <c>Type</c> represents the Type of the DataSet(calculated, ...).</value>
        public string Type;
        /// <value>Property <c>Distribution</c> represents the Type of Distribution used(uniform, logarithmic).</value>
        public string Distribution;
        /// <value>Property <c>Minimum</c> represents the Minimum of the DataSet.</value>
        public float Minimum;
        /// <value>Property <c>Maximum</c> represents the Maximum of the DataSet.</value>
        public float Maximum;
        /// <value>Property <c>Decimals</c> represents the Number of decimal digits used in the DataSet.</value>
        public int Decimals;
        /// <value>Property <c>ItemCount</c> represents the Number of generated Items in the DataSet.</value>
        public int ItemCount;
        /// <value>Property <c>DataSetItems</c> represents the pregenerated DataSetItems in the DataSet.</value>
        public List<DatasetItem> DataSetItems = new List<DatasetItem>();
    }
}