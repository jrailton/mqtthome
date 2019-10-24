namespace MqttHome
{
    public class Rule
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Switch { get; set; }
        public string Condition { get; set; }
        public int? FlipFlop { get; set; }
    }
}
