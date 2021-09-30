using Ring.Util.Core.Extensions;
using Ring.Util.Models;
using System.Collections.Generic;

namespace Ring.Util.Builders
{
    internal static class ExcelSheetBuilder
    {
        public static ExcelSheet GetInstance(SortedDictionary<string, Dictionary<int, string>> columnDico, ExcelSheet partialExcelSheet,
                    List<string> mergeInfo, bool duplicateMergedCell)
        {
            var columnArray = GetColumnArray(columnDico, mergeInfo, duplicateMergedCell);
            var colCount = 0;
            var currentId = 0;
            var lastSheetRowNum = 0;
            var minSheetRowNum = int.MaxValue;

            // count effective columns
            for (var i = 0; i < columnArray.Length; ++i) if (columnArray[i].Value.Count > 0) ++colCount;

            // re-create ExcelSheet object
            var subResult = new ExcelColumn[colCount];


            // build cells dico
            for (var i = 0; i < columnArray.Length; ++i)
                if (columnArray[i].Value.Count > 0)
                {
                    var colDico = new Dictionary<int, string>(columnArray[i].Value.Count);
                    var tempArray = columnArray[i].Value.ToArray();
                    var maxId = 0;
                    var minId = int.MaxValue;

                    for (var j = 0; j < tempArray.Length; ++j)
                    {
                        var currentKey = tempArray[j].Key;
                        colDico.Add(currentKey, tempArray[j].Value);
                        if (currentKey > maxId) maxId = currentKey;
                        if (currentKey < minId) minId = currentKey;
                    }
                    subResult[currentId] = new ExcelColumn(columnArray[i].Key, minId, maxId, colDico);
                    if (maxId > lastSheetRowNum) lastSheetRowNum = maxId;
                    if (minId < minSheetRowNum) minSheetRowNum = minId;
                    ++currentId;
                }
            var result = new ExcelSheet(partialExcelSheet.Id, partialExcelSheet.Name, partialExcelSheet.FileName, subResult, minSheetRowNum, lastSheetRowNum, 0);
            return result;
        }

        #region private methods

        /// <summary>
        /// Get ColumnArray from merge information list
        /// </summary>
        private static KeyValuePair<string, Dictionary<int, string>>[] GetColumnArray
            (SortedDictionary<string, Dictionary<int, string>> columnDico, List<string> mergeInfo, bool duplicateMergedCell)
        {
            if (columnDico == null) return Constants.DefaultColumnArray;
            if (!duplicateMergedCell || mergeInfo == null || mergeInfo.Count == 0) return columnDico.ToArray(); // no need to duplicate cells 
            //TODO manage horizontal merge
            for (var i = 0; i < mergeInfo.Count; ++i)
            {
                var merge = mergeInfo[i];
                var letter = GetFirstLetter(merge);
                if (string.IsNullOrEmpty(letter)) continue;
                var minRange = GetMinRange(letter, merge);
                var maxRange = GetMaxRange(letter, merge);
                var subDico = columnDico.TryGetValue(letter.ToUpper());
                var value = subDico?.TryGetValue(minRange);
                if (value != null && maxRange > minRange)
                    for (var j = minRange + 1; j <= maxRange; ++j)
                        if (!subDico.ContainsKey(j)) subDico.Add(j, value);
            }
            var result = columnDico.ToArray();
            return result;
        }

        private static string GetFirstLetter(string input)
        {
            if (input == null) return null;
            var result = string.Empty;
            var i = 0;
            while (i < input.Length && (input[i] < Constants.MinDigitDelimiter || input[i] > Constants.MaxDigitDelimiter))
                result += input[i++];
            return result;
        }

        private static int GetMinRange(string letter, string input)
        {
            var result = 0;
            if (input == null || letter == null) return result;
            var temp = input.Replace(letter, null).Split(Constants.MergeRangeSplit);
            if (temp.Length > 1 && int.TryParse(temp[0], out result)) return result;
            return 0;
        }

        private static int GetMaxRange(string letter, string input)
        {
            var result = 0;
            if (input == null || letter == null) return result;
            var temp = input.Replace(letter, null).Split(Constants.MergeRangeSplit);
            if (temp.Length > 1 && int.TryParse(temp[1], out result)) return result;
            return 0;
        }
        #endregion

    }
}
