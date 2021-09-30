using Ring.Schema.Core.Rules.Impl;

namespace Ring.Schema.Core.Rules
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface IValidationRule<in T>
    {
        /// <summary>
        /// Validation method
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        ValidationResult Validate(T[] source);
    }
}
