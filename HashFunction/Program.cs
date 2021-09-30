using System;
using System.Collections.Generic;
using System.Linq;

[assembly: CLSCompliant(true)]
namespace ConsoleApplication8
{
	[CLSCompliant(true)]
	public class Program
    {
        private static Random _random = new Random();
        private const int elementCount= 257;

        static void Main(string[] args)
        {
	        EntityComparison eleme;

			uint oopp = 0;

	        ++oopp;
			var testR = HashMap4.RotateLeft(-1377384929, 15);
	        var testR2 = HashMap4.RotateLeft(int.MinValue, 15);
	        var testR3 = HashMap4.RotateLeft(int.MaxValue, 15);


			var source  = new Dictionary<string, string>();
            LoadValue(source);
	        var count = elementCount*5;
			var dico1 = new Dictionary<string, string>(count);
            var dico2 = new SortedDictionary<string, string>();
            var hash = new HashMap1(count);
            var hash2 = new HashMap2(count);
            var hash3 = new HashMap3(count);
	        var hash4 = new HashMap4(count);
	        var hash5 = new HashMap5(count);
			var hash100 = new Generic.HashMap<string, string>(count);
            var hash200 = new Generic.HashMap<int, string>(count);
            var hash201 = new Dictionary<int, string>(count);


            var heelo =  "AC";
			var heelo2 = heelo.GetHashCode();

			var startTime = DateTime.Now;
            var key= string.Empty;
            var iii = 0;

            foreach (var keyValue in source)
            {
                //key = RandomString(15);
                //var value = RandomString(50);
                key = keyValue.Key;
                var value = keyValue.Value;
                if (!dico2.ContainsKey(key))
                {
                    dico1.Add(key,value);
                    dico2.Add(key,value);
                    hash.Add(key, value);
                    hash2.Add(key, value);
                    hash3.Add(key, value);
	                hash4.Add(key, value);
	                hash5.Add(key, value);
	                hash100.Add(key, value);
                    hash200.Add(iii, value);
                    hash201.Add(iii, value);
                }
                ++iii;
            }
			var arr = dico1.ToArray();
            var arr2 = new string[dico1.Count];
            // copy array 
            for (var i=0; i < arr.Length; ++i) arr2[i] = arr[i].Key;
            Array.Sort(arr2);
            Console.WriteLine("Items       : " + dico2.Count.ToString());
            Console.WriteLine("Bucket Size : " + count.ToString());
            var table = new Table(arr2);
            // **********************
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Start --> SortedDictionary<string, string>\t\t\t\t\t");
            startTime = DateTime.Now;
            for (var i = 0; i < 10000; ++i)
            {
                var test = dico2[key];
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(DateTime.Now- startTime);

            // **********************
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Start --> Dictionary<string, string>\t\t\t\t\t\t");
            startTime = DateTime.Now;
            for (var i = 0; i < 10000000; ++i)
            {
                var test = dico1[key];
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(DateTime.Now - startTime);

	        // **********************
	        Console.ForegroundColor = ConsoleColor.White;
	        Console.Write("Start --> Table.GetField()\t\t\t\t\t\t\t");
	        startTime = DateTime.Now;
	        for (var i = 0; i < 10000000; ++i)
	        {
		        var test = table.GetField(key);
	        }
	        Console.ForegroundColor = ConsoleColor.Green;
	        Console.WriteLine(DateTime.Now - startTime);

			// **********************
			Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Start --> Table.GetField() without MethodImplOptions.AggressiveInlining\t\t");
			startTime = DateTime.Now;
            for (var i = 0; i < 10000000; ++i)
            {
                var test = table.GetField2(key);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(DateTime.Now - startTime);

            // **********************
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Start --> Table.GetField() without MethodImplOptions.AggressiveInlining (div)\t");
            startTime = DateTime.Now;
            for (var i = 0; i < 10000000; ++i)
            {
                var test = table.GetField3(key);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(DateTime.Now - startTime);

	        Console.WriteLine("---------------------------------------------------");

			// ********************** 1
			Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Start --> HashMap<string, string> (djb2)\t\t\t\t\t");
            startTime = DateTime.Now;
            for (var i = 0; i < 10000000; ++i)
            {
                var test = hash.Get(key);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            var result = Math.Round((double)hash.Collisions / hash.Count, 2) * 100;
            Console.WriteLine(DateTime.Now - startTime + " collisions=" + hash.Collisions + " - " + result + "%" + " - " + hash.GetMaxLevel());
            // ********************** 2
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Start --> HashMap<string, string> (GetHashCode)\t\t\t\t\t");
            startTime = DateTime.Now;
            for (var i = 0; i < 10000000; ++i)
            {
                var test = hash2.Get(key);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            result = Math.Round((double)hash2.Collisions / hash2.Count, 2) * 100;
            Console.WriteLine(DateTime.Now - startTime + " collisions=" + hash2.Collisions + " - " + result + "% - " + hash2.GetMaxLevel());
            
            // ********************** 3
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Start --> HashMap<string, string> (java hashCode)\t\t\t\t");
            startTime = DateTime.Now;
            for (var i = 0; i < 10000000; ++i)
            {
                var test = hash3.Get(key);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            result = Math.Round((double)hash3.Collisions / hash3.Count, 2) * 100;
            Console.WriteLine(DateTime.Now - startTime + " collisions=" + hash3.Collisions + " - " + result + "% - " + hash3.GetMaxLevel());

			// ********************** 4
			Console.ForegroundColor = ConsoleColor.White;
	        Console.Write("Start --> HashMap<string, string> (Murmur3)\t\t\t\t\t");
	        startTime = DateTime.Now;
	        for (var i = 0; i < 10000000; ++i)
	        {
		        var test = hash4.Get(key);
	        }
	        Console.ForegroundColor = ConsoleColor.Green;
	        result = Math.Round((double)hash4.Collisions / hash4.Count, 2) * 100;
	        Console.WriteLine(DateTime.Now - startTime + " collisions=" + hash4.Collisions + " - " + result + "% - " + hash4.GetMaxLevel());

	        // ********************** 5
	        Console.ForegroundColor = ConsoleColor.White;
	        Console.Write("Start --> HashMap<string, string> (SBDM)\t\t\t\t\t");
	        startTime = DateTime.Now;
	        for (var i = 0; i < 10000000; ++i)
	        {
		        var test = hash5.Get(key);
	        }
	        Console.ForegroundColor = ConsoleColor.Green;
	        result = Math.Round((double)hash5.Collisions / hash5.Count, 2) * 100;
	        Console.WriteLine(DateTime.Now - startTime + " collisions=" + hash5.Collisions + " - " + result + "% - " + hash5.GetMaxLevel());

			// ********************** 100
			Console.ForegroundColor = ConsoleColor.White;
	        Console.Write("Start --> HashMap<string, string> (Generic)\t\t\t\t\t");
	        startTime = DateTime.Now;
	        for (var i = 0; i < 10000000; ++i)
	        {
		        var test = hash100.Get(key);
	        }
	        Console.ForegroundColor = ConsoleColor.Green;
	        result = Math.Round((double)hash100.Collisions / hash100.Count, 2) * 100;
			Console.WriteLine(DateTime.Now - startTime + " collisions=" + hash100.Collisions + " - " + result + "% - ");

	        Console.WriteLine("---------------------------------------------------");

            // ********************** 200
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Start --> HashMap<int, string> (Generic) Full Avalanche\t\t\t\t");
            startTime = DateTime.Now;
            for (var i = 0; i < 10000000; ++i)
            {
                var test = hash200.Get(57);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            result = Math.Round((double)hash200.Collisions / hash200.Count, 2) * 100;
            Console.WriteLine(DateTime.Now - startTime + " collisions=" + hash200.Collisions + " - " + result + "% - ");

            // ********************** 201
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Start --> Dictionary<int, string>\t\t\t\t\t\t");
            startTime = DateTime.Now;
            for (var i = 0; i < 10000000; ++i)
            {
                var test = hash201[57];
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(DateTime.Now - startTime + " collisions= 0 - % - ");

            // final 
            Console.ForegroundColor = ConsoleColor.Gray;

		}

		public static void LoadValue(Dictionary<string, string> dico)
        {
			/*
#region key iso2 code 
dico.Add("AC", "Ascension Island");
dico.Add("BQ", "Bonaire");
dico.Add("CX", "Christmas Island");
dico.Add("CC", "Cocos (Keeling) Islands");
dico.Add("CW", "Curaçao");
dico.Add("AX", "Åland Islands");
dico.Add("AL", "Albania");
dico.Add("DZ", "Algeria");
dico.Add("AS", "American Samoa");
dico.Add("AD", "Andorra");
dico.Add("AO", "Angola");
dico.Add("AI", "Anguilla");
dico.Add("AQ", "Antarctica");
dico.Add("AG", "Antigua and Barbuda");
dico.Add("AR", "Argentina");
dico.Add("AM", "Armenia");
dico.Add("AW", "Aruba");
dico.Add("AU", "Australia");
dico.Add("AT", "Austria");
dico.Add("AZ", "Azerbaijan");
dico.Add("BS", "Bahamas");
dico.Add("BH", "Bahrain");
dico.Add("BD", "Bangladesh");
dico.Add("BB", "Barbados");
dico.Add("BY", "Belarus");
dico.Add("BE", "Belgium");
dico.Add("BZ", "Belize");
dico.Add("BJ", "Benin");
dico.Add("BM", "Bermuda");
dico.Add("BT", "Bhutan");
dico.Add("BO", "Bolivia");
dico.Add("BA", "Bosnia and Herzegovina");
dico.Add("BW", "Botswana");
dico.Add("BV", "Bouvet Island");
dico.Add("BR", "Brazil");
dico.Add("VG", "British Virgin Islands");
dico.Add("BN", "Brunei");
dico.Add("BG", "Bulgaria");
dico.Add("BF", "Burkina Faso");
dico.Add("BI", "Burundi");
dico.Add("CV", "Cabo Verde");
dico.Add("KH", "Cambodia");
dico.Add("CM", "Cameroon");
dico.Add("CA", "Canada");
dico.Add("KY", "Cayman Islands");
dico.Add("CF", "Central African Republic");
dico.Add("TD", "Chad");
dico.Add("CL", "Chile");
dico.Add("CN", "China");
dico.Add("CO", "Colombia");
dico.Add("KM", "Comoros");
dico.Add("CG", "Congo");
dico.Add("CD", "Congo (DRC)");
dico.Add("CK", "Cook Islands");
dico.Add("CR", "Costa Rica");
dico.Add("CI", "Côte d'Ivoire");
dico.Add("HR", "Croatia");
dico.Add("CU", "Cuba");
dico.Add("CY", "Cyprus");
dico.Add("CZ", "Czechia");
dico.Add("DK", "Denmark");
dico.Add("DJ", "Djibouti");
dico.Add("DM", "Dominica");
dico.Add("DO", "Dominican Republic");
dico.Add("EC", "Ecuador");
dico.Add("EG", "Egypt");
dico.Add("SV", "El Salvador");
dico.Add("GQ", "Equatorial Guinea");
dico.Add("ER", "Eritrea");
dico.Add("EE", "Estonia");
dico.Add("ET", "Ethiopia");
dico.Add("FK", "Falkland Islands");
dico.Add("FO", "Faroe Islands");
dico.Add("FJ", "Fiji");
dico.Add("FI", "Finland");
dico.Add("FR", "France");
dico.Add("PF", "French Polynesia");
dico.Add("TF", "French Southern Territories");
dico.Add("GA", "Gabon");
dico.Add("GM", "Gambia");
dico.Add("XJ", "Jan Mayen");
dico.Add("AN", "Netherlands Antilles");
dico.Add("DE", "Germany");
dico.Add("GH", "Ghana");
dico.Add("GI", "Gibraltar");
dico.Add("GR", "Greece");
dico.Add("GL", "Greenland");
dico.Add("GD", "Grenada");
dico.Add("GP", "Guadeloupe");
dico.Add("GU", "Guam");
dico.Add("GT", "Guatemala");
dico.Add("GN", "Guinea");
dico.Add("GW", "Guinea-Bissau");
dico.Add("GY", "Guyana");
dico.Add("HT", "Haiti");
dico.Add("HM", "Heard Island and McDonald Islands");
dico.Add("HN", "Honduras");
dico.Add("HK", "Hong Kong SAR");
dico.Add("HU", "Hungary");
dico.Add("IS", "Iceland");
dico.Add("IN", "India");
dico.Add("ID", "Indonesia");
dico.Add("IR", "Iran");
dico.Add("IQ", "Iraq");
dico.Add("IE", "Ireland");
dico.Add("IM", "Isle of Man");
dico.Add("IL", "Israel");
dico.Add("IT", "Italy");
dico.Add("JM", "Jamaica");
dico.Add("JP", "Japan");
dico.Add("JE", "Jersey");
dico.Add("JO", "Jordan");
dico.Add("KE", "Kenya");
dico.Add("KI", "Kiribati");
dico.Add("KR", "Korea");
dico.Add("XK", "Kosovo");
dico.Add("KW", "Kuwait");
dico.Add("KG", "Kyrgyzstan");
dico.Add("LA", "Laos");
dico.Add("LV", "Latvia");
dico.Add("LB", "Lebanon");
dico.Add("LS", "Lesotho");
dico.Add("LR", "Liberia");
dico.Add("LY", "Libya");
dico.Add("LI", "Liechtenstein");
dico.Add("LT", "Lithuania");
dico.Add("LU", "Luxembourg");
dico.Add("MO", "Macao SAR");
dico.Add("MK", "Macedonia");
dico.Add("MG", "Madagascar");
dico.Add("MW", "Malawi");
dico.Add("MY", "Malaysia");
dico.Add("MV", "Maldives");
dico.Add("ML", "Mali");
dico.Add("MT", "Malta");
dico.Add("MQ", "Martinique");
dico.Add("MR", "Mauritania");
dico.Add("MU", "Mauritius");
dico.Add("YT", "Mayotte");
dico.Add("MX", "Mexico");
dico.Add("FM", "Micronesia");
dico.Add("MD", "Moldova");
dico.Add("MC", "Monaco");
dico.Add("MN", "Mongolia");
dico.Add("MS", "Montserrat");
dico.Add("MA", "Morocco");
dico.Add("MZ", "Mozambique");
dico.Add("MM", "Myanmar");
dico.Add("NA", "Namibia");
dico.Add("NR", "Nauru");
dico.Add("NP", "Nepal");
dico.Add("NL", "Netherlands");
dico.Add("NC", "New Caledonia");
dico.Add("NZ", "New Zealand");
dico.Add("NI", "Nicaragua");
dico.Add("NE", "Niger");
dico.Add("NG", "Nigeria");
dico.Add("NU", "Niue");
dico.Add("KP", "North Korea");
dico.Add("MP", "Northern Mariana Islands");
dico.Add("NO", "Norway");
dico.Add("ME", "Serbia, Montenegro");
dico.Add("XS", "Saba");
dico.Add("XE", "Sint Eustatius");
dico.Add("SX", "Sint Maarten");
dico.Add("TP", "Timor-Leste (East Timor)");
dico.Add("TA", "Tristan da Cunha");
dico.Add("OM", "Oman");
dico.Add("PW", "Palau");
dico.Add("PS", "Palestinian Authority");
dico.Add("PA", "Panama");
dico.Add("PG", "Papua New Guinea");
dico.Add("PY", "Paraguay");
dico.Add("PE", "Peru");
dico.Add("PH", "Philippines");
dico.Add("PN", "Pitcairn Islands");
dico.Add("PL", "Poland");
dico.Add("PT", "Portugal");
dico.Add("PR", "Puerto Rico");
dico.Add("QA", "Qatar");
dico.Add("RE", "Réunion");
dico.Add("RO", "Romania");
dico.Add("RU", "Russia");
dico.Add("RW", "Rwanda");
dico.Add("BL", "Saint Barthélemy");
dico.Add("SH", "Saint Helena");
dico.Add("LC", "Saint Lucia");
dico.Add("MF", "Saint Martin");
dico.Add("PM", "Saint Pierre and Miquelon");
dico.Add("VC", "Saint Vincent and Grenadines");
dico.Add("WS", "Samoa");
dico.Add("SM", "San Marino");
dico.Add("ST", "São Tomé and Príncipe");
dico.Add("SA", "Saudi Arabia");
dico.Add("SN", "Senegal");
dico.Add("RS", "Serbia");
dico.Add("SC", "Seychelles");
dico.Add("SL", "Sierra Leone");
dico.Add("SG", "Singapore");
dico.Add("SK", "Slovakia");
dico.Add("SI", "Slovenia");
dico.Add("SB", "Solomon Islands");
dico.Add("SO", "Somalia");
dico.Add("ZA", "South Africa");
dico.Add("SS", "South Sudan");
dico.Add("ES", "Spain");
dico.Add("LK", "Sri Lanka");
dico.Add("SD", "Sudan");
dico.Add("SR", "Suriname");
dico.Add("SJ", "Svalbard");
dico.Add("SZ", "Swaziland");
dico.Add("SE", "Sweden");
dico.Add("CH", "Switzerland");
dico.Add("SY", "Syria");
dico.Add("TW", "Taiwan");
dico.Add("TJ", "Tajikistan");
dico.Add("TZ", "Tanzania");
dico.Add("TH", "Thailand");
dico.Add("TL", "Timor-Leste");
dico.Add("TG", "Togo");
dico.Add("TK", "Tokelau");
dico.Add("TO", "Tonga");
dico.Add("TT", "Trinidad and Tobago");
dico.Add("TN", "Tunisia");
dico.Add("TR", "Turkey");
dico.Add("TM", "Turkmenistan");
dico.Add("TC", "Turks and Caicos Islands");
dico.Add("TV", "Tuvalu");
dico.Add("VI", "U.S. Virgin Islands");
dico.Add("UM", "U.S. Minor Outlying Islands");
dico.Add("UG", "Uganda");
dico.Add("UA", "Ukraine");
dico.Add("AE", "United Arab Emirates");
dico.Add("GB", "United Kingdom");
dico.Add("US", "United States");
dico.Add("UY", "Uruguay");
dico.Add("UZ", "Uzbekistan");
dico.Add("VU", "Vanuatu");
dico.Add("VA", "Vatican City");
dico.Add("VE", "Venezuela");
dico.Add("VN", "Vietnam");
dico.Add("WF", "Wallis and Futuna");
dico.Add("AF", "Afghanistan");
dico.Add("IO", "British Indian Ocean Territory");
dico.Add("GF", "French Guiana");
dico.Add("GE", "Georgia");
dico.Add("GG", "Guernsey");
dico.Add("KZ", "Kazakhstan");
dico.Add("MH", "Marshall Islands");
dico.Add("NF", "Norfolk Island");
dico.Add("PK", "Pakistan");
dico.Add("KN", "Saint Kitts and Nevis");
dico.Add("GS", "South Georgia and the South Sandwich Islands");
dico.Add("EH", "Western Sahara");
dico.Add("YE", "Yemen");
dico.Add("ZM", "Zambia");
dico.Add("ZW", "Zimbabwe");
#endregion
*/
dico.Add("Ascension Island", "AC");
dico.Add("Bonaire", "BQ");
dico.Add("Christmas Island", "CX");
dico.Add("Cocos (Keeling) Islands", "CC");
dico.Add("Curaçao", "CW");
dico.Add("Åland Islands", "AX");
dico.Add("Albania", "AL");
dico.Add("Algeria", "DZ");
dico.Add("American Samoa", "AS");
dico.Add("Andorra", "AD");
dico.Add("Angola", "AO");
dico.Add("Anguilla", "AI");
dico.Add("Antarctica", "AQ");
dico.Add("Antigua and Barbuda", "AG");
dico.Add("Argentina", "AR");
dico.Add("Armenia", "AM");
dico.Add("Aruba", "AW");
dico.Add("Australia", "AU");
dico.Add("Austria", "AT");
dico.Add("Azerbaijan", "AZ");
dico.Add("Bahamas", "BS");
dico.Add("Bahrain", "BH");
dico.Add("Bangladesh", "BD");
dico.Add("Barbados", "BB");
dico.Add("Belarus", "BY");
dico.Add("Belgium", "BE");
dico.Add("Belize", "BZ");
dico.Add("Benin", "BJ");
dico.Add("Bermuda", "BM");
dico.Add("Bhutan", "BT");
dico.Add("Bolivia", "BO");
dico.Add("Bosnia and Herzegovina", "BA");
dico.Add("Botswana", "BW");
dico.Add("Bouvet Island", "BV");
dico.Add("Brazil", "BR");
dico.Add("British Virgin Islands", "VG");
dico.Add("Brunei", "BN");
dico.Add("Bulgaria", "BG");
dico.Add("Burkina Faso", "BF");
dico.Add("Burundi", "BI");
dico.Add("Cabo Verde", "CV");
dico.Add("Cambodia", "KH");
dico.Add("Cameroon", "CM");
dico.Add("Canada", "CA");
dico.Add("Cayman Islands", "KY");
dico.Add("Central African Republic", "CF");
dico.Add("Chad", "TD");
dico.Add("Chile", "CL");
dico.Add("China", "CN");
dico.Add("Colombia", "CO");
dico.Add("Comoros", "KM");
dico.Add("Congo", "CG");
dico.Add("Congo (DRC)", "CD");
dico.Add("Cook Islands", "CK");
dico.Add("Costa Rica", "CR");
dico.Add("Côte d'Ivoire", "CI");
dico.Add("Croatia", "HR");
dico.Add("Cuba", "CU");
dico.Add("Cyprus", "CY");
dico.Add("Czechia", "CZ");
dico.Add("Denmark", "DK");
dico.Add("Djibouti", "DJ");
dico.Add("Dominica", "DM");
dico.Add("Dominican Republic", "DO");
dico.Add("Ecuador", "EC");
dico.Add("Egypt", "EG");
dico.Add("El Salvador", "SV");
dico.Add("Equatorial Guinea", "GQ");
dico.Add("Eritrea", "ER");
dico.Add("Estonia", "EE");
dico.Add("Ethiopia", "ET");
dico.Add("Falkland Islands", "FK");
dico.Add("Faroe Islands", "FO");
dico.Add("Fiji", "FJ");
dico.Add("Finland", "FI");
dico.Add("France", "FR");
dico.Add("French Polynesia", "PF");
dico.Add("French Southern Territories", "TF");
dico.Add("Gabon", "GA");
dico.Add("Gambia", "GM");
dico.Add("Jan Mayen", "XJ");
dico.Add("Netherlands Antilles", "AN");
dico.Add("Germany", "DE");
dico.Add("Ghana", "GH");
dico.Add("Gibraltar", "GI");
dico.Add("Greece", "GR");
dico.Add("Greenland", "GL");
dico.Add("Grenada", "GD");
dico.Add("Guadeloupe", "GP");
dico.Add("Guam", "GU");
dico.Add("Guatemala", "GT");
dico.Add("Guinea", "GN");
dico.Add("Guinea-Bissau", "GW");
dico.Add("Guyana", "GY");
dico.Add("Haiti", "HT");
dico.Add("Heard Island and McDonald Islands", "HM");
dico.Add("Honduras", "HN");
dico.Add("Hong Kong SAR", "HK");
dico.Add("Hungary", "HU");
dico.Add("Iceland", "IS");
dico.Add("India", "IN");
dico.Add("Indonesia", "ID");
dico.Add("Iran", "IR");
dico.Add("Iraq", "IQ");
dico.Add("Ireland", "IE");
dico.Add("Isle of Man", "IM");
dico.Add("Israel", "IL");
dico.Add("Italy", "IT");
dico.Add("Jamaica", "JM");
dico.Add("Japan", "JP");
dico.Add("Jersey", "JE");
dico.Add("Jordan", "JO");
dico.Add("Kenya", "KE");
dico.Add("Kiribati", "KI");
dico.Add("Korea", "KR");
dico.Add("Kosovo", "XK");
dico.Add("Kuwait", "KW");
dico.Add("Kyrgyzstan", "KG");
dico.Add("Laos", "LA");
dico.Add("Latvia", "LV");
dico.Add("Lebanon", "LB");
dico.Add("Lesotho", "LS");
dico.Add("Liberia", "LR");
dico.Add("Libya", "LY");
dico.Add("Liechtenstein", "LI");
dico.Add("Lithuania", "LT");
dico.Add("Luxembourg", "LU");
dico.Add("Macao SAR", "MO");
dico.Add("Macedonia", "MK");
dico.Add("Madagascar", "MG");
dico.Add("Malawi", "MW");
dico.Add("Malaysia", "MY");
dico.Add("Maldives", "MV");
dico.Add("Mali", "ML");
dico.Add("Malta", "MT");
dico.Add("Martinique", "MQ");
dico.Add("Mauritania", "MR");
dico.Add("Mauritius", "MU");
dico.Add("Mayotte", "YT");
dico.Add("Mexico", "MX");
dico.Add("Micronesia", "FM");
dico.Add("Moldova", "MD");
dico.Add("Monaco", "MC");
dico.Add("Mongolia", "MN");
dico.Add("Montserrat", "MS");
dico.Add("Morocco", "MA");
dico.Add("Mozambique", "MZ");
dico.Add("Myanmar", "MM");
dico.Add("Namibia", "NA");
dico.Add("Nauru", "NR");
dico.Add("Nepal", "NP");
dico.Add("Netherlands", "NL");
dico.Add("New Caledonia", "NC");
dico.Add("New Zealand", "NZ");
dico.Add("Nicaragua", "NI");
dico.Add("Niger", "NE");
dico.Add("Nigeria", "NG");
dico.Add("Niue", "NU");
dico.Add("North Korea", "KP");
dico.Add("Northern Mariana Islands", "MP");
dico.Add("Norway", "NO");
dico.Add("Serbia, Montenegro", "ME");
dico.Add("Saba", "XS");
dico.Add("Sint Eustatius", "XE");
dico.Add("Sint Maarten", "SX");
dico.Add("Timor-Leste (East Timor)", "TP");
dico.Add("Tristan da Cunha", "TA");
dico.Add("Oman", "OM");
dico.Add("Palau", "PW");
dico.Add("Palestinian Authority", "PS");
dico.Add("Panama", "PA");
dico.Add("Papua New Guinea", "PG");
dico.Add("Paraguay", "PY");
dico.Add("Peru", "PE");
dico.Add("Philippines", "PH");
dico.Add("Pitcairn Islands", "PN");
dico.Add("Poland", "PL");
dico.Add("Portugal", "PT");
dico.Add("Puerto Rico", "PR");
dico.Add("Qatar", "QA");
dico.Add("Réunion", "RE");
dico.Add("Romania", "RO");
dico.Add("Russia", "RU");
dico.Add("Rwanda", "RW");
dico.Add("Saint Barthélemy", "BL");
dico.Add("Saint Helena", "SH");
dico.Add("Saint Lucia", "LC");
dico.Add("Saint Martin", "MF");
dico.Add("Saint Pierre and Miquelon", "PM");
dico.Add("Saint Vincent and Grenadines", "VC");
dico.Add("Samoa", "WS");
dico.Add("San Marino", "SM");
dico.Add("São Tomé and Príncipe", "ST");
dico.Add("Saudi Arabia", "SA");
dico.Add("Senegal", "SN");
dico.Add("Serbia", "RS");
dico.Add("Seychelles", "SC");
dico.Add("Sierra Leone", "SL");
dico.Add("Singapore", "SG");
dico.Add("Slovakia", "SK");
dico.Add("Slovenia", "SI");
dico.Add("Solomon Islands", "SB");
dico.Add("Somalia", "SO");
dico.Add("South Africa", "ZA");
dico.Add("South Sudan", "SS");
dico.Add("Spain", "ES");
dico.Add("Sri Lanka", "LK");
dico.Add("Sudan", "SD");
dico.Add("Suriname", "SR");
dico.Add("Svalbard", "SJ");
dico.Add("Swaziland", "SZ");
dico.Add("Sweden", "SE");
dico.Add("Switzerland", "CH");
dico.Add("Syria", "SY");
dico.Add("Taiwan", "TW");
dico.Add("Tajikistan", "TJ");
dico.Add("Tanzania", "TZ");
dico.Add("Thailand", "TH");
dico.Add("Timor-Leste", "TL");
dico.Add("Togo", "TG");
dico.Add("Tokelau", "TK");
dico.Add("Tonga", "TO");
dico.Add("Trinidad and Tobago", "TT");
dico.Add("Tunisia", "TN");
dico.Add("Turkey", "TR");
dico.Add("Turkmenistan", "TM");
dico.Add("Turks and Caicos Islands", "TC");
dico.Add("Tuvalu", "TV");
dico.Add("U.S. Virgin Islands", "VI");
dico.Add("U.S. Minor Outlying Islands", "UM");
dico.Add("Uganda", "UG");
dico.Add("Ukraine", "UA");
dico.Add("United Arab Emirates", "AE");
dico.Add("United Kingdom", "GB");
dico.Add("United States", "US");
dico.Add("Uruguay", "UY");
dico.Add("Uzbekistan", "UZ");
dico.Add("Vanuatu", "VU");
dico.Add("Vatican City", "VA");
dico.Add("Venezuela", "VE");
dico.Add("Vietnam", "VN");
dico.Add("Wallis and Futuna", "WF");
dico.Add("Afghanistan", "AF");
dico.Add("British Indian Ocean Territory", "IO");
dico.Add("French Guiana", "GF");
dico.Add("Georgia", "GE");
dico.Add("Guernsey", "GG");
dico.Add("Kazakhstan", "KZ");
dico.Add("Marshall Islands", "MH");
dico.Add("Norfolk Island", "NF");
dico.Add("Pakistan", "PK");
dico.Add("Saint Kitts and Nevis", "KN");
dico.Add("South Georgia and the South Sandwich Islands", "GS");
dico.Add("Western Sahara", "EH");
dico.Add("Yemen", "YE");
dico.Add("Zambia", "ZM");
dico.Add("Zimbabwe", "ZW");
        }

        public static string RandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringChars = new char[length];
            for (int i = 0; i < length; i++) stringChars[i] = chars[_random.Next(chars.Length)];
            return new string(stringChars);
        }


    }
}
