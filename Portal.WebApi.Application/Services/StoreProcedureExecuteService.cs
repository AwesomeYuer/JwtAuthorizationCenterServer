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
            base
                .LoadDynamicExecutors();
        }
    }
}
