using Ring.Util.Builders;
using Ring.Util.Core.Extensions;
using Ring.Util.Helpers;
using Ring.Util.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace Ring.Util
{
    internal sealed class ExcelWorkbook
    {
        private string _fileName;
        private ExcelSheet[] _sheetList;
        private string _creator;
        private DateTime _created;
        private DateTime _modified;
        private List<string> _mergedCellList;

        public ExcelWorkbook()
        {
            _creator = string.Empty;
            _created = DateTime.MinValue;
            _modified = DateTime.MinValue;
            DuplicateMergedCell = false;
            _mergedCellList = null;
        }

        public void LoadDocument(string archiveFilename)
        {
            _fileName = archiveFilename;
            _sheetList = null;
            string[] sharedStringList = null;

            if (!File.Exists(archiveFilename)) return;

            #region unzip file 
            using (var fs = File.Open(archiveFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Read, false))
            {
                #region first pass - get relations def 
                for (var i = 0; i < zip.Entries.Count; ++i)
                {
                    var entry = zip.Entries[i];
                    var entryFileName = entry.Name;

                    if (string.IsNullOrEmpty(entryFileName)) continue;
                    if (entryFileName.EndsWith(Constants.XmlRelationSuffix, StringComparison.OrdinalIgnoreCase))
                        using (var reader = XmlReader.Create(entry.Open())) LoadRelationInformation(reader);

                }
                #endregion
                #region second pass - get shared string + sheet name
                for (var i = 0; i < zip.Entries.Count; ++i)
                {
                    var entry = zip.Entries[i];
                    var entryFileName = entry.Name;
                    var entryFileFullName = entry.FullName;

                    if (string.IsNullOrEmpty(entryFileName)) continue;
                    if (entryFileName.EndsWith(Constants.XmlFileExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        if (Constants.XmlSharedStringsFile.EndsWith(entryFileName, StringComparison.OrdinalIgnoreCase))
                            using (var reader = XmlReader.Create(entry.Open())) sharedStringList = LoadSharedString(reader);
                        if (Constants.XmlWorkgroupFile.Equals(entryFileFullName, StringComparison.OrdinalIgnoreCase))
                            using (var reader = XmlReader.Create(entry.Open())) LoadSheetInformation(reader);
                        if (entryFileName.EndsWith(Constants.XmlCoreFile, StringComparison.OrdinalIgnoreCase))
                            using (var reader = XmlReader.Create(entry.Open())) LoadCoreData(reader);
                    }
                }
                #endregion
                #region third pass - get sheet information
                for (var i = 0; i < zip.Entries.Count; ++i)
                {
                    var entry = zip.Entries[i];
                    // reading sheet*.xml
                    var entryFileName = entry.Name;
                    if (!entryFileName.StartsWith(Constants.XmlStartSheetDocument, StringComparison.OrdinalIgnoreCase) ||
                        !entryFileName.EndsWith(Constants.XmlEndSheetDocument, StringComparison.OrdinalIgnoreCase)) continue;
                    using (var reader = XmlReader.Create(entry.Open()))
                        LoadSheetData(entry.FullName, reader, sharedStringList);
                }
                #endregion 
            }
            #endregion
        }
        public int SheetCount => _sheetList.Length;
        public string FileName => _fileName;
        public DateTime Modified => _modified;
        public DateTime Created => _created;
        public string Creator => _creator;
        public bool DuplicateMergedCell { get; set; }

        public List<string> MergedCells => _mergedCellList;

        /// <summary>
        /// Returns the index of the sheet by his name (case insensitive match)
        /// </summary>
        /// <param name="id"></param>
        /// <returns>the sheet or null if not found</returns>
        public ExcelSheet GetSheet(int id) => id >= 0 || id < _sheetList.Length ? _sheetList?[id] : null;

        /// <summary>
        /// Returns the index of the sheet by his name (case insensitive match)
        /// </summary>
        /// <param name="name"></param>
        /// <returns>the sheet or null if not found</returns>
        public ExcelSheet GetSheet(string name)
        {
            if (_sheetList == null || _sheetList.Length == 0 || name == null) return null;
            //1) try to find exact name
            for (var i = 0; i < _sheetList.Length; ++i)
                if (string.CompareOrdinal(_sheetList[i].Name, name) == 0) return _sheetList[i];
            //2) try to find case insensitif
            for (var i = 0; i < _sheetList.Length; ++i)
                if (string.Compare(_sheetList[i].Name, name, StringComparison.OrdinalIgnoreCase) == 0) return _sheetList[i];
            //3) try to find case insensitif + trim
            for (var i = 0; i < _sheetList.Length; ++i)
                if (_sheetList[i].Name != null &&
                    string.Compare(_sheetList[i].Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
                    return _sheetList[i];
            return null;
        }

        #region private methods 
        private string[] LoadSharedString(XmlReader reader)
        {
            var sharedStringList = new List<string>();
	        while (reader.Read())
		        if (reader.NodeType == XmlNodeType.Text  || reader.NodeType == XmlNodeType.SignificantWhitespace)
					sharedStringList.Add(reader.Value);
	        return sharedStringList.ToArray();
        }
        private void LoadRelationInformation(XmlReader reader)
        {
            var sheets = new List<ExcelSheet>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (!Constants.XmlRelationshipNode.Equals(reader.Name, StringComparison.OrdinalIgnoreCase)) continue;
                    var id = XmlHelper.GetAttributeValue(reader, Constants.XmlSheetIdAttribute, false);
                    var target = XmlHelper.GetAttributeValue(reader, Constants.XmlRelationshipTarget, false);

                    if (string.IsNullOrEmpty(target)) continue;
                    if (target.IndexOf(Constants.XmlStartSheetDocument, StringComparison.OrdinalIgnoreCase) >= 0 &&
                            target.EndsWith(Constants.XmlEndSheetDocument, StringComparison.OrdinalIgnoreCase))
                        sheets.Add(new ExcelSheet(id, null, target, null, 0, 0, 0));
                }
            }
            // load sheet information once !!
            if (_sheetList == null || _sheetList.Length <= 0)
                _sheetList = sheets.ToArray();
        }
        private void LoadSheetInformation(XmlReader reader)
        {
            var sheetDico = new SortedDictionary<string, ExcelSheet>();

            // generate temp dico
            for (var i = 0; i < _sheetList.Length; ++i) sheetDico.TryToAdd(_sheetList[i]?.Id?.ToUpper(), _sheetList[i]);
            var sheets = new List<ExcelSheet>();

            // read xml 
            while (reader.Read())
            {
                if (Constants.XmlSheetNode.Equals(reader.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var sheetId = XmlHelper.GetAttributeValue(reader, Constants.XmlSheetIdAttribute, false);
                    if (sheetId == null) continue;
                    var sheetObj = sheetDico.TryGetValue(sheetId.ToUpper());
                    if (sheetObj == null) continue;
                    // valid sheet - recreate later the sheet 
                    sheets.Add(new ExcelSheet(sheetId,
                        XmlHelper.GetAttributeValue(reader, Constants.XmlSheetNameAttribute),
                        RemovePathInfo(sheetObj.FileName), null, 0, 0, 0));
                }
            }
            // load sheet information once !!
            _sheetList = sheets.ToArray();
        }
        private void LoadSheetData(string entryName, XmlReader reader, string[] sharedStrings)
        {
            var rowId = 0;
            var currentLetter = string.Empty;
            var columnDico = new SortedDictionary<string, Dictionary<int, string>>(); // <letter , <rowid,dataId>>
            var newSharedStringDico = new SortedDictionary<decimal, string>(); // <letter , <rowid,dataId>>
            var useSharedString = false;
            _mergedCellList = new List<string>();

            #region read sheet{0}.xml
            while (reader.Read())
            {

                if (reader.NodeType == XmlNodeType.Element)
                {
                    // Get RowId - reader.Name == "row"
                    if (Constants.XmlSheetDataRow.Equals(reader.Name, StringComparison.OrdinalIgnoreCase))
                        int.TryParse(XmlHelper.GetAttributeValue(reader, Constants.XmlSheetDataRowAttribute), out rowId);

                    // Get column letter - reader.Name == "c"
                    if (Constants.XmlSheetDataColAttribute.Equals(reader.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        currentLetter = XmlHelper.GetAttributeValue(reader, Constants.XmlSheetDataRowAttribute)
                            .Replace(rowId.ToString(), null).ToUpper();
                        var currentType = XmlHelper.GetAttributeValue(reader, Constants.XmlSheetDataTypeAttribute);
                        useSharedString = Constants.XmlSheetDataTypeStringAttribute.Equals(currentType, StringComparison.OrdinalIgnoreCase);
                    }

                    // allow structure for the current letter 
                    if (!columnDico.ContainsKey(currentLetter)) columnDico.Add(currentLetter, new Dictionary<int, string>());

                    // Get data - reader.Name == "v"
                    if (Constants.XmlSheetDataColValAttribute.Equals(reader.Name, StringComparison.OrdinalIgnoreCase) &&
                        !columnDico[currentLetter].ContainsKey(rowId))
                    {
                        if (useSharedString)
                        {
                            int dataId;
                            if (int.TryParse(reader.ReadString(), out dataId))
                                columnDico[currentLetter].Add(rowId, GetData(sharedStrings, dataId));
                        }
                        else
                        {
                            decimal dataId;
                            if (decimal.TryParse(reader.ReadString(), out dataId) && !newSharedStringDico.ContainsKey(dataId))
                                newSharedStringDico.Add(dataId, dataId.ToString(CultureInfo.InvariantCulture));
                            columnDico[currentLetter].Add(rowId, newSharedStringDico[dataId]);
                        }
                    }

                    // get merge information
                    if (Constants.XmlSheetMergedCellInfo.Equals(reader.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        var merge = XmlHelper.GetAttributeValue(reader, Constants.XmlSheetMergeRef);
                        if (!string.IsNullOrWhiteSpace(merge)) _mergedCellList.Add(merge);
                    }
                }
            }
            #endregion 

            // Array.Resize(ref _sheetList, _sheetList.Length+1); no resize here sheet should exists already
            // add result to end of array 
            var sheetIndex = GetSheetIndex(entryName);
            if (sheetIndex >= 0)
                _sheetList[sheetIndex] = ExcelSheetBuilder.GetInstance(columnDico, _sheetList[sheetIndex],
                        _mergedCellList, DuplicateMergedCell);

        }
        private int GetSheetIndex(string fileName)
        {
            var criteria = RemovePathInfo(fileName);
            for (var i = 0; i < _sheetList.Length; ++i)
                if (string.Equals(_sheetList[i].FileName, criteria, StringComparison.OrdinalIgnoreCase))
                    return i;
            return -1;
        }

        private static string GetData(string[] sharedStrings, int id) => sharedStrings != null && id >= 0 && id < sharedStrings.Length
                            ? sharedStrings[id] : string.Empty;
        private void LoadCoreData(XmlReader reader)
        {
            while (reader.Read())
            {

                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (string.IsNullOrEmpty(reader.Name)) continue;
                    if (reader.Name.EndsWith(Constants.XmlCreatorNode, StringComparison.OrdinalIgnoreCase))
                        _creator = reader.ReadElementString();
                    DateTime dtTemp;
                    if (reader.Name.EndsWith(Constants.XmlCreated, StringComparison.OrdinalIgnoreCase) &&
                        DateTime.TryParse(reader.ReadElementString(), out dtTemp)) _created = dtTemp;
                    if (reader.Name.EndsWith(Constants.XmlModified, StringComparison.OrdinalIgnoreCase) &&
                        DateTime.TryParse(reader.ReadElementString(), out dtTemp)) _modified = dtTemp;
                }
            }
        }

        private string RemovePathInfo(string file)
        {
            if (file == null) return null;
            var arr = file.Split(Constants.PathSeparator);
            return arr[arr.Length - 1];
        }

        #endregion

    }
}
