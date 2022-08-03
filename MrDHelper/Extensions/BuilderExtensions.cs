/*
 * https://github.com/EdCharbeneau/BlazorComponentUtilities/blob/master/BlazorComponentUtilities/CssBuilder.cs
 *  The MIT License (MIT)
 * 
 *  Copyright (c) 2011-2019 Ed Charbeneau
 *  Copyright (c) 2011-2019 Ed Charbeneau 
 * 
 */
namespace MrDHelper
{
#pragma warning disable CS8603 // Possible null reference return.
    public static class BuilderExtensions
    {
        /// <summary>
        /// Used to convert a CssBuilder into a null when it is empty.
        /// Usage: class=null causes the attribute to be excluded when rendered.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>string</returns>
        public static string NullIfEmpty(this ConnectionStringBuilder builder) =>
            builder.ToString().IsNullOrEmpty() ? null : builder.ToString();


        /// <summary>
        /// Used to convert a string.IsNullOrEmpty into a null when it is empty.
        /// Usage: attribute=null causes the attribute to be excluded when rendered.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>string</returns>
        public static string NullIfEmpty(this string s) =>
            s.IsNullOrEmpty() ? null : s;

    }
#pragma warning restore CS8603 // Possible null reference return.
}