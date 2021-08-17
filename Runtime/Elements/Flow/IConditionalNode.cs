using System.Collections.Generic;
using System.Reflection;
using GraphProcessor;

public interface IConditionalNode
	{
	IEnumerable<BaseNode> GetExecutedNodes();
	IEnumerable<BaseNode> GetAllPossibleExecutedNodes();

	FieldInfo[] GetNodeFields(); // Provide a custom order for fields (so conditional links are always at the top of the node)
	}