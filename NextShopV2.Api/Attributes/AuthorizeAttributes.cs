using Microsoft.AspNetCore.Authorization;

namespace NextShopV2.Api.Attributes
{
    /// <summary>
    /// Admin only access
    /// </summary>
    public class AdminOnlyAttribute : AuthorizeAttribute
    {
        public AdminOnlyAttribute()
        {
            Roles = "Admin";
        }
    }

    /// <summary>
    /// Shipper only access
    /// </summary>
    public class ShipperOnlyAttribute : AuthorizeAttribute
    {
        public ShipperOnlyAttribute()
        {
            Roles = "Shipper";
        }
    }

    /// <summary>
    /// Admin or Shipper access
    /// </summary>
    public class AdminOrShipperAttribute : AuthorizeAttribute
    {
        public AdminOrShipperAttribute()
        {
            Roles = "Admin,Shipper";
        }
    }

    /// <summary>
    /// Admin or User access
    /// </summary>
    public class AdminOrUserAttribute : AuthorizeAttribute
    {
        public AdminOrUserAttribute()
        {
            Roles = "Admin,User";
        }
    }

    /// <summary>
    /// User only access
    /// </summary>
    public class UserOnlyAttribute : AuthorizeAttribute
    {
        public UserOnlyAttribute()
        {
            Roles = "User";
        }
    }

    /// <summary>
    /// Any authenticated user
    /// </summary>
    public class AuthenticatedAttribute : AuthorizeAttribute
    {
        public AuthenticatedAttribute()
        {
            // No roles specified = any authenticated user
        }
    }
}