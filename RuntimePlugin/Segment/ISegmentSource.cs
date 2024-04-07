namespace RuntimePlugin;

/// <summary>
/// 指可以做为一个segment的源，可以是一个文件，也可以是一个segment
/// 主要取得segment的名称，即label
/// </summary>
public interface ISegmentSource
{
    /// <summary>
    /// 是指本segment的输出
    /// 即[v1]这样的标签
    /// 将在接收segment中使用
    /// </summary>
    string Label { get; }
    ISegmentSource CreateChildSegment();
} 
