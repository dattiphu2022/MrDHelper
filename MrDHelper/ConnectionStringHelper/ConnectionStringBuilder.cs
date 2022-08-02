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
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    [ExcludeFromCodeCoverage]
    public class ConnectionStringBuilder
    {
        private readonly StringBuilder stringBuffer;

        /// <summary>
        /// Creates an Empty ConnectionStringBuilder used to define conditional connectionstring classes used in a component.
        /// Call Build() to return the completed connectionstring Classes as a string. 
        /// </summary>
        public static ConnectionStringBuilder Empty() => new ConnectionStringBuilder();

        /// <summary>
        /// Creates a ConnectionStringBuilder used to define conditional connectionstring classes used in a component.
        /// Call Build() to return the completed connectionstring Classes as a string. 
        /// </summary>
        /// <param name="value"></param>
        public ConnectionStringBuilder() => stringBuffer = new StringBuilder();

        /// <summary>
        /// Adds a raw string to the builder that will be concatenated with the next value added to the builder.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>ConnectionStringBuilder</returns>
        private ConnectionStringBuilder AddValue(string value)
        {
            stringBuffer.Append(value);
            return this;
        }

        /// <summary>
        /// Adds a connectionstring property to the builder with comma separator.
        /// </summary>
        /// <param name="value">connectionstring property to add</param>
        /// <returns>ConnectionStringBuilder</returns>
        public ConnectionStringBuilder AddProperty(string value) 
        {
            if (value.Length - 1 > 0 && value[value.Length - 1] == ';')
            {
                value = value.Remove(value.Length - 1);
            }
            return AddValue($"{value};"); 
        }

        /// <summary>
        /// Adds a conditional connectionstring property to the builder with comma separator.
        /// </summary>
        /// <param name="value">connectionstring property to conditionally add.</param>
        /// <param name="when">Condition in which the connectionstring property is added.</param>
        /// <returns>ConnectionStringBuilder</returns>
        public ConnectionStringBuilder AddProperty(string value, bool when = true) => when ? this.AddProperty(value) : this;

        /// <summary>
        /// Adds a conditional connectionstring property to the builder with comma separator.
        /// </summary>
        /// <param name="value">connectionstring property to conditionally add.</param>
        /// <param name="when">Condition in which the connectionstring property is added.</param>
        /// <returns>ConnectionStringBuilder</returns>
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public ConnectionStringBuilder AddProperty(string value, Func<bool> when = null) => this.AddProperty(value, when());
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        /// <summary>
        /// Adds a conditional connectionstring property to the builder with comma separator.
        /// </summary>
        /// <param name="value">Function that returns a connectionstring property to conditionally add.</param>
        /// <param name="when">Condition in which the connectionstring property is added.</param>
        /// <returns>ConnectionStringBuilder</returns>
        public ConnectionStringBuilder AddProperty(Func<string> value, bool when = true) => when ? this.AddProperty(value()) : this;

        /// <summary>
        /// Adds a conditional connectionstring property to the builder with comma separator.
        /// </summary>
        /// <param name="value">Function that returns a connectionstring property to conditionally add.</param>
        /// <param name="when">Condition in which the connectionstring property is added.</param>
        /// <returns>ConnectionStringBuilder</returns>
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public ConnectionStringBuilder AddProperty(Func<string> value, Func<bool> when = null) => this.AddProperty(value, when());
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        /// <summary>
        /// Adds a conditional nested ConnectionStringBuilder to the builder with comma separator.
        /// </summary>
        /// <param name="value">connectionstring property to conditionally add.</param>
        /// <param name="when">Condition in which the connectionstring property is added.</param>
        /// <returns>ConnectionStringBuilder</returns>
        public ConnectionStringBuilder AddProperty(ConnectionStringBuilder builder, bool when = true) => when ? this.AddProperty(builder.Build()) : this;

        /// <summary>
        /// Adds a conditional connectionstring property to the builder with comma separator.
        /// </summary>
        /// <param name="value">connectionstring property to conditionally add.</param>
        /// <param name="when">Condition in which the connectionstring property is added.</param>
        /// <returns>ConnectionStringBuilder</returns>
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public ConnectionStringBuilder AddProperty(ConnectionStringBuilder builder, Func<bool> when = null) => this.AddProperty(builder, when());
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        /// <summary>
        /// Finalize the completed connectionstring Classes as a string.
        /// </summary>
        /// <returns>string</returns>
        public string Build()
        {
            // String buffer finalization code
            return stringBuffer != null ? stringBuffer.ToString().Trim() : string.Empty;
        }

        // ToString should only and always call Build to finalize the rendered string.
        public override string ToString() => Build();

    }
}