using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System;

namespace GraphProcessor
{
	[System.Serializable, NodeMenuItem("Utils/Get Parameter", shortcut = '>')]
	public class GetParameterNode : BaseParameterNode, IDescribableNode
	{
		[Output]
		public object output;

		public override string name => "Get Parameter";


		[CustomPortBehavior(nameof(output))]
		IEnumerable<PortData> GetOutputPort(List<SerializableEdge> edges)
		{
			yield return new PortData
			{
				identifier = "output",
				displayName = "Value",
				displayType = (parameter == null) ? typeof(object) : parameter.GetValueType(),
				acceptMultipleEdges = true
			};
		}

		[CustomPortOutput(nameof(output), typeof(object))]
		void PushOutput(List<SerializableEdge> edges)
        {
			foreach (var edge in edges)
				edge.passThroughBuffer = output;
        }

		protected override void Process()
		{
			base.Process();

			if (parameter == null)
			{
				Debug.LogError("Property \"" + parameterGUID + "\" Can't be found !");
				return;
			}

			output = parameter.value;
		}

		public string Describe()
		{
			if (parameter == null)
				return base.ToString();

			return $"get {parameter.name}";
		}
	}
}
