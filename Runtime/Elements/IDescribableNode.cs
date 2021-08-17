using GraphProcessor;

public interface IDescribableNode
{
    string Describe();
}

public static class DescriabableNodeExtension
{
    public static string Describe(this BaseNode node) 
        => node is IDescribableNode descNode ? descNode.Describe() : node.ToString();
}