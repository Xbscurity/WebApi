using api.Options;
using api.Providers.Time;
using api.Providers.TimeProvider;
using api.Services.Account;
using api.Services.Auth;
using api.Services.Categories;
using api.Services.FinancialTransactions;
using api.Services.Token;
using api.Services.UnitOfWork;
using api.Services.UserManagement;

namespace api
{
    /// <summary>
    /// Provides extension methods for registering application-layer services.
    /// </summary>
    public static class ApplicationServiceCollectionExtensions
    {
        /// <summary>
        /// Registers application services, business logic, and configuration options.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddOptions<RefreshTokenOptions>()
                .BindConfiguration(RefreshTokenOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddScoped<IUnitOfWorkService, UnitOfWorkService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IFinancialTransactionService, FinancialTransactionService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddSingleton<ITimeProvider, UtcTimeProvider>();
            services.AddScoped<IGroupingReportStrategy, GroupByCategoryStrategy>();
            services.AddScoped<IGroupingReportStrategy, GroupByDateStrategy>();
            services.AddScoped<IGroupingReportStrategy, GroupByCategoryAndDateStrategy>();
            return services;
        }
    }
}