namespace api.Providers.ClientIpProvider
{
    /// <summary>
    /// Default implementation of <see cref="IClientIpProvider"/>.
    /// </summary>
    public class ClientIpProvider : IClientIpProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientIpProvider"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">
        /// The accessor used to retrieve the current HTTP context.
        /// </param>
        public ClientIpProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public string? GetClientIp()
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress;
            if (ip == null)
            {
                return null;
            }

            return ip.IsIPv4MappedToIPv6
                ? ip.MapToIPv4().ToString()
                : ip.ToString();
        }
    }
}
