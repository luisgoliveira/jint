﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint.Native.Array;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Native.RegExp;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace Jint.Native.String
{


    /// <summary>
    /// http://www.ecma-international.org/ecma-262/5.1/#sec-15.5.4
    /// </summary>
    public sealed class StringPrototype : StringInstance
    {
        private StringPrototype(Engine engine)
            : base(engine)
        {
        }

        public static StringPrototype CreatePrototypeObject(Engine engine, StringConstructor stringConstructor)
        {
            var obj = new StringPrototype(engine);
            obj.Prototype = engine.Object.PrototypeObject;
            obj.PrimitiveValue = "";
            obj.Extensible = true;
            obj.FastAddProperty("length", 0, false, false, false); 
            obj.FastAddProperty("constructor", stringConstructor, true, false, true);

            return obj;
        }

        public void Configure()
        {
            FastAddProperty("toString", new ClrFunctionInstance(Engine, ToStringString), true, false, true);
            FastAddProperty("valueOf", new ClrFunctionInstance(Engine, ValueOf), true, false, true);
            FastAddProperty("charAt", new ClrFunctionInstance(Engine, CharAt, 1), true, false, true);
            FastAddProperty("charCodeAt", new ClrFunctionInstance(Engine, CharCodeAt, 1), true, false, true);
            FastAddProperty("concat", new ClrFunctionInstance(Engine, Concat, 1), true, false, true);
            FastAddProperty("indexOf", new ClrFunctionInstance(Engine, IndexOf, 1), true, false, true);
            FastAddProperty("lastIndexOf", new ClrFunctionInstance(Engine, LastIndexOf, 1), true, false, true);
            FastAddProperty("localeCompare", new ClrFunctionInstance(Engine, LocaleCompare, 1), true, false, true);
            FastAddProperty("match", new ClrFunctionInstance(Engine, Match, 1), true, false, true);
            FastAddProperty("replace", new ClrFunctionInstance(Engine, Replace, 2), true, false, true);
            FastAddProperty("search", new ClrFunctionInstance(Engine, Search, 1), true, false, true);
            FastAddProperty("slice", new ClrFunctionInstance(Engine, Slice, 2), true, false, true);
            FastAddProperty("split", new ClrFunctionInstance(Engine, Split, 2), true, false, true);
            FastAddProperty("substr", new ClrFunctionInstance(Engine, Substr, 2), true, false, true);
            FastAddProperty("substring", new ClrFunctionInstance(Engine, Substring, 2), true, false, true);
            FastAddProperty("toLowerCase", new ClrFunctionInstance(Engine, ToLowerCase), true, false, true);
            FastAddProperty("toLocaleLowerCase", new ClrFunctionInstance(Engine, ToLocaleLowerCase), true, false, true);
            FastAddProperty("toUpperCase", new ClrFunctionInstance(Engine, ToUpperCase), true, false, true);
            FastAddProperty("toLocaleUpperCase", new ClrFunctionInstance(Engine, ToLocaleUpperCase), true, false, true);
            FastAddProperty("trim", new ClrFunctionInstance(Engine, Trim), true, false, true);
        }

        private JsValue ToStringString(JsValue thisObj, JsValue[] arguments)
        {
            var s = TypeConverter.ToObject(Engine, thisObj) as StringInstance;
            if (s == null)
            {
                throw new JavaScriptException(Engine.TypeError);
            }

            return s.PrimitiveValue;
        }

        // http://msdn.microsoft.com/en-us/library/system.char.iswhitespace(v=vs.110).aspx
        // http://en.wikipedia.org/wiki/Byte_order_mark
        const char BOM_CHAR = '\uFEFF';

        private static bool IsWhiteSpaceEx(char c)
        {
            return char.IsWhiteSpace(c) || c == BOM_CHAR;
        }

        private static string TrimEndEx(string s)
        {
            if (s.Length == 0)
                return string.Empty;

            var i = s.Length - 1;
            while (i >= 0)
            {
                if (IsWhiteSpaceEx(s[i]))
                    i--;
                else
                    break;
            }
            if (i >= 0)
                return s.Substring(0, i + 1);
            else
                return string.Empty;
        }

        private static string TrimStartEx(string s)
        {
            if (s.Length == 0)
                return string.Empty;

            var i = 0;
            while (i < s.Length)
            {
                if (IsWhiteSpaceEx(s[i]))
                    i++;
                else
                    break;
            }
            if (i >= s.Length)
                return string.Empty;
            else
                return s.Substring(i);
        }

        private static string TrimEx(string s)
        {
            return TrimEndEx(TrimStartEx(s));
        } 

        private JsValue Trim(JsValue thisObj, JsValue[] arguments)
        {
            TypeConverter.CheckObjectCoercible(Engine, thisObj);
            var s = TypeConverter.ToString(thisObj);
            return TrimEx(s);
        }
        
        private static JsValue ToLocaleUpperCase(JsValue thisObj, JsValue[] arguments)
        {
            var s = TypeConverter.ToString(thisObj);
            return s.ToUpper();
        }

        private static JsValue ToUpperCase(JsValue thisObj, JsValue[] arguments)
        {
            var s = TypeConverter.ToString(thisObj);
            return s.ToUpperInvariant();
        }

        private static JsValue ToLocaleLowerCase(JsValue thisObj, JsValue[] arguments)
        {
            var s = TypeConverter.ToString(thisObj);
            return s.ToLower();
        }

        private static JsValue ToLowerCase(JsValue thisObj, JsValue[] arguments)
        {
            var s = TypeConverter.ToString(thisObj);
            return s.ToLowerInvariant();
        }

        private static int ToIntegerSupportInfinity(JsValue numberVal)
        {
            var doubleVal = TypeConverter.ToInteger(numberVal);
            var intVal = (int) doubleVal;
            if (double.IsPositiveInfinity(doubleVal))
                intVal = int.MaxValue;
            else if (double.IsNegativeInfinity(doubleVal))
                intVal = int.MinValue;
            else
                intVal = (int) doubleVal;
            return intVal;
        }

        private JsValue Substring(JsValue thisObj, JsValue[] arguments)
        {
            TypeConverter.CheckObjectCoercible(Engine, thisObj);

            var s = TypeConverter.ToString(thisObj);
            var start = TypeConverter.ToNumber(arguments.At(0));
            var end = TypeConverter.ToNumber(arguments.At(1));

            if (double.IsNaN(start) || start < 0)
            {
                start = 0;
            }

            if (double.IsNaN(end) || end < 0)
            {
                end = 0;
            }

            var len = s.Length;
            var intStart = ToIntegerSupportInfinity(start);

            var intEnd = arguments.At(1) == Undefined.Instance ? len : (int)ToIntegerSupportInfinity(end);
            var finalStart = System.Math.Min(len, System.Math.Max(intStart, 0));
            var finalEnd = System.Math.Min(len, System.Math.Max(intEnd, 0));
            // Swap value if finalStart < finalEnd
            var from = System.Math.Min(finalStart, finalEnd);
            var to = System.Math.Max(finalStart, finalEnd);
            return s.Substring(from, to - from);
        }

        private JsValue Substr(JsValue thisObj, JsValue[] arguments)
        {
            var s = TypeConverter.ToString(thisObj);
            var start = TypeConverter.ToInteger(arguments.At(0));
            var length = arguments.At(1) == JsValue.Undefined 
                ? double.PositiveInfinity 
                : TypeConverter.ToInteger(arguments.At(1));

            start = start >= 0 ? start : System.Math.Max(s.Length + start, 0);
            length = System.Math.Min(System.Math.Max(length, 0), s.Length - start);
            if (length <= 0)
            {
                return "";
            }

            return s.Substring(TypeConverter.ToInt32(start), TypeConverter.ToInt32(length));
        }

        private JsValue Split(JsValue thisObj, JsValue[] arguments)
        {
            TypeConverter.CheckObjectCoercible(Engine, thisObj);
            var s = TypeConverter.ToString(thisObj);

            var separator = arguments.At(0);

            // Coerce into a number, true will become 1 
            var l = arguments.At(1);
            var a = (ArrayInstance) Engine.Array.Construct(Arguments.Empty);
            var limit = l == Undefined.Instance ? UInt32.MaxValue : TypeConverter.ToUint32(l);
            var len = s.Length;
            
            if (limit == 0)
            {
                return a;
            }

            if (separator == Null.Instance)
            {
                separator = Null.Text;
            }
            else if (separator == Undefined.Instance)
            {
                return (ArrayInstance)Engine.Array.Construct(Arguments.From(s));
            }
            else
            {
                if (!separator.IsRegExp())
                {
                    separator = TypeConverter.ToString(separator); // Coerce into a string, for an object call toString()
                }
            }

            var rx = TypeConverter.ToObject(Engine, separator) as RegExpInstance;

            const string regExpForMatchingAllCharactere = "(?:)";

            if (rx != null &&
                rx.Source != regExpForMatchingAllCharactere // We need pattern to be defined -> for s.split(new RegExp) 
                )
            {
                var match = rx.Value.Match(s, 0);

                if (!match.Success) // No match at all return the string in an array
                {
                    a.DefineOwnProperty("0", new PropertyDescriptor(s, true, true, true), false);
                    return a;
                }

                int lastIndex = 0;
                int index = 0;
                while (match.Success && index < limit)
                {
                    if (match.Length == 0 && (match.Index == 0 || match.Index == len || match.Index == lastIndex))
                    {
                        match = match.NextMatch();
                        continue;
                    }

                    // Add the match results to the array.
                    a.DefineOwnProperty(index++.ToString(), new PropertyDescriptor(s.Substring(lastIndex, match.Index - lastIndex), true, true, true), false);
                    
                    if (index >= limit)
                    {
                        return a;
                    }

                    lastIndex = match.Index + match.Length;
                    for (int i = 1; i < match.Groups.Count; i++)
                    {
                        var group = match.Groups[i];
                        var item = Undefined.Instance;
                        if (group.Captures.Count > 0)
                        {
                            item = match.Groups[i].Value;
                        }

                        a.DefineOwnProperty(index++.ToString(), new PropertyDescriptor(item, true, true, true ), false);

                        if (index >= limit)
                        {
                            return a;
                        }
                    }

                    match = match.NextMatch();
                    if (!match.Success) // Add the last part of the split
                    {
                        a.DefineOwnProperty(index++.ToString(), new PropertyDescriptor(s.Substring(lastIndex), true, true, true), false);                        
                    }
                }

                return a;
            }
            else
            {
                var segments = new List<string>();
                var sep = TypeConverter.ToString(separator);

                if (sep == string.Empty || (rx != null && rx.Source == regExpForMatchingAllCharactere)) // for s.split(new RegExp)
                {
                    foreach (var c in s)
                    {
                        segments.Add(c.ToString());    
                    }
                }
                else
                {
                    segments = s.Split(new[] {sep}, StringSplitOptions.None).ToList();
                }

                for (int i = 0; i < segments.Count && i < limit; i++)
                {
                    a.DefineOwnProperty(i.ToString(), new PropertyDescriptor(segments[i], true, true, true), false);
                }
            
                return a;
            }
        }

        private JsValue Slice(JsValue thisObj, JsValue[] arguments)
        {
            TypeConverter.CheckObjectCoercible(Engine, thisObj);

            var s = TypeConverter.ToString(thisObj);

            var start = TypeConverter.ToNumber(arguments.At(0));
            if (double.NegativeInfinity.Equals(start))
            {
                start = 0;
            }
            if (double.PositiveInfinity.Equals(start))
            {
                return string.Empty;
            }
            
            var end = TypeConverter.ToNumber(arguments.At(1));
            if (double.PositiveInfinity.Equals(end))
            {
                end = s.Length;
            }

            var len = s.Length;
            var intStart = (int)TypeConverter.ToInteger(start);
            var intEnd = arguments.At(1) == Undefined.Instance ? len : (int)TypeConverter.ToInteger(end);
            var from = intStart < 0 ? System.Math.Max(len + intStart, 0) : System.Math.Min(intStart, len);
            var to = intEnd < 0 ? System.Math.Max(len + intEnd, 0) : System.Math.Min(intEnd, len);
            var span = System.Math.Max(to - from, 0);

            return s.Substring(from, span);
        }

        private JsValue Search(JsValue thisObj, JsValue[] arguments)
        {
            TypeConverter.CheckObjectCoercible(Engine, thisObj);

            var s = TypeConverter.ToString(thisObj);

            var regex = arguments.At(0);

            if (regex.IsUndefined())
            {
                regex = string.Empty;
            }
            else if (regex.IsNull())
            {
                regex = Null.Text;
            }

            var rx = TypeConverter.ToObject(Engine, regex) as RegExpInstance ?? (RegExpInstance)Engine.RegExp.Construct(new[] { regex });
            var match = rx.Value.Match(s);
            if (!match.Success)
            {
                return -1;
            }

            return match.Index;
        }

        private JsValue Replace(JsValue thisObj, JsValue[] arguments)
        {
            TypeConverter.CheckObjectCoercible(Engine, thisObj);

            var thisString = TypeConverter.ToString(thisObj);
            var searchValue = arguments.At(0);
            var replaceValue = arguments.At(1);

            // If the second parameter is not a function we create one
            var replaceFunction = replaceValue.TryCast<FunctionInstance>();
            if (replaceFunction == null)
            {
                replaceFunction = new ClrFunctionInstance(Engine, (self, args) =>
                {
                    var replaceString = TypeConverter.ToString(replaceValue);
                    var matchValue = TypeConverter.ToString(args.At(0));
                    var matchIndex = (int)TypeConverter.ToInteger(args.At(args.Length - 2));

                    // Check if the replacement string contains any patterns.
                    bool replaceTextContainsPattern = replaceString.IndexOf('$') >= 0;

                    // If there is no pattern, replace the pattern as is.
                    if (replaceTextContainsPattern == false)
                        return replaceString;

                    // Patterns
                    // $$	Inserts a "$".
                    // $&	Inserts the matched substring.
                    // $`	Inserts the portion of the string that precedes the matched substring.
                    // $'	Inserts the portion of the string that follows the matched substring.
                    // $n or $nn	Where n or nn are decimal digits, inserts the nth parenthesized submatch string, provided the first argument was a RegExp object.
                    var replacementBuilder = new StringBuilder();
                    for (int i = 0; i < replaceString.Length; i++)
                    {
                        char c = replaceString[i];
                        if (c == '$' && i < replaceString.Length - 1)
                        {
                            c = replaceString[++i];
                            if (c == '$')
                                replacementBuilder.Append('$');
                            else if (c == '&')
                                replacementBuilder.Append(matchValue);
                            else if (c == '`')
                                replacementBuilder.Append(thisString.Substring(0, matchIndex));
                            else if (c == '\'')
                                replacementBuilder.Append(thisString.Substring(matchIndex + matchValue.Length));
                            else if (c >= '0' && c <= '9')
                            {
                                int matchNumber1 = c - '0';

                                // The match number can be one or two digits long.
                                int matchNumber2 = 0;
                                if (i < replaceString.Length - 1 && replaceString[i + 1] >= '0' && replaceString[i + 1] <= '9')
                                    matchNumber2 = matchNumber1 * 10 + (replaceString[i + 1] - '0');

                                // Try the two digit capture first.
                                if (matchNumber2 > 0 && matchNumber2 < args.Length - 2)
                                {
                                    // Two digit capture replacement.
                                    replacementBuilder.Append(TypeConverter.ToString(args[matchNumber2]));
                                    i++;
                                }
                                else if (matchNumber1 > 0 && matchNumber1 < args.Length - 2)
                                {
                                    // Single digit capture replacement.
                                    replacementBuilder.Append(TypeConverter.ToString(args[matchNumber1]));
                                }
                                else
                                {
                                    // Capture does not exist.
                                    replacementBuilder.Append('$');
                                    i--;
                                }
                            }
                            else
                            {
                                // Unknown replacement pattern.
                                replacementBuilder.Append('$');
                                replacementBuilder.Append(c);
                            }
                        }
                        else
                            replacementBuilder.Append(c);
                    }

                    return replacementBuilder.ToString();
                });
            }

            // searchValue is a regular expression

            if (searchValue.IsNull()) 
            {
                searchValue = new JsValue(Null.Text);
            }
            if (searchValue.IsUndefined())
            {
                searchValue = new JsValue(Undefined.Text);
            }
            
            var rx = TypeConverter.ToObject(Engine, searchValue) as RegExpInstance;
            if (rx != null)
            {
                // Replace the input string with replaceText, recording the last match found.
                string result = rx.Value.Replace(thisString, match =>
                {
                    var args = new List<JsValue>();
                    
                    for (var k = 0; k < match.Groups.Count; k++)
                    {
                        var group = match.Groups[k];
                        if (group.Success)
                            args.Add(group.Value);
                    }
                    
                    args.Add(match.Index);
                    args.Add(thisString);

                    var v = TypeConverter.ToString(replaceFunction.Call(Undefined.Instance, args.ToArray()));
                    return v;
                }, rx.Global == true ? -1 : 1);

                // Set the deprecated RegExp properties if at least one match was found.
                //if (lastMatch != null)
                //    this.Engine.RegExp.SetDeprecatedProperties(input, lastMatch);

                return result;
            }

            // searchValue is a string
            else
            {
                var substr = TypeConverter.ToString(searchValue);

                // Find the first occurrance of substr.
                int start = thisString.IndexOf(substr, StringComparison.Ordinal);
                if (start == -1)
                    return thisString;
                int end = start + substr.Length;

                var args = new List<JsValue>();
                args.Add(substr);
                args.Add(start);
                args.Add(thisString);

                var replaceString = TypeConverter.ToString(replaceFunction.Call(Undefined.Instance, args.ToArray()));

                // Replace only the first match.
                var result = new StringBuilder(thisString.Length + (substr.Length - substr.Length));
                result.Append(thisString, 0, start);
                result.Append(replaceString);
                result.Append(thisString, end, thisString.Length - end);
                return result.ToString();
            }
        }

        private JsValue Match(JsValue thisObj, JsValue[] arguments)
        {
            TypeConverter.CheckObjectCoercible(Engine, thisObj);

            var s = TypeConverter.ToString(thisObj);

            var regex = arguments.At(0);
            var rx = regex.TryCast<RegExpInstance>();

            rx = rx ?? (RegExpInstance) Engine.RegExp.Construct(new[] {regex});

            var global = rx.Get("global").AsBoolean();
            if (!global)
            {
                return Engine.RegExp.PrototypeObject.Exec(rx, Arguments.From(s));
            }
            else
            {
                rx.Put("lastIndex", 0, false);
                var a = Engine.Array.Construct(Arguments.Empty);
                double previousLastIndex = 0;
                var n = 0;
                var lastMatch = true;
                while (lastMatch)
                {
                    var result = Engine.RegExp.PrototypeObject.Exec(rx, Arguments.From(s)).TryCast<ObjectInstance>();
                    if (result == null)
                    {
                        lastMatch = false;
                    }
                    else
                    {
                        var thisIndex = rx.Get("lastIndex").AsNumber();
                        if (thisIndex == previousLastIndex)
                        {
                            rx.Put("lastIndex", thisIndex + 1, false);
                            previousLastIndex = thisIndex;
                        }

                        var matchStr = result.Get("0");
                        a.DefineOwnProperty(TypeConverter.ToString(n), new PropertyDescriptor(matchStr, true, true, true), false);
                        n++;
                    }
                }
                if (n == 0)
                {
                    return Null.Instance;
                }
                return a;
            }

        }

        private JsValue LocaleCompare(JsValue thisObj, JsValue[] arguments)
        {
            TypeConverter.CheckObjectCoercible(Engine, thisObj);

            var s = TypeConverter.ToString(thisObj);
            var that = TypeConverter.ToString(arguments.At(0));
            
            return string.CompareOrdinal(s, that);
        }

        private static List<int> AllIndexesOf(string str, string value)
        {
            if (string.IsNullOrEmpty(value))
                return new List<int>();

            var indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1) // no more fond
                    return indexes;
                indexes.Add(index);
            }
        }

        private int LastIndexJavaScriptImplementation(string s, string searchStr, int pos = -1)
        {
            if (pos == -1)
                pos = s.Length;

            var len          = s.Length;
            var start        = System.Math.Min(System.Math.Max(pos, 0), len);
            var searchLen    = searchStr.Length;
            var kPositions   = AllIndexesOf(s, searchStr);

            if (kPositions.Count == 0) // Nothing found
            {
                return -1;
            }
            else if (kPositions.Count == 1) // Only one found
            {
                return kPositions[0] <= start ? kPositions[0] : -1;
            }

            // Return the largest possible nonnegative integer k not larger than start 
            // such that k+ searchLen is not greater than len
            for (var i = 0; i < kPositions.Count; i++)
            {
                if (kPositions[i] <= start)
                {
                    // ok move to the next one to find a greater pos
                }
                else
                {
                    if ((i > 0) && ((kPositions[i - 1] + searchLen) <= len))
                        return kPositions[i - 1];
                    else
                        return -1;
                }
            }
            return kPositions[kPositions.Count - 1];
        }

        private JsValue LastIndexOf(JsValue thisObj, JsValue[] arguments)
        {
            TypeConverter.CheckObjectCoercible(Engine, thisObj);

            var s = TypeConverter.ToString(thisObj);
            var searchStr = TypeConverter.ToString(arguments.At(0));
            double numPos = arguments.At(1) == Undefined.Instance ? s.Length : TypeConverter.ToNumber(arguments.At(1));
            double pos = double.IsNaN(numPos) ? double.PositiveInfinity : TypeConverter.ToInteger(numPos);
            var len = s.Length;
            var start = System.Math.Min(len, System.Math.Max(pos, 0));

            // The JavaScript spec of string.lastIndexOf does match the C# spec
            // Therefore we need to write our own specific implementation.
            // Enjoy the fact that Ecma spec and Mozilla spec have different definition which
            // I guess mean the same thing.
            // Ecma spec
            // http://www.ecma-international.org/ecma-262/5.1/#sec-15.5.4.8
            // Mozilla spec
            // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/lastIndexOf

            return LastIndexJavaScriptImplementation(s, searchStr, (int)start);
        }

        private JsValue IndexOf(JsValue thisObj, JsValue[] arguments)
        {
            TypeConverter.CheckObjectCoercible(Engine, thisObj);

            var s = TypeConverter.ToString(thisObj);
            var searchStr = TypeConverter.ToString(arguments.At(0));
            double pos = 0;
            if (arguments.Length > 1 && arguments[1] != Undefined.Instance)
            {
                pos = TypeConverter.ToInteger(arguments[1]);
            }

            if (pos >= s.Length)
            {
                return -1;
            }

            if (pos < 0)
            {
                pos = 0;
            }

            return s.IndexOf(searchStr, (int) pos, StringComparison.Ordinal);
        }

        private JsValue Concat(JsValue thisObj, JsValue[] arguments)
        {
            TypeConverter.CheckObjectCoercible(Engine, thisObj);

            var s = TypeConverter.ToString(thisObj);
            var sb = new StringBuilder(s);
            for (int i = 0; i < arguments.Length; i++)
            {
                sb.Append(TypeConverter.ToString(arguments[i]));
            }

            return sb.ToString();
        }

        private JsValue CharCodeAt(JsValue thisObj, JsValue[] arguments)
        {
            TypeConverter.CheckObjectCoercible(Engine, thisObj);

            JsValue pos = arguments.Length > 0 ? arguments[0] : 0;
            var s = TypeConverter.ToString(thisObj);
            var position = (int)TypeConverter.ToInteger(pos);
            if (position < 0 || position >= s.Length)
            {
                return double.NaN;
            }
            return s[position];
        }

        private JsValue CharAt(JsValue thisObj, JsValue[] arguments)
        {
            TypeConverter.CheckObjectCoercible(Engine, thisObj);
            var s = TypeConverter.ToString(thisObj);
            var position = TypeConverter.ToInteger(arguments.At(0));
            var size = s.Length;
            if (position >= size || position < 0)
            {
                return "";
            }
            return s[(int) position].ToString();

        }

        private JsValue ValueOf(JsValue thisObj, JsValue[] arguments)
        {
            var s = thisObj.TryCast<StringInstance>();
            if (s == null)
            {
                throw new JavaScriptException(Engine.TypeError);
            }

            return s.PrimitiveValue;
        }
    }
}
