namespace Microshaoft.Web
{
    using Microsoft.Extensions.Configuration;
    public class StoreProceduresExecuteService
                            : AbstractStoreProceduresService
    {
        public StoreProceduresExecuteService(IConfiguration configuration)
                    : base(configuration)
        {
        }

        //called by base AbstractStoreProceduresService constructor
        public override void Initialize()
        {
            //using derived class implmention
            LoadDataBasesConnectionsInfo();
            //using base class implmention
            base
                .LoadDynamicExecutors();
        }
        protected override void LoadDataBasesConnectionsInfo()
        {
            // test for override base LoadDataBasesConnectionsInfo implement
            // you can implement by using other process except config json file(that's base method)
            base
                .LoadDataBasesConnectionsInfo();
        }
        //protected override int
        //        CachedParametersDefinitionExpiredInSeconds
        //{
        //    get;
        //    //private set;
        //}
        //protected override bool 
        //        NeedAutoRefreshExecutedTimeForSlideExpire => true;
    }
}
