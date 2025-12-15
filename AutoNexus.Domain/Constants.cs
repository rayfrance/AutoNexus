using System.Collections.ObjectModel;

namespace AutoNexus.Domain
{
    public static class Constants
    {
        #region Roles
        public const string ADMIN_ROLE = "Admin";
        public const string SELLER_ROLE = "Vendedor";

        public static readonly IReadOnlyCollection<string> AllRoles = new ReadOnlyCollection<string>(new[]
        {
            ADMIN_ROLE,
            SELLER_ROLE
        });
        #endregion

        #region Seed Users
        public const string ADMIN_EMAIL = "admin@autonexus.com";
        public const string ADMIN_PASSWORD = "Admin@123";
        public const string SELLER_EMAIL = "vendedor@autonexus.com";
        public const string SELLER_PASSWORD = "Vendedor@123";
        #endregion

        #region External Services
        public const string FIPE_URL = "https://parallelum.com.br/fipe/api/v1/carros";
        #endregion
    }
}