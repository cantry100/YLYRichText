/*
Copyright (C) 2016 yly(cantry100@163.com) - All Rights Reserved
YLY富文本解析器
author：雨Lu尧
blog：http://www.hiwrz.com
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;

class YlyRichTextParser
{
    //0: normal char
    //10000 ~ 99999 rich text, 分高三位与低两位，高三位用来标记富文本类型，低二位标记控制块长度;
    //高三位如下;
    //100-101: <emote=001>类型; #000-#999类型;          //表情字符;
	//102: <res=path>类型;                              //用路径表示的png图片;
	//103: <icon=assetName>类型;                        //定制图标，如银两、经验、元宝等图标;
	//200: <color=ffffffff>类型;        				//颜色代码;
	//200: <color=ffffffffffffffff>                     //颜色渐变：由第一个参数变到第二参数;如00ff00ffff0000ff表示由00ff00ff渐变到ff0000ff;
	//201: <url=click_arg>类型;                         //超链接; click_arg参数：字符串
	//202: <u>类型;                                     //下划线;
	//203: </url>类型;                                  //超链接终止符;
	//204: </color>类型;                                //颜色终止符;
	//205: <r>类型;                                     //回车;
	//206: </u>类型;                                    //下划线终止符;
	//207: <b>类型;                                     //加粗;
	//208: </b>类型;                                    //加粗终止符;
	//209: <i>类型;                                     //斜体;
	//210: </i>类型;                                    //斜体终止符;
	//211: <size=30>类型;                               //字体大小;
	//212: </size>类型;                                 //字体大小终止符;
	//213: <d>类型;                                     //删除线;
	//214: </d>类型;                                    //删除线终止符;
	//215: </d>类型;                                    //删除线终止符;

    public const int RICHTEXT_MULL_BASE = 100;
    public const int RICHTEXT_EMOTE_BASE = 100 * RICHTEXT_MULL_BASE;
    public const int RICHTEXT_ANIM_EMOTE_BASE = 101 * RICHTEXT_MULL_BASE;
    public const int RICHTEXT_RES_PATH_BASE = 102 * RICHTEXT_MULL_BASE;
	public const int RICHTEXT_CICON_BASE = 103 * RICHTEXT_MULL_BASE;
    public const int RICHTEXT_COLOR_BASE = 200 * RICHTEXT_MULL_BASE;
    public const int RICHTEXT_LINK_BASE = 201 * RICHTEXT_MULL_BASE;
    public const int RICHTEXT_UNDER_LINE_BASE = 202 * RICHTEXT_MULL_BASE;
    public const int RICHTEXT_LINK_END = 203 * RICHTEXT_MULL_BASE;
	public const int RICHTEXT_COLOR_END = 204 * RICHTEXT_MULL_BASE;
    public const int RICHTEXT_ENTER = 205 * RICHTEXT_MULL_BASE;
	public const int RICHTEXT_UNDER_LINE_END = 206 * RICHTEXT_MULL_BASE;
	public const int RICHTEXT_BOLD_BASE = 207 * RICHTEXT_MULL_BASE;
	public const int RICHTEXT_BOLD_END = 208 * RICHTEXT_MULL_BASE;
	public const int RICHTEXT_ITALIC_BASE = 209 * RICHTEXT_MULL_BASE;
	public const int RICHTEXT_ITALIC_END = 210 * RICHTEXT_MULL_BASE;
	public const int RICHTEXT_SIZE_BASE = 211 * RICHTEXT_MULL_BASE;
	public const int RICHTEXT_SIZE_END = 212 * RICHTEXT_MULL_BASE;
	public const int RICHTEXT_DELETE_LINE_BASE = 213 * RICHTEXT_MULL_BASE;
	public const int RICHTEXT_DELETE_LINE_END = 214 * RICHTEXT_MULL_BASE;

    public const int RICHTEXT_UNKNOWN = -9999;
    public const int FIXED_LEN_COLOR_BLOCK = 16; //<color=ffffffff>;
	public const int FIXED_LEN_COLOR_MORPH_BLOCK = 24; //<color=ffffffffffffffff>;

	//静态表情id范围：001 ~ 499，可根据实际情况修改(static emote id range, can be modified according to actual condition)
	public const int EMOTE_IDX_BEGIN = 1;
	public const int EMOTE_IDX_END = 499;
	//动态表情id范围：500 ~ 999，可根据实际情况修改(dynamic emote id range, can be modified according to actual condition)
    public const int ANIM_EMOTE_IDX_BEGIN = 500;
    public const int ANIM_EMOTE_IDX_END = 999;

	static int[] _mask;
	static Dictionary<int, string> _parms;
	static Dictionary<string, int> _preLoadAsset;
	static StringBuilder _logStr = new StringBuilder();
    public static int GetEmoteType(int idx)
    {
        if (idx >= ANIM_EMOTE_IDX_BEGIN && idx <= ANIM_EMOTE_IDX_END)
            return RICHTEXT_ANIM_EMOTE_BASE;
        else if (idx >= EMOTE_IDX_BEGIN && idx <= EMOTE_IDX_END)
            return RICHTEXT_EMOTE_BASE;
        else
            return RICHTEXT_UNKNOWN;
    }

	public static string Parse(string str, out int[] mask, out Dictionary<int, string> param, out Dictionary<string, int> preLoadAsset, bool enableRichText)
    {
        Stopwatch st = new Stopwatch();
        st.Start();

		str = str + "\n"; //在最后面加多一个换行符，便于parsedText解析成lineData

		if (enableRichText) {
			//<r>替换为正常的回车;
			str = str.Replace ("<r>", "\n");
			str = Regex.Replace(str, @"<color\W+#(\w+)>([\s\S]*?)</color>", "#C($1)$2#C()"); //兼容原生的<color=#xxxxxxxx></color>占位符
			_mask = new int[str.Length];
			//_logStr.Remove (0, _logStr.Length);
			//_logStr.Append ("===============YlyRichTextParser：分析串: \n" + str + "\n");

			_parms = new Dictionary<int, string> ();
			_preLoadAsset = new Dictionary<string, int> ();

			string emoteRgx = @"<emote=(\d{3})>"; //组0匹配形如<emote=002>，组1匹配002;
			string cIconRgx = @"<icon=(.+?)>"; //组0匹配形如<icon=gold>，组1匹配gold;
			string resPathRgx = @"<res=(.+?\.png)>"; //组0匹配形如<res=Assets/UI/Atlas/666.png>，组1匹配Assets/UI/Atlas/666.png;
			string colorRgx = @"<color=([0-9a-fA-F]{8}|[0-9a-fA-F]{16})>"; //组0匹配<color=FFFFFFFF>或<color=FFFFFFFFFFFFFFFF>,组1匹配FFFFFFFF或FFFFFFFFFFFFFFFF;
			string linkRgx = @"<url=(.+?)>";    //组0匹配<url=123>,组1匹配123;
			string underLineRgx = @"<u>";     //组0匹配<u>，组1匹配空串;
			string linkEndRgx = @"</url>";       //组0匹配</url>，组1匹配空串;
			string colorEndRgx = @"</color>";     //组0匹配</color>，组1匹配空串;
			string underLineEndRgx = @"</u>";     //组0匹配</u>，组1匹配空串;
			string boldRgx = @"<b>";     //组0匹配<b>，组1匹配空串;
			string boldEndRgx = @"</b>";     //组0匹配</b>，组1匹配空串;
			string italicRgx = @"<i>";     //组0匹配<i>，组1匹配空串;
			string italicEndRgx = @"</i>";     //组0匹配</i>，组1匹配空串;
			string sizeRgx = @"<size=(\d+)>";     //组0匹配形如<size=30>，组1匹配30;
			string sizeEndRgx = @"</size>";     //组0匹配</size>，组1匹配空串;
			string deleteLineRgx = @"<d>";     //组0匹配<d>，组1匹配空串;
			string deleteLineEndRgx = @"</d>";     //组0匹配</d>，组1匹配空串;

			ParseSingle (str, emoteRgx, RICHTEXT_EMOTE_BASE);
			ParseSingle (str, cIconRgx, RICHTEXT_CICON_BASE);
			ParseSingle (str, resPathRgx, RICHTEXT_RES_PATH_BASE);
			ParseSingle (str, colorRgx, RICHTEXT_COLOR_BASE);
			ParseSingle (str, linkRgx, RICHTEXT_LINK_BASE);
			ParseSingle (str, underLineRgx, RICHTEXT_UNDER_LINE_BASE);
			ParseSingle (str, linkEndRgx, RICHTEXT_LINK_END);
			ParseSingle (str, colorEndRgx, RICHTEXT_COLOR_END);
			ParseSingle (str, underLineEndRgx, RICHTEXT_UNDER_LINE_END);
			ParseSingle (str, boldRgx, RICHTEXT_BOLD_BASE);
			ParseSingle (str, boldEndRgx, RICHTEXT_BOLD_END);
			ParseSingle (str, italicRgx, RICHTEXT_ITALIC_BASE);
			ParseSingle (str, italicEndRgx, RICHTEXT_ITALIC_END);
			ParseSingle (str, sizeRgx, RICHTEXT_SIZE_BASE);
			ParseSingle (str, sizeEndRgx, RICHTEXT_SIZE_END);
			ParseSingle (str, deleteLineRgx, RICHTEXT_DELETE_LINE_BASE);
			ParseSingle (str, deleteLineEndRgx, RICHTEXT_DELETE_LINE_END);
		} else {
			_mask = new int[str.Length];
			//_logStr.Remove (0, _logStr.Length);
			//_logStr.Append("===============YlyRichTextParser：Parse text 分析串: \n" + str + "\n");

			_parms = new Dictionary<int, string>();
			_preLoadAsset = new Dictionary<string, int>();
		}

        st.Stop();
		//_logStr.Append("\nParse text use time 用时：" + st.Elapsed);
        //Console.WriteLine(_logStr);
        mask = _mask;
        param = _parms;
		preLoadAsset = _preLoadAsset;

		_mask = null;
		_parms = null;
		_preLoadAsset = null;

        return str;
    }

	static void ParseSingle(string str, string rex, int ibase)
    {
		string arg;
        foreach (Match mtch in Regex.Matches(str, rex))
        {
			arg = mtch.Groups[mtch.Groups.Count - 1].Value;
            _mask[mtch.Index] = ibase + mtch.Length;
			if(ibase == YlyRichTextParser.RICHTEXT_EMOTE_BASE){//表情
				int eidx = -1;
				if(int.TryParse(arg, out eidx))
				{
					if (GetEmoteType (eidx) == YlyRichTextParser.RICHTEXT_ANIM_EMOTE_BASE) {
						//动态表情;
						string assetPath = string.Format(YlyRichText.animEmotePrefabPathFormat, arg);
						if(!_preLoadAsset.ContainsKey(assetPath)){
							_preLoadAsset.Add(assetPath, YlyRichTextParser.RICHTEXT_ANIM_EMOTE_BASE);
						}
					} else {
						string assetPath = string.Format(YlyRichText.emotePngPathFormat, arg);
						if(!_preLoadAsset.ContainsKey(assetPath)){
							_preLoadAsset.Add(assetPath, YlyRichTextParser.RICHTEXT_EMOTE_BASE);
						}
					}
				}
			} else if(ibase == YlyRichTextParser.RICHTEXT_RES_PATH_BASE){//图片
				string assetPath = arg;
				if(!_preLoadAsset.ContainsKey(assetPath)){
					_preLoadAsset.Add(assetPath, YlyRichTextParser.RICHTEXT_RES_PATH_BASE);
				}
			} else if(ibase == YlyRichTextParser.RICHTEXT_CICON_BASE){//定制图标
				string assetPath = string.Format(YlyRichText.cIconPngPathFormat, arg);
				if(!_preLoadAsset.ContainsKey(assetPath)){
					_preLoadAsset.Add(assetPath, YlyRichTextParser.RICHTEXT_CICON_BASE);
				}
			}
			_parms[mtch.Index] = arg;
			//_logStr.Append("type=" + (ibase / YlyRichTextParser.RICHTEXT_MULL_BASE) + ", str=" + mtch.Value + ", arg=" + _parms[mtch.Index] + ", idx= " + mtch.Index + ", len= " + mtch.Length + "\n");
        }
    }

	//是否是标点符号
	public static bool IsPunctuation(char str)
	{
		return Regex.IsMatch(str.ToString(), @"[\[【】\]~!！@#,，\.。？\?]");
	}

	public static string GetLog()
    {
		//return _logStr.ToString();
		return "为了避免不必要的性能消耗，富文本的log被屏蔽了=、=";
    }
}