using System.Collections.ObjectModel;

namespace AutoNexus.Domain
{
    public static class Constants
    {
        #region Roles
        public const string AdminRole = "Admin";
        public const string SellerRole = "Vendedor";

        public static readonly IReadOnlyCollection<string> AllRoles = new ReadOnlyCollection<string>(new[]
        {
            AdminRole,
            SellerRole
        });
        #endregion

        #region Seed Users - Admin
        public const string AdminEmail = "admin@autonexus.com";
        public const string AdminPassword = "Admin@123";
        #endregion

        #region Seed Users - Vendedor
        public const string SellerEmail = "vendedor@autonexus.com";
        public const string SellerPassword = "Vendedor@123";
        #endregion
    }
}