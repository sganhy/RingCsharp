using Ring.Schema.Enums;

namespace Ring.Schema.Core.Extensions
{
    internal static class EntityTypeExtension
    {

        /// <summary>
        ///     Get Source id  from the entity sourceType
        /// </summary>
        /// <param name="entityType">BaseEntity sourceType</param>
        /// <returns></returns>
        public static sbyte GetId(this EntityType entityType) => (sbyte)entityType;


    }
}
