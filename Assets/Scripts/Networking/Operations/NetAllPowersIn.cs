namespace Magnethands.Networking.Operations
{
    [System.Serializable]
    class NetAllPowersIn: NetMsg
    {
        public NetAllPowersIn()
        {
            Code =(byte)Operation.PowersIn;
        }
    }
}
