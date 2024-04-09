using System;


namespace ET
{
    public static class LoginHelper
    {
        //address包含ip和端口号
        public static async ETTask Login(Scene zoneScene, string address, string account, string password)
        {
            try
            {
                // 创建一个ETModel层的Session， 客户端向服务器发起登录请求
                R2C_Login r2CLogin;//声明游戏服务器处理完后返回的消息
                Session session = null;
                try
                {
					//ToIPEndPoint将字符串转换为计算机能理解的二进制地址
					//这段代码相当于创建了会话，通过服务器地址address连接我们的游戏服务器
					session = zoneScene.GetComponent<NetKcpComponent>().Create(NetworkHelper.ToIPEndPoint(address));
					{
                        //session.Call给我们的游戏服务器打一通电话，电话内容为登录消息
                        r2CLogin = (R2C_Login) await session.Call(new C2R_Login() { Account = account, Password = password });
                    }
                }
                finally
                {
                    session?.Dispose();
                }

				// 创建一个gate Session,并且保存到SessionComponent中
				// r2CLogin.Address为服务器返回的地址
				Session gateSession = zoneScene.GetComponent<NetKcpComponent>().Create(NetworkHelper.ToIPEndPoint(r2CLogin.Address));
                gateSession.AddComponent<PingComponent>();
                zoneScene.AddComponent<SessionComponent>().Session = gateSession;
				
                G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(
                    new C2G_LoginGate() { Key = r2CLogin.Key, GateId = r2CLogin.GateId});

                Log.Debug("登陆gate成功!");

                Game.EventSystem.Publish(new EventType.LoginFinish() {ZoneScene = zoneScene});
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        } 
    }
}