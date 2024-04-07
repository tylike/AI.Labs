namespace RuntimePlugin;

public class MediaConcatCommand : FilterCommand
{

    public override string GetCommand(int ident)
    {
        return $"concat={this.ParentSegment}";
    }
}
