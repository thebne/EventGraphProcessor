using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;
using System;

using Status = UnityEngine.UIElements.DropdownMenuAction.Status;

namespace GraphProcessor
{
	public class ToolbarView : VisualElement
	{
		protected enum ElementType
		{
			Button,
			Toggle,
			DropDownButton,
			Separator,
			Custom,
			FlexibleSpace,
		}

		protected class ToolbarButtonData
		{
			public GUIContent		content;
			public ElementType		type;
			public bool				value;
			public bool				visible = true;
			public bool				enabled = true;
			public Color			color = Color.white;
			public Action			buttonCallback;
			public Action< bool >	toggleCallback;
			public int				size;
			public Action			customDrawFunction;
		}

		List< ToolbarButtonData >	leftButtonDatas = new List< ToolbarButtonData >();
		List< ToolbarButtonData >	rightButtonDatas = new List< ToolbarButtonData >();
		protected BaseGraphView		graphView;
		
		ToolbarButtonData showProcessor;
		ToolbarButtonData showParameters;

		public ToolbarView(BaseGraphView graphView)
		{
			name = "ToolbarView";
			this.graphView = graphView;

			graphView.initialized += () => {
				leftButtonDatas.Clear();
				rightButtonDatas.Clear();
				AddButtons();
			};

			Add(new IMGUIContainer(DrawImGUIToolbar));
		}

		protected ToolbarButtonData AddButton(string name, Action callback, bool left = true, bool enabled = true)
			=> AddButton(new GUIContent(name), callback, left, enabled);

		protected ToolbarButtonData AddButton(GUIContent content, Action callback, bool left = true, bool enabled = true)
		{
			var data = new ToolbarButtonData{
				content = content,
				type = ElementType.Button,
				enabled = enabled,
				buttonCallback = callback
			};
			((left) ? leftButtonDatas : rightButtonDatas).Add(data);
			return data;
		}

		protected void AddSeparator(int sizeInPixels = 10, bool left = true)
		{
			var data = new ToolbarButtonData{
				type = ElementType.Separator,
				size = sizeInPixels,
			};
			((left) ? leftButtonDatas : rightButtonDatas).Add(data);
		}

		protected void AddCustom(Action imguiDrawFunction, bool left = true)
		{
			if (imguiDrawFunction == null)
				throw new ArgumentException("imguiDrawFunction can't be null");

			var data = new ToolbarButtonData{
				type = ElementType.Custom,
				customDrawFunction = imguiDrawFunction,
			};
			((left) ? leftButtonDatas : rightButtonDatas).Add(data);
		}

		protected void AddFlexibleSpace(bool left = true)
		{
			((left) ? leftButtonDatas : rightButtonDatas).Add(new ToolbarButtonData{ type = ElementType.FlexibleSpace });
		}

		protected ToolbarButtonData AddToggle(string name, bool defaultValue, Action< bool > callback, bool left = true)
			=> AddToggle(new GUIContent(name), defaultValue, callback, left);

		protected ToolbarButtonData AddToggle(GUIContent content, bool defaultValue, Action< bool > callback, bool left = true)
		{
			var data = new ToolbarButtonData{
				content = content,
				type = ElementType.Toggle,
				value = defaultValue,
				toggleCallback = callback
			};
			((left) ? leftButtonDatas : rightButtonDatas).Add(data);
			return data;
		}

		protected ToolbarButtonData AddDropDownButton(string name, Action callback, bool left = true)
			=> AddDropDownButton(new GUIContent(name), callback, left);

		protected ToolbarButtonData AddDropDownButton(GUIContent content, Action callback, bool left = true)
		{
			var data = new ToolbarButtonData{
				content = content,
				type = ElementType.DropDownButton,
				buttonCallback = callback
			};
			((left) ? leftButtonDatas : rightButtonDatas).Add(data);
			return data;
		}

		/// <summary>
		/// Also works for toggles
		/// </summary>
		/// <param name="name"></param>
		/// <param name="left"></param>
		protected void RemoveButton(string name, bool left)
		{
			((left) ? leftButtonDatas : rightButtonDatas).RemoveAll(b => b.content.text == name);
		}

		/// <summary>
		/// Hide the button
		/// </summary>
		/// <param name="name">Display name of the button</param>
		protected void HideButton(string name)
		{
			leftButtonDatas.Concat(rightButtonDatas).All(b => {
				if (b?.content?.text == name)
					b.visible = false;
				return true;
			});
		}

		/// <summary>
		/// Show the button
		/// </summary>
		/// <param name="name">Display name of the button</param>
		protected void ShowButton(string name)
		{
			leftButtonDatas.Concat(rightButtonDatas).All(b => {
				if (b?.content?.text == name)
					b.visible = true;
				return true;
			});
		}

		/// <summary>
		/// Enable the button
		/// </summary>
		/// <param name="name">Display name of the button</param>
		protected void EnableButton(string name)
		{
			leftButtonDatas.Concat(rightButtonDatas).All(b => {
				if (b?.content?.text == name)
					b.enabled = true;
				return true;
			});
		}

		/// <summary>
		/// Disable the button
		/// </summary>
		/// <param name="name">Display name of the button</param>
		protected void DisableButton(string name)
		{
			leftButtonDatas.Concat(rightButtonDatas).All(b => {
				if (b?.content?.text == name)
					b.enabled = true;
				return true;
			});
		}

		protected void SetButtonColor(string name, Color color)
		{
			leftButtonDatas.Concat(rightButtonDatas).All(b => {
				if (b?.content?.text == name)
					b.color = color;
				return true;
			});
		}

		protected virtual void AddButtons()
		{
			AddButton("Center", graphView.ResetPositionAndZoom);

			bool exposedParamsVisible = graphView.GetPinnedElementStatus< ExposedParameterView >() != Status.Hidden;
			showParameters = AddToggle("Show Parameters", exposedParamsVisible, (v) => graphView.ToggleView< ExposedParameterView>());

			AddButton("Show In Project", () => EditorGUIUtility.PingObject(graphView.graph), false);
		}

		public virtual void UpdateButtonStatus()
		{
			if (showProcessor != null)
				showProcessor.value = graphView.GetPinnedElementStatus< ProcessorView >() != Status.Hidden;
			if (showParameters != null)
				showParameters.value = graphView.GetPinnedElementStatus< ExposedParameterView >() != Status.Hidden;
		}

		void DrawImGUIButtonList(List< ToolbarButtonData > buttons)
		{
			foreach (var button in buttons.ToList())
			{
				if (!button.visible)
					continue;

				var origColor = GUI.contentColor;
				GUI.enabled = button.enabled;
				GUI.contentColor = button.color;

				switch (button.type)
				{
					case ElementType.Button:
						if (GUILayout.Button(button.content, EditorStyles.toolbarButton) && button.buttonCallback != null)
							button.buttonCallback();
						break;
					case ElementType.Toggle:
						EditorGUI.BeginChangeCheck();
						button.value = GUILayout.Toggle(button.value, button.content, EditorStyles.toolbarButton);
						if (EditorGUI.EndChangeCheck() && button.toggleCallback != null)
							button.toggleCallback(button.value);
						break;
					case ElementType.DropDownButton:
						if (EditorGUILayout.DropdownButton(button.content, FocusType.Passive, EditorStyles.toolbarDropDown))
							button.buttonCallback();
						break;
					case ElementType.Separator:
						EditorGUILayout.Separator();
						EditorGUILayout.Space(button.size);
						break;
					case ElementType.Custom:
						button.customDrawFunction();
						break;
					case ElementType.FlexibleSpace:
						GUILayout.FlexibleSpace();
						break;
				}

				GUI.contentColor = origColor;
				GUI.enabled = true;
			}
		}

		protected virtual void DrawImGUIToolbar()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);

			DrawImGUIButtonList(leftButtonDatas);

			GUILayout.FlexibleSpace();

			DrawImGUIButtonList(rightButtonDatas);

			GUILayout.EndHorizontal();
		}
	}
}
