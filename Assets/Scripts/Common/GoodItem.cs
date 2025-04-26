namespace ETD.Scripts.Common
{
    public class GoodItem
    {
        public GoodType GoodType { get; }
        public double Value { get; }
        public int Param0 { get; }
        
        public GoodItem() { }
        public GoodItem(GoodItem goodItem) : this(goodItem.GoodType, goodItem.Value, goodItem.Param0){ }
        public GoodItem(GoodType goodType, double value, int param0 = 0)
        {
            GoodType = goodType;
            Value = value;
            Param0 = param0;
        }
    }
}