#if NETCOREAPP2_X
namespace Microshaoft.WebApi.ModelBinders
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web;
    public class JTokenModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext
                                    .HttpContext
                                    .Request;

            JToken jToken = null;
            async void RequestBodyProcess()
            {
                if (request.HasFormContentType)
                {
                    var formBinder = new FormCollectionModelBinder(NullLoggerFactory.Instance);
                    await formBinder.BindModelAsync(bindingContext);
                    if (bindingContext.Result.IsModelSet)
                    {
                        jToken = JTokenWebHelper
                                        .ToJToken
                                            (
                                                (IFormCollection)
                                                    bindingContext
                                                            .Result
                                                            .Model
                                            );
                    }
                }
                else
                {
                    //if (request.IsJsonRequest())
                    {
                        using (var streamReader = new StreamReader(request.Body))
                        {
                            var task = streamReader.ReadToEndAsync();
                            await task;
                            var json = task.Result;
                            if (!json.IsNullOrEmptyOrWhiteSpace())
                            {
                                jToken = JToken.Parse(json);
                            }
                        }
                    }
                }
            }
            void RequestHeaderProcess()
            {
                var qs = request.QueryString.Value;
                qs = HttpUtility
                            .UrlDecode
                                (
                                    qs
                                );
                qs = qs.TrimStart('?');
                var isJson = false;
                try
                {
                    jToken = JToken.Parse(qs);
                    isJson = jToken is JObject;
                }
                catch
                {

                }
                if (!isJson)
                {
                    jToken = request.Query.ToJToken();
                }
            }
            // 取 jwtToken 优先级顺序：Header → QueryString → Body
            StringValues jwtToken = string.Empty;
            IConfiguration configuration = 
                    (IConfiguration) request
                                        .HttpContext
                                        .RequestServices
                                        .GetService
                                            (
                                                typeof(IConfiguration)
                                            );
            var jwtTokenName = configuration
                                .GetSection("TokenName")
                                .Value;
            var needProcessJwtToken = !jwtTokenName.IsNullOrEmptyOrWhiteSpace();
            void JwtTokenProcessInJToken()
            {
                if (needProcessJwtToken)
                {
                    if (jToken != null)
                    {
                        if (StringValues.IsNullOrEmpty(jwtToken))
                        {
                            var j = jToken[jwtTokenName];
                            if (j != null)
                            {
                                jwtToken = j.Value<string>();
                            }
                        }
                    }
                }
            }
            if (needProcessJwtToken)
            {
                request
                    .Headers
                    .TryGetValue
                        (
                           jwtTokenName
                           , out jwtToken
                        );
            }
            RequestHeaderProcess();
            JwtTokenProcessInJToken();
            if
                (
                    string.Compare(request.Method, "post", true) == 0
                )
            {
                RequestBodyProcess();
                JwtTokenProcessInJToken();
                //if (jToken == null)
                //{
                //    RequestHeaderProcess();
                //}
            }
            if (!StringValues.IsNullOrEmpty(jwtToken))
            {
                request
                    .HttpContext
                    .Items
                    .Add
                        (
                            jwtTokenName
                            , jwtToken
                        );
            }
            bindingContext
                    .Result =
                        ModelBindingResult
                                .Success
                                    (
                                        jToken
                                    );
        }
    }
    //public class JTokenModelBinderProvider
    //                        : IModelBinderProvider
    //                            , IModelBinder
    //{
    //    private IModelBinder _binder = new JTokenModelBinder();
    //    public async Task BindModelAsync(ModelBindingContext bindingContext)
    //    {
    //        await _binder.BindModelAsync(bindingContext);
    //    }
    //    public IModelBinder GetBinder(ModelBinderProviderContext context)
    //    {
    //        if (context == null)
    //        {
    //            throw new ArgumentNullException(nameof(context));
    //        }
    //        if (context.Metadata.ModelType == typeof(JToken))
    //        {
    //            //_binder = new JTokenModelBinder();
    //            return _binder;
    //        }
    //        return null;
    //    }
    //}
}
#endif