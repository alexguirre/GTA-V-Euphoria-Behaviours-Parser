namespace GTA_V_Euphoria_Behaviours_Parser
{
    internal class NMParam
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string InitialValue { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public string Description { get; set; }
    }

    internal class NMBehaviour
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public NMParam[] Params { get; set; }
    }
}
