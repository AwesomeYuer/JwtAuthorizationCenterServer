#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microshaoft;
    //using Microshaoft.Linq.Dynamic;
    using Microshaoft.WebApi.Controllers;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    public interface IStoreProceduresWebApiService
    {
        (int StatusCode, JToken Result)
                Process
                     (
                        //string connectionID
                        string routeName
                        , JToken parameters = null
                        , Func
                                <
                                    IDataReader
                                    , Type        // fieldType
                                    , string    // fieldName
                                    , int       // row index
                                    , int       // column index
                                    , JProperty   //  JObject Field 对象
                                > onReadRowColumnProcessFunc = null
                        , string httpMethod = "Get"
                        , int commandTimeoutInSeconds = 101
                    );
    }
    public abstract class
                AbstractStoreProceduresService
                                : IStoreProceduresWebApiService
    {
        private static object _locker = new object();
        protected readonly IConfiguration _configuration;

        public AbstractStoreProceduresService(IConfiguration configuration)
        {
            _configuration = configuration;
            Initialize();
        }
        //for override from derived class
        public virtual void Initialize()
        {
            LoadDataBasesConnectionsInfo();
            LoadDynamicExecutors();
        }
        protected IDictionary<string, DataBaseConnectionInfo>
                        GetDataBasesConnectionsInfoProcess
                                    ()
        {
            var cv = _configuration
                            .GetSection("NeedAutoRefreshExecutedTimeForSlideExpire")
                            .Value;

            if (!cv.IsNullOrEmptyOrWhiteSpace())
            {
                if (bool.TryParse(cv, out var b))
                {
                    _needAutoRefreshExecutedTimeForSlideExpire = b;
                }
            }

            cv = _configuration
                        .GetSection("CachedParametersDefinitionExpiredInSeconds")
                        .Value;

            if (!cv.IsNullOrEmptyOrWhiteSpace())
            {
                if (int.TryParse(cv, out var i))
                {
                    _cachedParametersDefinitionExpiredInSeconds = i;
                }
            }

            //var result =
            //        _configuration
            //            .GetSection("Connections")
            //            .AsEnumerable()
            //            .Where
            //                (
            //                    (x) =>
            //                    {
            //                        return
            //                            !x
            //                                .Value
            //                                .IsNullOrEmptyOrWhiteSpace();
            //                    }
            //                )
            //            .GroupBy
            //                (
            //                    (x) =>
            //                    {
            //                        var key = x.Key;
            //                        var i = key.FindIndex(":", 2);
            //                        var rr = key.Substring(0, i);
            //                        return rr;
            //                    }
            //                )
            //            .ToArray();
            
            //var connections = _configuration
            //                        .GetSection("Connections")
            //                        .GetChildren()
            //                        .Select
            //                        (
            //                            (x) =>
            //                            {
            //                               return
                                          
            //                                (
            //                                     x.GetValue<string>("ConnectionID"),
            //                                     x.GetValue<string>("DataBaseType"),
            //                                     x.GetValue<string>("ConnectionString"),
            //                                     x
            //                                        .GetSection("WhiteList")
            //                                        .GetChildren()
            //                                        .Select
            //                                            (
            //                                                (xx) =>
            //                                                {
            //                                                    return
            //                                                        (
            //                                                            xx.GetValue<string>("StoreProcedureName"),
            //                                                            xx.GetValue<string>("StoreProcedureAlias"),
            //                                                            xx.GetValue<string>("AllowedHttpMethods")
            //                                                        );
            //                                                }
            //                                            )
            //                                        .ToArray()
            //                                 );
            //                            }
            //                        )
            //                        //.ToList();
            //                        .ToList
            //                            <
            //                                (
            //                                    string ConnectionID
            //                                    , string DataBaseType
            //                                    , string ConnectionString
            //                                    , 
            //                                        (
            //                                                    string StoreProcedureName
            //                                                    , string StoreProcedureAlias
            //                                                    , string AllowedHttpMethods
            //                                        )
            //                                        [] WhiteList
            //                                )
            //                            >();

            //var dict = connections
            //                .ToDictionary
            //                    (
            //                        (x) =>
            //                        {
            //                            return
            //                                x.ConnectionID;
            //                        }
            //                        ,
            //                        (x) =>
            //                        {
            //                            var r =
            //                            x.WhiteList
            //                                .Select
            //                                    (
            //                                        (xx) =>
            //                                        {
            //                                            (
            //                                                    string ConnectionID
            //                                                    , DataBasesType DataBaseType
            //                                                    , string ConnectionString
            //                                                    , string StoreProcedureName
            //                                                    , string StoreProcedureAlias
            //                                                    , HttpMethodsFlags AllowedHttpMethods
            //                                            ) rr =
            //                                                (
            //                                                    x.ConnectionID
            //                                                    , Enum
            //                                                        .Parse<DataBasesType>
            //                                                            (
            //                                                                x.DataBaseType
            //                                                                , true
            //                                                            )
            //                                                    , x.ConnectionString
            //                                                    , xx.StoreProcedureName
            //                                                    , 
            //                                                        (
            //                                                            !xx
            //                                                                .StoreProcedureAlias
            //                                                                .IsNullOrEmptyOrWhiteSpace()
            //                                                            ?
            //                                                            xx.StoreProcedureAlias
            //                                                            :
            //                                                            xx.StoreProcedureName
            //                                                        )
            //                                                    , 
            //                                                        Enum
            //                                                            .Parse<HttpMethodsFlags>
            //                                                                (
            //                                                                    xx.AllowedHttpMethods
            //                                                                    , true
            //                                                                )
            //                                                );
            //                                            return rr;
            //                                        }

            //                                    )
            //                                .GroupBy
            //                                    (
            //                                        (xx) =>
            //                                        {
            //                                            return xx.StoreProcedureAlias;
            //                                        }
            //                                    )
            //                                 .ToDictionary
            //                                    (
            //                                        (xx) =>
            //                                        {
            //                                            return xx.Key;
            //                                        }
            //                                        ,
            //                                        (xx) =>
            //                                        {
            //                                            xx.

            //                                        }
            //                                        StringComparer.OrdinalIgnoreCase
            //                                    );



                                        
                                                
            //                            return r;
            //                        }
            //                        ,
            //                        StringComparer.OrdinalIgnoreCase
            //                        //StringComparison.OrdinalIgnoreCase
            //                    );



            //var temp =   result   .ToDictionary
            //                (
            //                    (x) =>
            //                    {
            //                        var r = _configuration[$"{x.Key}ConnectionID"];
            //                        return r;
            //                    }
            //                    , (x) =>
            //                    {
            //                        var allowExecuteWhiteList
            //                            = _configuration
            //                                .GetSection($"{x.Key}WhiteList")
            //                                .AsEnumerable()
            //                                .Where
            //                                    (
            //                                        (xx) =>
            //                                        {
            //                                            var v = xx.Value;
            //                                            var rr = !v.IsNullOrEmptyOrWhiteSpace();
            //                                            return rr;
            //                                        }
            //                                    )
            //                                .GroupBy
            //                                    (
            //                                        (xx) =>
            //                                        {
            //                                            var key = xx.Key;
            //                                            var i = key.FindIndex(":", 4);
            //                                            var rr = key.Substring(0, i);
            //                                            return rr;
            //                                        }
            //                                    )
            //                                .ToDictionary
            //                                    (
            //                                        (xx) =>
            //                                        {
            //                                            var key = _configuration[$"{xx.Key}StoreProcedureAlias"];
            //                                            var storeProcedureName = _configuration[$"{xx.Key}StoreProcedureName"];
            //                                            if (key.IsNullOrEmptyOrWhiteSpace())
            //                                            {
            //                                                key = storeProcedureName;
            //                                            }
            //                                            return key;
            //                                        }
            //                                        ,
            //                                        (xx) =>
            //                                        {
            //                                            var storeProcedureName = _configuration[$"{xx.Key}StoreProcedureName"];
            //                                            var s = _configuration[$"{xx.Key}AllowedHttpMethods"];
            //                                            var allowedHttpMethods =
            //                                                        Enum
            //                                                            .Parse<HttpMethodsFlags>
            //                                                                (
            //                                                                    s
            //                                                                    , true
            //                                                                );
            //                                            var rr = new StoreProcedureInfo()
            //                                            {
            //                                                Alias = xx.Key
            //                                                , Name = storeProcedureName
            //                                                , AllowedHttpMethods = allowedHttpMethods
            //                                            };
            //                                            return
            //                                                rr;
            //                                        }
            //                                        ,
            //                                        StringComparer
            //                                                .OrdinalIgnoreCase
            //                                    );
            //                        //var connectionTimeoutInSeconds = 120;
            //                        //int.TryParse
            //                        //        (
            //                        //            configuration[$"{x.Key}ConnectionTimeoutInSeconds"]
            //                        //            , out connectionTimeoutInSeconds
            //                        //        );
            //                        var r = new DataBaseConnectionInfo()
            //                        {
            //                            ConnectionID = _configuration[$"{x.Key}ConnectionID"]
            //                            , ConnectionString = _configuration[$"{x.Key}ConnectionString"]
            //                            //, ConnectionTimeoutInSeconds = _connectionTimeoutInSeconds
            //                            , DataBaseType = Enum.Parse<DataBasesType>(_configuration[$"{x.Key}DataBaseType"], true)
            //                            , AllowExecuteWhiteList = allowExecuteWhiteList
                                      
            //                        };
            //                        // cv = configuration[$"{x.Key}CachedParametersDefinitionExpiredInSeconds"];
            //                        //if (cv != null)
            //                        //{
            //                        //    r.CachedParametersDefinitionExpiredInSeconds = int.Parse(cv);
            //                        //}
            //                        //cv = configuration[$"{x.Key}NeedAutoRefreshExecutedTimeForSlideExpire"];
            //                        //if (cv != null)
            //                        //{
            //                        //    r.NeedAutoRefreshExecutedTimeForSlideExpire = bool.Parse(cv);
            //                        //}
            //                        return r;
            //                    }
            //                    , StringComparer
            //                            .OrdinalIgnoreCase
            //                );
            return
                null;
//                result;
        }
        protected virtual void LoadDataBasesConnectionsInfo
                                    (
                                        
                                    )
        {
            var connections = GetDataBasesConnectionsInfoProcess();
            _locker
                .LockIf
                    (
                        () =>
                        {
                            var r = (_indexedConnections == null);
                            return r;
                        }
                        , () =>
                        {
                            _indexedConnections = connections;
                        }
                    );
        }
        protected virtual string[] GetDynamicLoadExecutorsPathsProcess
                    (
                    )
        {
            var result =
                    _configuration
                        .GetSection("DynamicLoadExecutorsPaths")
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
            return result;
        }
        private class StoreProcedureComparer : IEqualityComparer<IStoreProcedureExecutable>
        {
            public bool Equals
                            (
                                IStoreProcedureExecutable x
                                , IStoreProcedureExecutable y
                            )
            {
                return 
                    (x.DataBaseType == y.DataBaseType);
            }

            public int GetHashCode(IStoreProcedureExecutable obj)
            {
                return -1;
            }
        }
        protected virtual void LoadDynamicExecutors
                        (
                            string dynamicLoadExecutorsPathsJsonFile = "dynamicLoadExecutorsPaths.json"
                        )
        {
            var executingDirectory = Path
                                        .GetDirectoryName
                                                (
                                                    Assembly
                                                        .GetExecutingAssembly()
                                                        .Location
                                                );
            var executors =
                    GetDynamicLoadExecutorsPathsProcess
                            (
                                //dynamicLoadExecutorsPathsJsonFile
                            )
                        .Select
                            (
                                (x) =>
                                {
                                    var path = x;
                                    if (!path.IsNullOrEmptyOrWhiteSpace())
                                    {
                                        if
                                            (
                                                x.StartsWith(".")
                                            )
                                        {
                                            path = path.TrimStart('.', '\\', '/');
                                        }
                                        path = Path.Combine
                                                        (
                                                            executingDirectory
                                                            , path
                                                        );
                                    }
                                    return path;
                                }
                            )
                        .Where
                            (
                                (x) =>
                                {
                                    return
                                        (
                                            !x
                                                .IsNullOrEmptyOrWhiteSpace()
                                            &&
                                            Directory
                                                .Exists(x)
                                        );
                                }
                            )
                        .SelectMany
                            (
                                (x) =>
                                {
                                    var r =
                                        CompositionHelper
                                            .ImportManyExportsComposeParts
                                                <IStoreProcedureExecutable>
                                                    (x);
                                    return r;
                                }
                            );
            var indexedExecutors =
                    executors
                        .Distinct
                            (
                                 new StoreProcedureComparer()
                            )
                        .ToDictionary
                            (
                                (x) =>
                                {
                                    return
                                        x.DataBaseType;
                                }
                                ,
                                (x) =>
                                {
                                    IParametersDefinitionCacheAutoRefreshable
                                        rr = x as IParametersDefinitionCacheAutoRefreshable;
                                    if (rr != null)
                                    {
                                        rr
                                            .CachedParametersDefinitionExpiredInSeconds
                                                = CachedParametersDefinitionExpiredInSeconds;
                                        rr
                                            .NeedAutoRefreshExecutedTimeForSlideExpire
                                                = NeedAutoRefreshExecutedTimeForSlideExpire;
                                    }
                                    return x;
                                }
                                , StringComparer
                                        .OrdinalIgnoreCase
                            );
            _locker
                .LockIf
                    (
                        () =>
                        {
                            var r = (_indexedExecutors == null);
                            return r;
                        }
                        , () =>
                        {
                            _indexedExecutors = indexedExecutors;
                        }
                    );
        }

        private int _cachedParametersDefinitionExpiredInSeconds = 3600;
        protected virtual int CachedParametersDefinitionExpiredInSeconds
        {
            get => _cachedParametersDefinitionExpiredInSeconds;
            private set => _cachedParametersDefinitionExpiredInSeconds = value;
        }
       

        private bool _needAutoRefreshExecutedTimeForSlideExpire = true;
        protected virtual bool NeedAutoRefreshExecutedTimeForSlideExpire
        {
            get => _needAutoRefreshExecutedTimeForSlideExpire;
            private set => _needAutoRefreshExecutedTimeForSlideExpire = value;
        }

        private IDictionary<string, DataBaseConnectionInfo> 
                    _indexedConnections;

        private IDictionary<string, IStoreProcedureExecutable>
                    _indexedExecutors;

        public
            (int StatusCode, JToken Result)
                        Process
                            (
                                string routeName //= "mssql"
                                , JToken parameters = null
                                , Func
                                    <
                                        IDataReader
                                        , Type        // fieldType
                                        , string    // fieldName
                                        , int       // row index
                                        , int       // column index
                                        , JProperty   //  JObject Field 对象
                                    > onReadRowColumnProcessFunc = null
                                , string httpMethod = "Get"
                                , int commandTimeoutInSeconds = 101
                            )
        {
            var r = false;
            JToken result = null;
            r = TryGetStoreProcedureNameAndConnectionID
                    (
                        routeName
                        , httpMethod
                        , out var connectionID
                        , out var storeProcedureName
                    );
            if (r)
            {

                Process
                    (
                        connectionID
                        , storeProcedureName
                        , parameters
                        , onReadRowColumnProcessFunc
                        , httpMethod
                    );

            }

            return (100, result);
        }

        private 
            (int StatusCode, JToken Result)
                        Process
                            (
                                string connectionID //= "mssql"
                                , string storeProcedureName
                                , JToken parameters = null
                                , Func
                                    <
                                        IDataReader
                                        , Type        // fieldType
                                        , string    // fieldName
                                        , int       // row index
                                        , int       // column index
                                        , JProperty   //  JObject Field 对象
                                    > onReadRowColumnProcessFunc = null
                                , string httpMethod = "Get"
                                , int commandTimeoutInSeconds = 101
                            )
        {





            var beginTime = DateTime.Now;
            JToken result = null;
            var r = false;
            int statusCode = 200;
            DataBaseConnectionInfo connectionInfo = null;
            r = _indexedConnections
                            .TryGetValue
                                (
                                    connectionID
                                    , out connectionInfo
                                );
            if (r)
            {
                r = Process
                        (
                            connectionInfo
                            , storeProcedureName
                            , httpMethod
                            , parameters
                            , out result
                            , onReadRowColumnProcessFunc
                            , commandTimeoutInSeconds
                        );
            }
            if (!r)
            {
                statusCode = 403;
                result = null;
                return (statusCode, result);
            }
            result["BeginTime"] = beginTime;
            var endTime = DateTime.Now;
            result["EndTime"] = endTime;
            result["DurationInMilliseconds"]
                    = DateTimeHelper
                            .MillisecondsDiff
                                    (
                                        beginTime
                                        , endTime
                                    );
            return (statusCode, result); 
        }
        private bool Process
            (
                DataBaseConnectionInfo connectionInfo
                , string storeProcedureAliasOrName
                , string httpMethod
                , JToken parameters
                , out JToken result
                , Func
                    <
                        IDataReader
                        , Type        // fieldType
                        , string    // fieldName
                        , int       // row index
                        , int       // column index
                        , JProperty   //  JObject Field 对象
                    > onReadRowColumnProcessFunc = null
                , int commandTimeoutInSeconds = 90
            )
        {
            var r = false;
            result = null;
            var allowExecuteWhiteList = connectionInfo.AllowExecuteWhiteList;
            var storeProcedureName = string.Empty;
            if (allowExecuteWhiteList != null)
            {
                if (allowExecuteWhiteList.Count > 0)
                {
                    r = CheckList
                            (
                                allowExecuteWhiteList
                                , storeProcedureAliasOrName
                                , httpMethod
                                , out StoreProcedureInfo storeProcedureInfo
                            );
                    if (r)
                    {
                        storeProcedureName = storeProcedureInfo.Name;
                    }
                }
            }
            else
            {
                r = true;
            }
            if (r)
            {
                r = Process
                        (
                            connectionInfo.ConnectionString
                            , connectionInfo.DataBaseType.ToString()
                            , storeProcedureName
                            , parameters
                            , out result
                            , onReadRowColumnProcessFunc
                            , commandTimeoutInSeconds
                        );
            }
            return r;
        }
        private bool Process
                        (
                            string connectionString
                            , string dataBaseType
                            , string storeProcedureName
                            , JToken parameters
                            , out JToken result
                            , Func
                                <
                                    IDataReader
                                    , Type        // fieldType
                                    , string    // fieldName
                                    , int       // row index
                                    , int       // column index
                                    , JProperty   //  JObject Field 对象
                                > onReadRowColumnProcessFunc = null
                            , int commandTimeoutInSeconds = 90
                        )
        {
            var r = false;
            result = null;
            IStoreProcedureExecutable executor = null;
            r = _indexedExecutors
                        .TryGetValue
                            (
                                dataBaseType
                                , out executor
                            );
            if (r)
            {
                r = executor
                        .Execute
                            (
                                connectionString
                                , storeProcedureName
                                , parameters
                                , out result
                                , onReadRowColumnProcessFunc
                                , commandTimeoutInSeconds
                            );
            }
            return r;
        }
        private bool Process
                        (
                            string connectionString
                            , string dataBaseType
                            , string storeProcedureName
                            , string parameters
                            , out JToken result
                            , Func
                                <
                                    IDataReader
                                    , Type        // fieldType
                                    , string    // fieldName
                                    , int       // row index
                                    , int       // column index
                                    , JProperty   //  JObject Field 对象
                                > onReadRowColumnProcessFunc = null
                            , int commandTimeoutInSeconds = 90
                        )
        {
            var j = JObject.Parse(parameters);
            var r = Process
                        (
                            connectionString
                            , dataBaseType
                            , storeProcedureName
                            , j
                            , out result
                            , onReadRowColumnProcessFunc
                            , commandTimeoutInSeconds
                        );
            return r;
        }
        public virtual bool TryGetStoreProcedureNameAndConnectionID
                        (
                            string routeName
                            , string httpMethod
                            , out string connectionID
                            , out string storeProcedureName
                        )
        {
            var r = false;
            var configurationSection =
                        _configuration
                                    .GetSection("Routes")
                                    .GetChildren()
                                    .First
                                        (
                                            (x) =>
                                            {
                                                return
                                                    (
                                                        string
                                                            .Compare
                                                                (
                                                                    x.Key
                                                                    , routeName
                                                                    , true
                                                                )
                                                        ==
                                                        0
                                                    );
                                            }
                                        )
                                    //                                    .GetSection(routeName)
                                    .GetChildren()
                                    .First
                                        (
                                            (x) =>
                                            {
                                                if
                                                    (
                                                        !httpMethod
                                                            .StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                                    )
                                                {
                                                    httpMethod = "http" + httpMethod;
                                                }

                                                return
                                                    (
                                                        string
                                                            .Compare
                                                                (
                                                                    x.Key
                                                                    , httpMethod
                                                                    , true
                                                                )
                                                        ==
                                                        0
                                                    );
                                            }
                                        );
            connectionID = configurationSection
                                    .GetValue<string>("ConnectionID");
            r = !connectionID.IsNullOrEmptyOrWhiteSpace();
            if (!r)
            {
                storeProcedureName = string.Empty;
                return r;
            }
            storeProcedureName = configurationSection
                                        .GetValue<string>("StoreProcedureName");
            r = !storeProcedureName.IsNullOrEmptyOrWhiteSpace();
            return r;
        }



        private bool CheckList
                (
                    IDictionary
                        <string, StoreProcedureInfo>
                            allowExecuteWhiteList
                    , string storeProcedureAliasOrName
                    , string httpMethod
                    , out StoreProcedureInfo storeProcedureInfo
                )
        {
            var r = false;
            HttpMethodsFlags httpMethodsFlag;
            storeProcedureInfo = null;
            r = Enum
                    .TryParse<HttpMethodsFlags>
                        (
                            httpMethod
                            , true
                            , out httpMethodsFlag
                        );
            if (r)
            {
                r = allowExecuteWhiteList
                        .TryGetValue
                            (
                                storeProcedureAliasOrName
                                , out storeProcedureInfo
                            );
                if (r)
                {
                    r = storeProcedureInfo
                            .AllowedHttpMethods
                            .HasFlag(httpMethodsFlag);
                }
            }
            return r;
        }
    }
}
#endif