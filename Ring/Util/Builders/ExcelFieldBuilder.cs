using Ring.Schema.Core.Extensions;
using Ring.Schema.Models;
using Ring.Util.Models;
using System;
using System.Collections.Generic;

namespace Ring.Util.Builders
{
    internal static class ExcelFieldBuilder
    {
        public static ExcelField[] GetInstances(Table table, ExcelSheet sheet)
        {
            if (sheet == null || table == null) return Constants.DefaultExcelField;
            var result = new List<ExcelField>();
            var dicoKey = GetkeyDico(table); // keys ?

			// CRASH HERE !!!!
            for (var i = 0; i < sheet.Columns.Length; ++i)
            {
				#region body 
					var column = sheet.Columns[i];
					var cell = column.Cells;

					// take first row as header - first row (can be changed)
					if (!cell.ContainsKey(1) || cell[1] == null) continue;
					var field = table.GetField(cell[1], StringComparison.OrdinalIgnoreCase);
					// if no keys - take id by default
					if (dicoKey.Count == 0 && table.Readonly && string.CompareOrdinal(field?.Name, table.PrimaryKey.Name) == 0)
						result.Add(new ExcelField(field, null, sheet.Columns[i], 1, true));
					else
					{
						if (field != null)
							result.Add(new ExcelField(field, null, sheet.Columns[i], 1, dicoKey.Contains(field.Name.ToUpper())));
						else
						{
							// manage relations
							var header = cell[1];
							var stringArr = header.Split(Constants.RelationSeparator);
							if (stringArr.Length > 0)
							{
								var relation = table.GetRelation(stringArr[0]);
								// not a relation, not a field 
								if (relation != null)
								{
									field = stringArr.Length > 1 ? relation.To.GetField(stringArr[1]) : table.PrimaryKey;
									result.Add(new ExcelField(field, relation, sheet.Columns[i], 1, dicoKey.Contains(relation.Name.ToUpper())));
								}
							}
						}
					}
					#endregion
	        }
            return result.Count == 0 ? null : result.ToArray();
        }

        #region private methods 

        /// <summary>
        /// Get Key Dico 
        /// </summary>
        private static HashSet<string> GetkeyDico(Table table)
        {
            var result = new HashSet<string>();
            var keys = table.GetFirstKey();
            if (keys == null) return result;
            for (var i = 0; i < keys.Fields.Length; ++i) if (!result.Contains(keys.Fields[i].Name.ToUpper())) result.Add(keys.Fields[i].Name.ToUpper());
            return result;
        }

        #endregion 

    }
}
