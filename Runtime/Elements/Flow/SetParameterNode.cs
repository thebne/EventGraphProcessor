using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System;

namespace GraphProcessor
{
	[System.Serializable, NodeMenuItem("Utils/Set Parameter", shortcut = '<')]
	public class SetParameterNode : BaseParameterNode, IDescribableNode
	{
		[Input]
		public object input;

		public override string name => "Set Parameter";

		[CustomPortBehavior(nameof(input))]
		IEnumerable<PortData> GetInputPort(List<SerializableEdge> edges)
		{
			yield return new PortData
			{
				identifier = "input",
				displayName = "Value",
				displayType = (parameter == null) ? typeof(object) : parameter.GetValueType(),
			};
		}

		[CustomPortInput(nameof(input), typeof(object))]
		void PullInput(List<SerializableEdge> edges)
		{
			input = Convert.ChangeType(edges.FirstOrDefault().passThroughBuffer, parameter.GetValueType());
		}

		protected override void Process()
		{
			base.Process();
			if (parameter == null)
			{
				Debug.LogError("Property \"" + parameterGUID + "\" Can't be found !");
				return;
			}

			graph.UpdateExposedParameter(parameter.guid, input);
		}

        public string Describe()
        {
			if (parameter == null)
				return base.ToString();

            return $"set {parameter.name}";
        }
    }
}
