namespace ETD.Scripts.Common
{
    public class BulletAbility
    {
        public double Power { get; set; }
        public bool IsCritical { get; set; }
        public float Size { get; set; }
        public float DurationTime { get; set; }
        public int ChainCount { get; set; }

        private string _key;

        public BulletAbility(string key)
        {
            _key = key;
        }
    }
}