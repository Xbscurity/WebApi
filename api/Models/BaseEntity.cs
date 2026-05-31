namespace api.Models
{
    /// <summary>
    /// Represents the base class for persisted entities.
    /// </summary>
    public abstract class BaseEntity : ITrackedEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public Guid Id { get; set; } = Guid.CreateVersion7();

        /// <inheritdoc/>
        public DateTimeOffset CreatedAt { get; set; }

        /// <inheritdoc/>
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
