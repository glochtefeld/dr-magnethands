namespace Magnethands.Networking.Operations
{
    [System.Serializable]
    public class NetChangeScene : NetMsg
    {
        public string Scene { set; get; }
        public NetChangeScene()
        {
            Code = (byte)Operation.Scene;
        }
    }
}
