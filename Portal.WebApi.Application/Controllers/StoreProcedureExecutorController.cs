#if !NETFRAMEWORK4_X && !NETSTANDARD2_0
namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;

    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAllOrigins")]
    public class StoreProcedureExecutorController
                    : AbstractStoreProceduresExecutorControllerBase
    {
        public StoreProcedureExecutorController(IStoreProceduresWebApiService service)
                : base(service)
        {
        }

        public override ActionResult<JToken> ProcessActionRequest([FromRoute] string connectionID, [FromRoute] string storeProcedureName, [ModelBinder(typeof(JTokenModelBinder))] JToken parameters = null, [FromRoute] string resultPathSegment1 = null, [FromRoute] string resultPathSegment2 = null, [FromRoute] string resultPathSegment3 = null, [FromRoute] string resultPathSegment4 = null, [FromRoute] string resultPathSegment5 = null, [FromRoute] string resultPathSegment6 = null)
        {
            return
                new ForbidResult();
        }


        [Authorize]

        [BearerTokenBasedAuthorizeWebApiFilter]

        [HttpDelete]
        [HttpGet]
        [HttpHead]
        [HttpOptions]
        [HttpPatch]
        [HttpPost]
        [HttpPut]
        [
            Route
                (
                    "test2/"
                        + "{storeProcedureName}/"
                )
        ]
        public ActionResult<JToken> ProcessActionRequest11
                        (
                            [FromRoute]
                                string storeProcedureName
                            , [ModelBinder(typeof(JTokenModelBinder))]
                                JToken parameters = null
                        )
        {

            var jj = new JObject();
            jj.Add
                    (
                        "User"
                        , HttpContext
                            .User
                            .Identity
                            .Name
                    );
            if
                (
                    HttpContext
                        .User
                        .TryGetClaimTypeJTokenValue
                            (
                                "Extension"
                                , out var claimValue
                            )
                )
            {
                jj.Add
                    (
                        "ExtensionClaims"
                        , claimValue
                    );
            }
         


            JToken result = null;
            (int StatusCode, JToken Result) r =
                    _service
                        .Process
                            (
                                "mssql2"
                                , "objects"
                                , parameters
                                , (reader, fieldType, fieldName, rowIndex, columnIndex) =>
                                {
                                    JProperty field = null;
                                    if (fieldType == typeof(string))
                                    {
                                        if (fieldName.Contains("Json", System.StringComparison.OrdinalIgnoreCase))
                                        {
                                            fieldName = fieldName.Replace("json", "", System.StringComparison.OrdinalIgnoreCase);
                                            field = new JProperty
                                                            (
                                                                fieldName
                                                                , JObject.Parse(reader.GetString(columnIndex))
                                                            );
                                        }
                                    }
                                    return field;
                                }
                                , Request.Method
                            );
            if (r.StatusCode == 200)
            {
                result =
                    r.Result
                        .GetDescendantByPath
                            (
                                "Outputs"
                                , "ResultSets"
                                , "1"
                                , "Rows"
                            );
            }
            else
            {
                Response
                    .StatusCode = r.StatusCode;
            }
            return
                result;
        }
    }
}
#endif
