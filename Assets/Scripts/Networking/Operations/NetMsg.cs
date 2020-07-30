namespace Magnethands.Networking
{
    public enum Operation
    {
        None,
        Power,
        Art,
        PowersIn,
        Scene
    }

    [System.Serializable]
    public abstract class NetMsg
    {
        public byte Code { set; get; }
        public NetMsg()
        {
            Code = (byte)Operation.None;
        }
    }
}