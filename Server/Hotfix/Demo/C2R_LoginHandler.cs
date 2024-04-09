using System;
using System.Net;


namespace ET
{
	[MessageHandler]
	public class C2R_LoginHandler : AMRpcHandler<C2R_Login, R2C_Login> //该类是一个消息处理类
	{
		//这里也包含一个Session，与我们游戏客户端的一个Session相对应，相当于在游戏服务器端也有一个电话，这个电话专门连接着游戏客户端
		//有了这个Session后我们就能回复游戏客户端发送过来的登录请求消息，可以使用session.Dispose()关闭通话，但一般不会在服务器端主动关闭，而是有客户端关闭【除非停服维护或者封禁某一个玩家的账号】。
		//Session被关闭后，Socket也会随之关闭。
		protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response, Action reply)
		{
			// 随机分配一个Gate
			StartSceneConfig config = RealmGateAddressHelper.GetGate(session.DomainZone());
			Log.Debug($"gate address: {MongoHelper.ToJson(config)}");
			
			// 向gate请求一个key,客户端可以拿着这个key连接gate
			G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await ActorMessageSenderComponent.Instance.Call(
				config.InstanceId, new R2G_GetLoginKey() {Account = request.Account});

			response.Address = config.OuterIPPort.ToString();
			response.Key = g2RGetLoginKey.Key;
			response.GateId = g2RGetLoginKey.GateId;	
			reply();
		}
	}
}