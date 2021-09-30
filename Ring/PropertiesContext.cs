using Ring.Data;
using Ring.Data.Core;

namespace Ring
{
    public class PropertiesContext
    {
        public object this[PropertyType type]
        {
            get
            {
                object result = null;
                switch (type)
                {
                    case PropertyType.MetaDataConnection:
                        result = Global.Databases.MetaSchema.Connections.ConnectionRef;
                        break;
                }
                return result;
            }
            set
            {
                switch (type)
                {
                    case PropertyType.MetaDataConnection:
                        Global.Databases.Add((IDbConnection)value);
                        break;
                }
            }
        }

    }
}
