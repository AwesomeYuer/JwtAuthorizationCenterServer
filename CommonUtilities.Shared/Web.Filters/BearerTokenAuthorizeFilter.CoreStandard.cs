#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Primitives;
    using System;
    using System.Linq;
    using System.Threading;
    [Flags]
    public enum TokenStoreFlags : ushort
    {
        Header = 0b0000_00001
        , Cookie = 0b0000_00010
    }

    public class BearerTokenBasedAuthorizeWebApiFilter
                    :
                        //AuthorizeAttribute
                        Attribute
                        , IActionFilter
    {
        public static int InstancesSeed = 0;
        public int InstanceID
        {
            private set;
            get;
        }

        public BearerTokenBasedAuthorizeWebApiFilter()
        {
            Initialize();
        }
        public virtual void Initialize()
        {
            InstanceID = Interlocked.Increment(ref InstancesSeed);
        }

        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            IConfiguration configuration = (IConfiguration)context.HttpContext.RequestServices.GetService(typeof(IConfiguration));

            var request = context.HttpContext.Request;
            StringValues token = string.Empty;
            var ok = false;

            var jwtName = configuration
                            .GetSection("TokenName")
                            .Value;
            var jwtCarrier = Enum
                            .Parse<TokenStoreFlags>
                                (
                                    configuration
                                        .GetSection("TokenCarrier")
                                        .Value
                                    , true
                                );
            var jwtIssuer = configuration
                                .GetSection("Issuer")
                                .Value;
            var jwtAudiences = configuration
                                .GetSection("Audiences")
                                .AsEnumerable()
                                .Select
                                    (
                                        (x) =>
                                        {
                                            return
                                                x.Value;
                                        }
                                    )
                                .ToArray();
            var jwtNeedValidIP = bool
                                .Parse
                                    (
                                        configuration
                                            .GetSection("NeedValidIP")
                                            .Value
                                    );
            var jwtSecretKey = configuration
                                .GetSection("SecretKey")
                                .Value;
            var jwtExpireInSeconds = int
                                    .Parse
                                        (
                                            configuration
                                                .GetSection("ExpireInSeconds")
                                                .Value
                                        );

            token = context.HttpContext.Items[jwtName].ToString();

            if (ok)
            {
                ok = JwtTokenHelper
                            .TryValidateToken
                                (
                                    jwtSecretKey
                                    , token
                                    , out var validatedPlainToken
                                    , out var claimsPrincipal
                                );
                if (ok)
                {
                    if (jwtExpireInSeconds > 0)
                    {
                        var iat = claimsPrincipal
                                        .GetIssuedAtLocalTime();
                        var diffNowSeconds = DateTimeHelper
                                                .SecondsDiffNow(iat.Value);
                        ok =
                                            (
                                                (
                                                    diffNowSeconds
                                                    >=
                                                    0
                                                )
                                                &&
                                                (
                                                    diffNowSeconds
                                                    <=
                                                    jwtExpireInSeconds
                                                )
                                            );
                    }
                }
                if (ok)
                {
                    ok = (string.Compare(validatedPlainToken.Issuer, jwtIssuer, true) == 0);
                }
                if (ok)
                {
                    ok = jwtAudiences
                             .Any
                                 (
                                     (x) =>
                                     {
                                         return
                                             validatedPlainToken
                                                     .Audiences
                                                     .Any
                                                         (
                                                             (xx) =>
                                                             {
                                                                 return
                                                                     (xx == x);
                                                             }
                                                         );
                                     }
                                 );
                }
                if (ok)
                {
                    var userName1 = context.HttpContext.User.Identity.Name;
                    var userName2 = claimsPrincipal.Identity.Name;
                    ok = (string.Compare(userName1, userName2, true) == 0);
                }
                if (ok)
                {
                    if (jwtNeedValidIP)
                    {
                        var requestIpAddress =
                                            context
                                                .HttpContext
                                                .Connection
                                                .RemoteIpAddress;
                        var tokenIpAddress = claimsPrincipal.GetClientIP();
                        ok = (requestIpAddress.ToString() == tokenIpAddress.ToString());
                    }
                }
                if (ok)
                {
                    context.HttpContext.User = claimsPrincipal;
                }
            }
            if (!ok)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
        public virtual void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
#endif