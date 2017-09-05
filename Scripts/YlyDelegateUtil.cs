/*
Copyright (C) 2016 yly(cantry100@163.com) - All Rights Reserved
YLY函数委托
author：雨Lu尧
blog：http://www.hiwrz.com
*/
using UnityEngine;
using UnityEngine.EventSystems;

public class YlyDelegateUtil{
	public delegate void VoidDelegate(GameObject go, PointerEventData eventData = null);
	public delegate void StringDelegate(string arg);

	/*
	/// <summary>
	/// the function can use for ylyRichText hyperLink's click callBack function for lua, but need tolua support, 
	/// here will not introduce tolua, if your project has tolua plugin, remove the comments, and then export the wrap file
	/// 下面的函数可以用作yly富文本组件超链接在lua那边的点击回调函数，不过需要tolua插件的支持，
	/// 这里就不引入tolua了，有tolua插件的项目，把注释去掉，然后导出wrap文件即可
	/// </summary>
	/// <returns>The delegate.</returns>
	/// <param name="func">Func.</param>
	public static YlyDelegateUtil.StringDelegate StringDelegate(LuaFunction func)
	{
		YlyDelegateUtil.StringDelegate action = (arg) =>
		{
			func.Call(arg);
		};
		return action;
	}
	*/
}
