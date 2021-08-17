using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using GraphProcessor;
using System.Linq;

public class BaseParameterNodeView : BaseNodeView
{
    protected BaseParameterNode node;
    protected Label resultLbl;

    public override void Enable()
    {
        base.Enable();

        node = nodeTarget as BaseParameterNode;

        paramSelector = new PopupField<ExposedParameter>(
            choices: owner.graph.allExposedParameters.ToList(),
            defaultIndex: 0,
            formatListItemCallback: p => p.name, formatSelectedValueCallback: p => p.name);
        paramSelector.RegisterValueChangedCallback(evt =>
        {
            node.SetParameterGUID(evt.newValue.guid);
            UpdateParams();
        });
        var exposed = owner.graph.GetExposedParameterFromGUID(node.parameter?.guid);
        if (exposed != null)
            paramSelector.SetValueWithoutNotify(exposed);

        node.SetParameterGUID(paramSelector.value.guid);
        UpdateParams();

        contentContainer.Add(paramSelector);
        node.onParameterChanged += UpdateParams;

        resultLbl = new Label();
        resultLbl.name = "result-lbl";
        contentContainer.Add(resultLbl);
    }

    public override void Disable()
    {
        base.Disable();

        if (node != null)
            node.onParameterChanged -= UpdateParams;
    }

    PopupField<ExposedParameter> paramSelector;
    void UpdateParams()
    {
        controlsContainer.MarkDirtyRepaint();
        ForceUpdatePorts();
    }
}

[NodeCustomEditor(typeof(GetParameterNode))]
public class GetParameterNodeView : BaseParameterNodeView 
{
    const string kGetParamNoExecutesMessage = "Get Parameter with no thens";
    const string kGetParamExecutesButNoOutputMessage = "then is connected, but output value isn't";

    public override void Enable()
    {
        base.Enable();

        var img = new Image();
        img.image = Resources.Load<Texture2D>("Get Parameter Icon");
        titleContainer.Insert(0, img);

        nodeTarget.onProcessed += HandleProcessed;
        owner.onAfterGraphChanged += HandleGraphChange;
        HandleGraphChange();
    }

    public override void Disable()
    {
        nodeTarget.onProcessed -= HandleProcessed;
        owner.onAfterGraphChanged -= HandleGraphChange;
        base.Disable();
    }

    private void HandleProcessed()
    {
        var n = node as GetParameterNode;
        resultLbl.text = $"Output: {n.output}";
    }

    private void HandleGraphChange()
    {
        var executesCount = node.outputPorts
            .FirstOrDefault(p => p.fieldName == nameof(node.executes))?.GetEdges()?.Count;
        if (executesCount == 0)
            AddMessageView(kGetParamNoExecutesMessage, NodeMessageType.Warning);
        else
            RemoveMessageView(kGetParamNoExecutesMessage);

        var n = node as GetParameterNode;
        if (executesCount > 0 && node.outputPorts
            .FirstOrDefault(p => p.fieldName == nameof(n.output))?.GetEdges()?.Count == 0)
            AddMessageView(kGetParamExecutesButNoOutputMessage, NodeMessageType.Error);
        else
            RemoveMessageView(kGetParamExecutesButNoOutputMessage);
    }
}
[NodeCustomEditor(typeof(SetParameterNode))]
public class SetParameterNodeView : BaseParameterNodeView 
{
    const string kNoInputMessage = "input value not connected";

    public override void Enable()
    {
        base.Enable();

        var img = new Image();
        img.image = Resources.Load<Texture2D>("Set Parameter Icon");
        titleContainer.Insert(0, img);

        nodeTarget.onProcessed += HandleProcessed;
        owner.onAfterGraphChanged += HandleGraphChange;
        HandleGraphChange();
    }

    public override void Disable()
    {
        nodeTarget.onProcessed -= HandleProcessed;
        owner.onAfterGraphChanged -= HandleGraphChange;
        base.Disable();
    }
    private void HandleProcessed()
    {
        var n = node as SetParameterNode;
        resultLbl.text = $"Input: {n.input}";
    }


    private void HandleGraphChange()
    {
        var n = node as SetParameterNode;

        var outputCount = node.inputPorts
            .FirstOrDefault(p => p.fieldName == nameof(n.input))?.GetEdges()?.Count;
        if (outputCount == 0)
            AddMessageView(kNoInputMessage, NodeMessageType.Error);
        else
            RemoveMessageView(kNoInputMessage);
    }
}