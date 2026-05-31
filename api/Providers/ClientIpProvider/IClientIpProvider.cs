namespace api.Providers.ClientIpProvider
{
    /// <summary>
    /// Provides access to the client IP address
    /// associated with the current HTTP request.
    /// </summary>
    public interface IClientIpProvider
    {
        /// <summary>
        /// Retrieves the client IP address.
        /// </summary>
        /// <returns>
        /// The client IP address as a string,
        /// or <see langword="null"/> when the address is unavailable.
        /// </returns>
        string? GetClientIp();
    }
}
