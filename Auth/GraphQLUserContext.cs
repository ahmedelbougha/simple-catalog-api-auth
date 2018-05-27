
using System.Security.Claims;
using GraphQL.Authorization;

namespace aspnetcoregraphql.Auth
{
    public class GraphQLUserContext : IProvideClaimsPrincipal
    {
        public ClaimsPrincipal User { get; set; }
        
    }
}