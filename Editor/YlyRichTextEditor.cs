/*
Copyright (C) 2016 yly(cantry100@163.com) - All Rights Reserved
YLY富文本编辑器
author：雨Lu尧
blog：http://www.hiwrz.com
*/ 
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

public class YlyRichTextMenu
{
	[MenuItem("GameObject/UI/YlyRichText", false, 0)]
	[MenuItem("UI/YlyRichText", false, 0)]
	public static void CreateYlyRichTextGo()
	{
		UnityEngine.Object[] selObjs = Selection.GetFiltered (typeof(UnityEngine.GameObject), SelectionMode.TopLevel);
		if (selObjs == null || selObjs.Length == 0) {
			return;
		}

		GameObject parentGo = (GameObject)selObjs[0];
		if(PrefabUtility.GetPrefabType(parentGo) == PrefabType.Prefab || PrefabUtility.GetPrefabType(parentGo) == PrefabType.ModelPrefab){
			return;
		}

		GameObject ylyRichTextGo = new GameObject("YlyRichText");
		ylyRichTextGo.transform.SetParent(parentGo.transform, false);

		YlyRichText ylyRichTextC = ylyRichTextGo.AddComponent<YlyRichText>();
		RectTransform rt = ylyRichTextGo.GetComponent<RectTransform>();
		rt.sizeDelta = new Vector2(160f, 60f);

		Selection.activeGameObject = ylyRichTextGo;
	}
}

[CustomEditor(typeof(YlyRichText), true)]
[CanEditMultipleObjects]
public class QtzRichTextEditor : GraphicEditor
{
	YlyRichText _target;
	SerializedProperty text;
	SerializedProperty fontSize;
	SerializedProperty lineHeght;
	SerializedProperty lineSpacing;
	SerializedProperty offCharX;
	SerializedProperty maxChars;
	SerializedProperty isCustomWidthToNewLine;
	SerializedProperty customWidthToNewLine;
	SerializedProperty isNeedOutLine;
	SerializedProperty outLineColor;
	SerializedProperty isAutoAdaptiveWidthHeight;
	SerializedProperty horizontalOverflow;
	SerializedProperty verticalOverflow;
	SerializedProperty textColor;
	SerializedProperty alignment;
	SerializedProperty enableRichText;

	protected override void OnEnable()
	{
		base.OnEnable();
		_target = target as YlyRichText;
		text = serializedObject.FindProperty("m_Text");
		fontSize = serializedObject.FindProperty("m_FontSize");
		lineHeght = serializedObject.FindProperty("m_LineHeght");
		lineSpacing = serializedObject.FindProperty("m_LineSpacing");
		offCharX = serializedObject.FindProperty("m_OffCharX");
		maxChars = serializedObject.FindProperty("m_MaxChars");
		isCustomWidthToNewLine = serializedObject.FindProperty("m_IsCustomWidthToNewLine");
		customWidthToNewLine = serializedObject.FindProperty("m_CustomWidthToNewLine");
		isNeedOutLine = serializedObject.FindProperty("m_IsNeedOutLine");
		outLineColor = serializedObject.FindProperty("m_OutLineColor");
		isAutoAdaptiveWidthHeight = serializedObject.FindProperty("m_IsAutoAdaptiveWidthHeight");
		horizontalOverflow = serializedObject.FindProperty("m_HorizontalOverflow");
		verticalOverflow = serializedObject.FindProperty("m_VerticalOverflow");
		textColor = serializedObject.FindProperty("m_TColor");
		alignment = serializedObject.FindProperty("m_Alignment");
		enableRichText = serializedObject.FindProperty("m_EnableRichText");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField(text);
		if (text.stringValue != _target.text) {
			_target.text = text.stringValue;
		}

		Font fnt = EditorGUILayout.ObjectField("TTF Font", _target.font, typeof(Font), false) as Font;
		if (fnt != _target.font) {
			_target.font = fnt;
		}

		EditorGUILayout.PropertyField(fontSize);
		if(fontSize.intValue <= 0){
			fontSize.intValue = 1;
		}
		if (fontSize.intValue != _target.fontSize) {
			_target.fontSize = fontSize.intValue;
		}
		EditorGUILayout.PropertyField(lineHeght);
		if (lineHeght.floatValue != _target.lineHeght) {
			_target.lineHeght = lineHeght.floatValue;
		}
		EditorGUILayout.PropertyField(lineSpacing);
		if (lineSpacing.floatValue != _target.lineSpacing) {
			_target.lineSpacing = lineSpacing.floatValue;
		}
		EditorGUILayout.PropertyField(offCharX);
		if (offCharX.floatValue != _target.offCharX) {
			_target.offCharX = offCharX.floatValue;
		}
		EditorGUILayout.PropertyField(maxChars);
		if(maxChars.intValue < 0){
			maxChars.intValue = 0;
		}
		if (maxChars.intValue != _target.maxChars) {
			_target.maxChars = maxChars.intValue;
		}

		EditorGUILayout.PropertyField(isCustomWidthToNewLine);
		if (isCustomWidthToNewLine.boolValue != _target.isCustomWidthToNewLine) {
			_target.isCustomWidthToNewLine = isCustomWidthToNewLine.boolValue;
		}
		if(isCustomWidthToNewLine.boolValue){
			EditorGUILayout.PropertyField(customWidthToNewLine);
			if (customWidthToNewLine.floatValue != _target.customWidthToNewLine) {
				_target.customWidthToNewLine = customWidthToNewLine.floatValue;
			}
		}

		EditorGUILayout.PropertyField(isNeedOutLine);
		if (isNeedOutLine.boolValue != _target.isNeedOutLine) {
			_target.isNeedOutLine = isNeedOutLine.boolValue;
		}
		EditorGUILayout.PropertyField(outLineColor);
		if (outLineColor.colorValue != _target.outLineColor) {
			_target.outLineColor = outLineColor.colorValue;
		}
		EditorGUILayout.PropertyField(isAutoAdaptiveWidthHeight);
		if (isAutoAdaptiveWidthHeight.boolValue != _target.isAutoAdaptiveWidthHeight) {
			_target.isAutoAdaptiveWidthHeight = isAutoAdaptiveWidthHeight.boolValue;
		}
		EditorGUILayout.PropertyField(horizontalOverflow);
		if (horizontalOverflow.enumValueIndex != (int)_target.horizontalOverflow) {
			_target.horizontalOverflow = (HorizontalWrapMode)horizontalOverflow.enumValueIndex;
		}
		EditorGUILayout.PropertyField(verticalOverflow);
		if (verticalOverflow.enumValueIndex != (int)_target.verticalOverflow) {
			_target.verticalOverflow = (VerticalWrapMode)verticalOverflow.enumValueIndex;
		}
		EditorGUILayout.PropertyField(textColor);
		if (textColor.colorValue != _target.color) {
			_target.color = textColor.colorValue;
		}
		EditorGUILayout.PropertyField(alignment);
		if (alignment.enumValueIndex != (int)_target.alignment) {
			_target.alignment = (TextAnchor)alignment.enumValueIndex;
		}
		EditorGUILayout.PropertyField(enableRichText);
		if (enableRichText.boolValue != _target.enableRichText) {
			_target.enableRichText = enableRichText.boolValue;
		}

		RaycastControlsGUI();
		serializedObject.ApplyModifiedProperties();
	}
}
