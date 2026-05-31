namespace api.Enums
{
    /// <summary>
    /// Represents the type of a financial transaction.
    /// </summary>
    public enum FinancialTransactionType
    {
        /// <summary>
        /// A transaction that increases available funds.
        /// </summary>
        Income = 1,

        /// <summary>
        /// A transaction that decreases available funds.
        /// </summary>
        Expense = 2,
    }
}
