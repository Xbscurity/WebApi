using api.Controllers.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace api.Tests.Unit.Architecture
{
    public class SecurityArchitectureTests
    {
        private static readonly HashSet<string> AllowedAnonymousEndpoints = new()
    {
        $"{nameof(AuthController)}.{nameof(AuthController.Register)}",
        $"{nameof(AuthController)}.{nameof(AuthController.Login)}",
        $"{nameof(AuthController)}.{nameof(AuthController.Refresh)}",
        $"{nameof(AuthController)}.{nameof(AuthController.Logout)}"
    };

        [Fact]
        public void AllowAnonymous_ShouldOnlyBeAppliedToWhitelistedEndpoints()
        {
            var actualAnonymousEndpoints = typeof(Program).Assembly
                .GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
                .SelectMany(type => type.GetMethods(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                .Where(method => !method.IsSpecialName)
                .Where(HasAllowAnonymous)
                .Select(method => $"{method.DeclaringType!.Name}.{method.Name}")
                .ToHashSet();

            var unexpectedEndpoints = actualAnonymousEndpoints.Except(AllowedAnonymousEndpoints).ToList();
            Assert.True(
                !unexpectedEndpoints.Any(),
                $"SECURITY ISSUE DETECTED: The following endpoints have [AllowAnonymous] but are not included in the whitelist:\n" +
                string.Join("\n", unexpectedEndpoints)
            );

            var redundantEndpoints = AllowedAnonymousEndpoints.Except(actualAnonymousEndpoints).ToList();
            Assert.True(
                !redundantEndpoints.Any(),
                $"TEST CLEANUP: The following endpoints are included in the whitelist but are no longer anonymous:\n" +
                string.Join("\n", redundantEndpoints)
            );
        }

        private static bool HasAllowAnonymous(MethodInfo method)
        {
            var hasOnMethod = method.GetCustomAttributes(true).OfType<IAllowAnonymous>().Any();
            var hasOnClass = method.DeclaringType!.GetCustomAttributes(true).OfType<IAllowAnonymous>().Any();

            return hasOnMethod || hasOnClass;
        }
    }
}
