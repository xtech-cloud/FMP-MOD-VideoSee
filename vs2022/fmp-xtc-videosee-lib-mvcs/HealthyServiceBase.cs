
//*************************************************************************************
//   !!! Generated by the fmp-cli 1.84.0.  DO NOT EDIT!
//*************************************************************************************

using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using XTC.FMP.LIB.MVCS;
using XTC.FMP.MOD.VideoSee.LIB.Proto;

namespace XTC.FMP.MOD.VideoSee.LIB.MVCS
{
    /// <summary>
    /// Healthy服务层基类
    /// </summary>
    public class HealthyServiceBase : Service
    {
        public HealthyServiceMock mock { get; set; } = new HealthyServiceMock();
    
        /// <summary>
        /// 带uid参数的构造函数
        /// </summary>
        /// <param name="_uid">实例化后的唯一识别码</param>
        /// <param name="_gid">直系的组的ID</param>
        public HealthyServiceBase(string _uid, string _gid) : base(_uid)
        {
            gid_ = _gid;
        }

        /// <summary>
        /// 注入GRPC通道
        /// </summary>
        /// <param name="_channel">GRPC通道</param>
        public void InjectGrpcChannel(GrpcChannel? _channel)
        {
            grpcChannel_ = _channel;
        }


        /// <summary>
        /// 调用Echo
        /// </summary>
        /// <param name="_request">Echo的请求</param>
        /// <returns>错误</returns>
        public virtual async Task<Error> CallEcho(HealthyEchoRequest? _request, object? _context)
        {
            getLogger()?.Trace("Call Echo ...");
            if (null == _request)
            {
                return Error.NewNullErr("parameter:_request is null");
            }

            HealthyEchoResponse? response = null;
            if (null != mock.CallEchoDelegate)
            {
                getLogger()?.Trace("use mock ...");
                response = await mock.CallEchoDelegate(_request);
            }
            else
            {
                var client = getGrpcClient();
                if (null == client)
                {
                    return await Task.FromResult(Error.NewNullErr("client is null"));
                }
                response = await client.EchoAsync(_request);
            }

            getModel()?.UpdateProtoEcho(response, _context);
            return Error.OK;
        }


        /// <summary>
        /// 获取直系数据层
        /// </summary>
        /// <returns>数据层</returns>
        protected HealthyModel? getModel()
        {
            if(null == model_)
                model_ = findModel(HealthyModel.NAME + "." + gid_) as HealthyModel;
            return model_;
        }

        /// <summary>
        /// 获取GRPC客户端
        /// </summary>
        /// <returns>GRPC客户端</returns>
        protected Healthy.HealthyClient? getGrpcClient()
        {
            if (null == grpcChannel_)
                return null;

            if(null == clientHealthy_)
            {
                clientHealthy_ = new Healthy.HealthyClient(grpcChannel_);
            }
            return clientHealthy_;
        }

        /// <summary>
        /// 直系的MVCS的四个组件的组的ID
        /// </summary>
        protected string gid_ = "";

        /// <summary>
        /// GRPC客户端
        /// </summary>
        protected Healthy.HealthyClient? clientHealthy_;

        /// <summary>
        /// GRPC通道
        /// </summary>
        protected GrpcChannel? grpcChannel_;

        /// <summary>
        /// 直系数据层
        /// </summary>
        private HealthyModel? model_;
    }

}
