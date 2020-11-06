using EfCoreScaffoldMssql.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EfCoreScaffoldMssql.Helpers
{
    public static class StringHelper
    {
        public static string ReplaceFirstOccurrance(this string original, string oldValue, string newValue, bool isPresetValue = false)
        {
            if (String.IsNullOrEmpty(original))
                return String.Empty;
            if (String.IsNullOrEmpty(oldValue))
                return original;
            if (String.IsNullOrEmpty(newValue))
                newValue = String.Empty;
            if (isPresetValue)
                return string.Format("{0}{1}", original, newValue);
            int loc = original.IndexOf(oldValue, StringComparison.Ordinal);
            if (loc == -1)
                return original;

            return original.Remove(loc, oldValue.Length).Insert(loc, newValue);
        }

        public static string UpperCaseFirstChar(this string value)
        {
            return value.Substring(0, 1).ToUpper() + value.Substring(1);
        }

        public static string LowerCaseFirstChar(this string value)
        {
            return value.Substring(0, 1).ToLower() + value.Substring(1);
        }

        /// <summary>
        /// Attempts to pluralize the specified text according to the rules of the English language.
        /// </summary>
        /// <remarks>
        /// This function attempts to pluralize as many words as practical by following these rules:
        /// <list type="bullet">
        ///		<item><description>Words that don't follow any rules (e.g. "mouse" becomes "mice") are returned from a dictionary.</description></item>
        ///		<item><description>Words that end with "y" (but not with a vowel preceding the y) are pluralized by replacing the "y" with "ies".</description></item>
        ///		<item><description>Words that end with "us", "ss", "x", "ch" or "sh" are pluralized by adding "es" to the end of the text.</description></item>
        ///		<item><description>Words that end with "f" or "fe" are pluralized by replacing the "f(e)" with "ves".</description></item>
        ///	</list>
        /// </remarks>
        /// <param name="text">The text to pluralize.</param>
        /// <param name="number">If number is 1, the text is not pluralized; otherwise, the text is pluralized.</param>
        /// <returns>A string that consists of the text in its pluralized form.</returns>
        public static string Pluralize(string text, int number = 2)
        {
            if (number == 1)
            {
                return text;
            }
            else
            {
                // Create a dictionary of exceptions that have to be checked first
                // This is very much not an exhaustive list!
                var exceptions = new Dictionary<string, string>()
                {
                    {"man", "men"},
                    {"woman", "women"},
                    {"child", "children"},
                    {"tooth", "teeth"},
                    {"foot", "feet"},
                    {"mouse", "mice"},
                    {"belief", "beliefs"},
                    {"staff", "staffs"}
                };

                foreach (var exception in exceptions.Keys)
                {
                    if (text.EndsWith(exception))
                    {
                        return text.Substring(0, text.Length - exception.Length) + exceptions[exception];
                    }
                    var upperCaseException = exception.UpperCaseFirstChar();

                    if (text.EndsWith(upperCaseException))
                    {
                        var result = exceptions[exception];
                        var upperCaseResult = result.UpperCaseFirstChar();

                        return text.Substring(0, text.Length - upperCaseException.Length) + upperCaseResult;
                    }
                }
                if (exceptions.ContainsKey(text.ToLowerInvariant()))
                {
                    return exceptions[text.ToLowerInvariant()];
                }
                else if (text.EndsWith("y", StringComparison.OrdinalIgnoreCase) &&
                    !text.EndsWith("ay", StringComparison.OrdinalIgnoreCase) &&
                    !text.EndsWith("ey", StringComparison.OrdinalIgnoreCase) &&
                    !text.EndsWith("iy", StringComparison.OrdinalIgnoreCase) &&
                    !text.EndsWith("oy", StringComparison.OrdinalIgnoreCase) &&
                    !text.EndsWith("uy", StringComparison.OrdinalIgnoreCase) &&
                    !text.EndsWith("By", StringComparison.OrdinalIgnoreCase))
                {
                    return text.Substring(0, text.Length - 1) + "ies";
                }
                else if (text.EndsWith("us", StringComparison.InvariantCultureIgnoreCase))
                {
                    // http://en.wikipedia.org/wiki/Plural_form_of_words_ending_in_-us
                    return text + "es";
                }
                else if (text.EndsWith("ss", StringComparison.InvariantCultureIgnoreCase))
                {
                    return text + "es";
                }
                else if (text.EndsWith("s", StringComparison.InvariantCultureIgnoreCase))
                {
                    return text;
                }
                else if (text.EndsWith("x", StringComparison.InvariantCultureIgnoreCase) ||
                    text.EndsWith("ch", StringComparison.InvariantCultureIgnoreCase) ||
                    text.EndsWith("sh", StringComparison.InvariantCultureIgnoreCase))
                {
                    return text + "es";
                }
                else if (text.EndsWith("f", StringComparison.InvariantCultureIgnoreCase) && !text.EndsWith("Off", StringComparison.InvariantCultureIgnoreCase) && text.Length > 1)
                {
                    return text.Substring(0, text.Length - 1) + "ves";
                }
                else if (text.EndsWith("fe", StringComparison.InvariantCultureIgnoreCase) && text.Length > 2)
                {
                    return text.Substring(0, text.Length - 2) + "ves";
                }
                else
                {
                    return text + "s";
                }
            }
        }
        public static string Pluralize(string text, List<EntityPluralizeNameDefinition> pluralizeNameSettingsList, int number = 2)
        {
            if (pluralizeNameSettingsList != null)
            {
                var pluralizeNameSetting = pluralizeNameSettingsList.FirstOrDefault(x => x.EntityName == text);
                if (pluralizeNameSetting != null)
                {
                    return pluralizeNameSetting.NewEntityPluralizedName;
                }
            }

            return Pluralize(text, number);
        }
    }
}