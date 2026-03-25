using PG.StarWarsGame.Engine.CommandBar.Xml;
using System.Numerics;
using PG.StarWarsGame.Engine.Rendering;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.CommandBar.Components;

public class CommandBarShellComponent : CommandBarBaseComponent
{
    public override CommandBarComponentType Type => CommandBarComponentType.Shell;

    public string? ModelName { get; }

    public string? ModelPath { get; }

    public Matrix3x4 ModelTransform { get; internal set; } = Matrix3x4.Identity;

    public CommandBarShellComponent(CommandBarComponentData xmlData) : base(xmlData)
    {
        ModelName = xmlData.ModelName;
        if (!string.IsNullOrEmpty(ModelName))
            ModelPath = $"DATA\\ART\\MODELS\\{ModelName}";
    }

    internal void SetOffsetAndScale(Vector3 offset, Vector3 scale)
    {
        var newOffset = new Vector3(0.0f, 0.0f, 0.0f);
        var newScale = new Vector3(1.0f, 1.0f, 1.0f);
        if (XmlData.ModelOffsetX) 
            newOffset.X = offset.X;
        if (XmlData.ModelOffsetY) 
            newOffset.Y = offset.Y;
        if (XmlData.ScaleModelX) 
            newScale.X = scale.X;
        if (XmlData.ScaleModelY) 
            newScale.Y = scale.Y;
        
        newOffset.X = PGMath.Floor(newOffset.X) + 0.5f;
        newOffset.Y = PGMath.Floor(newOffset.Y) + 0.5f;

        ModelTransform = Matrix3x4.Identity
                         * Matrix3x4.Scale(newScale)
                         * Matrix3x4.CreateTranslation(newOffset);
    }
}