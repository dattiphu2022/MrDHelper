namespace MrDHelper.ValueTypeHelpers.BoolHelper
{
    using System;
    using System.Diagnostics.CodeAnalysis;


    /// <summary>
    /// Bool extension methods.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class BoolHelper
    {
        #region Method start with IS
        /// <summary>
        /// Short way of "bool? == false"
        /// </summary>
        /// <param name="input">bool to check</param>
        /// <returns><see cref="Boolean"/></returns>
        public static bool IsFalse(this bool? input)
        {
            return input == false;
        }
        /// <summary>
        /// Short way of "bool == false"
        /// </summary>
        /// <param name="input">bool to check</param>
        /// <returns><see cref="Boolean"/></returns>
        public static bool IsFalse(this bool input)
        {
            return input == false;
        }

        /// <summary>
        /// Short way of "bool? == true"
        /// </summary>
        /// <param name="input">bool to check</param>
        /// <returns><see cref="Boolean"/></returns>
        public static bool IsTrue(this bool? input)
        {
            return input == true;
        }
        /// <summary>
        /// Short way of "bool == true"
        /// </summary>
        /// <param name="input">bool to check</param>
        /// <returns><see cref="Boolean"/></returns>
        public static bool IsTrue(this bool input)
        {
            return input == true;
        }
        #endregion
        #region Method start with NOT
        /// <summary>
        /// Short way of "bool? != false"
        /// </summary>
        /// <param name="input">bool to check</param>
        /// <returns><see cref="Boolean"/></returns>
        public static bool NotFalse(this bool? input)
        {
            return input != false;
        }
        /// <summary>
        /// Short way of "bool != false"
        /// </summary>
        /// <param name="input">bool to check</param>
        /// <returns><see cref="Boolean"/></returns>
        public static bool NotFalse(this bool input)
        {
            return input != false;
        }

        /// <summary>
        /// Short way of "bool? != true"
        /// </summary>
        /// <param name="input">bool to check</param>
        /// <returns><see cref="Boolean"/></returns>
        public static bool NotTrue(this bool? input)
        {
            return input != true;
        }
        /// <summary>
        /// Short way of "bool != true"
        /// </summary>
        /// <param name="input">bool to check</param>
        /// <returns><see cref="Boolean"/></returns>
        public static bool NotTrue(this bool input)
        {
            return input != true;
        }
        #endregion
    }
}
