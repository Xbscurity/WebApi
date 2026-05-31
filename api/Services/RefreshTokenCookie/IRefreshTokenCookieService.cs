namespace api.Services.RefreshTokenCookie
{
    /// <summary>
    /// Defines a contract for managing refresh token cookies in HTTP responses.
    /// </summary>
    public interface IRefreshTokenCookieService
    {
        /// <summary>
        /// Sets the refresh token as an HTTP-only cookie on the response.
        /// </summary>
        /// <param name="token">The refresh token value.</param>
        void Set(string token);
    }
}
