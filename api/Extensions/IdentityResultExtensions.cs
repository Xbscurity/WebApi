using api.Constants;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace api.Extensions
{
    /// <summary>
    /// Provides extension methods for converting
    /// <see cref="IdentityResult"/> instances into application-specific
    /// error representations.
    /// </summary>
    public static class IdentityResultExtensions
    {
        /// <summary>
        /// Converts an <see cref="IdentityResult"/> into a collection
        /// of application-specific <see cref="Error"/> objects.
        /// </summary>
        /// <param name="result">
        /// The identity operation result to convert.
        /// </param>
        /// <returns>
        /// A collection of mapped application errors.
        /// </returns>
        public static List<Error> MapToErrors(this IdentityResult result)
        {
            const string Password = "password";
            const string Email = "email";
            const string Username = "username";
            const string General = "general";
            const string DuplicateUserName = "DuplicateUserName";

            return result.Errors.Select(e =>
            {
                var (code, field) = e.Code switch
                {
                    "PasswordTooShort" => ("PASSWORD_TOO_SHORT", Password),
                    "PasswordRequiresUpper" => ("PASSWORD_UPPER_REQUIRED", Password),
                    "PasswordRequiresLower" => ("PASSWORD_LOWER_REQUIRED", Password),
                    DuplicateUserName => ("USERNAME_TAKEN", Username),
                    "InvalidUserName" => ("USERNAME_INVALID", Username),
                    "InvalidEmail" => ($"EMAIL_INVALID", Email),
                    _ => ($"{e.Code.ToUpperInvariant()}", General)
                };

                var fullCode = $"AUTH_{code}";
                var metadata = new Dictionary<string, object> { [ErrorMetadataKeys.Field] = field };

                return e.Code switch
                {
                    DuplicateUserName
                    => Error.Conflict(fullCode, e.Description, metadata),

                    _ => Error.Validation(fullCode, e.Description, metadata)
                };
            }).ToList();
        }

        /// <summary>
        /// Converts an <see cref="IdentityResult"/> into a dictionary
        /// grouped by error code.
        /// </summary>
        /// <param name="result">
        /// The identity operation result to convert.
        /// </param>
        /// <returns>
        /// A dictionary where the key represents the identity error code
        /// and the value contains the associated error descriptions.
        /// </returns>
        public static Dictionary<string, string[]> ToErrorDictionary(this IdentityResult result)
        {
            return result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Description).ToArray());
        }
    }
}
