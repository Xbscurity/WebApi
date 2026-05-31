using System.Text.Json.Serialization;

namespace api.Dtos.FinancialTransaction
{
    /// <summary>
    /// Represents a grouping key used in financial transaction reports.
    /// </summary>
    /// <remarks>
    /// This abstract record serves as a base type for all supported
    /// report grouping key variants.
    /// </remarks>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(CategoryKey), typeDiscriminator: "category")]
    [JsonDerivedType(typeof(DateKey), typeDiscriminator: "date")]
    [JsonDerivedType(typeof(CategoryAndDateKey), typeDiscriminator: "categoryAndDate")]
    public abstract record ReportKey
    {
        private ReportKey()
        {
        }

        /// <summary>
        /// Represents grouping by transaction category.
        /// </summary>
        public sealed record CategoryKey(string Name) : ReportKey;

        /// <summary>
        /// Represents grouping by year and month of transaction creation date.
        /// </summary>
        public sealed record DateKey(int Year, int Month) : ReportKey;

        /// <summary>
        /// Represents grouping by both category and transaction creation date.
        /// </summary>
        public sealed record CategoryAndDateKey(string Name, int Year, int Month) : ReportKey;
    }
}