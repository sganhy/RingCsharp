using System.Globalization;

namespace Ring.Schema.Models
{
    internal sealed class Language : BaseEntity
    {
        internal readonly CultureInfo CultureInfo;
        internal readonly string NameUpper; // name in upperCase 

        /// <summary>
        ///     Ctor
        /// </summary>
        public Language(int id, string name, string description, CultureInfo culture, string nameUpper, bool active, bool baseline)
            : base(id, name, description, active, baseline)
        {
            CultureInfo = culture;
            NameUpper = nameUpper;
        }
    }
}