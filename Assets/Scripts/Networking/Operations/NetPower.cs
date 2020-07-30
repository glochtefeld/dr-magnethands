namespace Magnethands.Networking.Operations
{
    [System.Serializable]
    public class NetPower : NetMsg
    {
        public string Suggestion { set; get; }
        public NetPower()
        {
            Code = (byte)Operation.Power;
        }
    }
}
