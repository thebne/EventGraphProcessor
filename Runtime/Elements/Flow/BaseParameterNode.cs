using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System;

namespace GraphProcessor
{
	public abstract class BaseParameterNode : LinearConditionalNode
	{
		// We serialize the GUID of the exposed parameter in the graph so we can retrieve the true ExposedParameter from the graph
		[SerializeField, HideInInspector]
		protected string parameterGUID;

		public ExposedParameter parameter { get; protected set; }

		public event Action onParameterChanged;

		public void SetParameterGUID(string guid)
        {
			parameterGUID = guid;
			if (graph != null)
				LoadExposedParameter();
        }

		protected override void Enable()
		{
			// load the parameter
			LoadExposedParameter();

			graph.onExposedParameterModified += OnParamChanged;
			if (onParameterChanged != null)
				onParameterChanged?.Invoke();
		}

		void LoadExposedParameter()
		{
			parameter = graph.GetExposedParameterFromGUID(parameterGUID);
		}

		void OnParamChanged(ExposedParameter modifiedParam)
		{
			if (parameter == modifiedParam)
			{
				onParameterChanged?.Invoke();
			}
		}

		protected override void Process()
		{
#if UNITY_EDITOR // In the editor, an undo/redo can change the parameter instance in the graph, in this case the field in this class will point to the wrong parameter
			parameter = graph.GetExposedParameterFromGUID(parameterGUID);
#endif
		}
	}
}
