using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using FrameworkThree.Web.Utils;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FrameworkThree.Web.Controllers
{
    public class BaseController : Controller
    {
        public BaseController()
        { }

        public async Task<AuthenticationResult> GetAuthenticationResult()
        {
            string azureADUserObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
            AuthenticationContext authenticationContext = new AuthenticationContext(Startup.AuthenticationAuthority, 
                                                                                new NaiveSessionCache(azureADUserObjectID, HttpContext.Session));

            ClientCredential clientCredential = new ClientCredential(Startup.ApplicationID, Startup.ApplicationKey);
            AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenSilentAsync(Startup.ServiceAppIDURI, 
                                                                                clientCredential, 
                                                                                new UserIdentifier(azureADUserObjectID, UserIdentifierType.UniqueId));

            return authenticationResult;
        }
    }
}
