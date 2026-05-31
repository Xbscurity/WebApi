namespace api.Models
{
    /// <summary>
    /// Defines audit timestamps for entities tracked by the application.
    /// </summary>
    public interface ITrackedEntity
    {
        /// <summary>
        /// Gets or sets the date and time when the entity was created.
        /// </summary>
        DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was updated.
        /// </summary>
        DateTimeOffset UpdatedAt { get; set; }
    }
}
