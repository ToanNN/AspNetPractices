namespace AspnetCoreFeatures.OptionPattern;

public class Position
{
    //Fields are not bound
    public const string ConfigSectionName = "Position";
    public string Title { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
}