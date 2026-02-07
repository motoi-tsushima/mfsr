using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MF.Shared
{

    public class Help
    {
        private string[] _helpMessage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Help(MfCommon.HelpCommandType helpCommandType)
        {
            // カルチャー情報に基づいて適切な言語のヘルプメッセージを取得
            string languageCode = HelpMessages.GetLanguageCode();
            if(helpCommandType == MfCommon.HelpCommandType.Mfsr)
                this._helpMessage = HelpMessages.GetRmsmfHelpMessage(languageCode);
            else if(helpCommandType == MfCommon.HelpCommandType.MfProbe)
                this._helpMessage = HelpMessages.GetTxprobeHelpMessage(languageCode);
            else
                this._helpMessage = HelpMessages.GetRmsmfHelpMessage("en");
        }

        /// <summary>
        /// ヘルプを表示する
        /// </summary>
        public void Show()
        {
            foreach (string message in this._helpMessage)
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// 使用可能なカルチャー情報の一覧を表示する
        /// </summary>
        public void ShowAvailableCultures()
        {
            Console.WriteLine("Available Culture Information for /ci: option:");
            Console.WriteLine("==============================================");
            Console.WriteLine();

            // まず、ヘルプメッセージで言及されている重要なカルチャーを表示
            Console.WriteLine("Commonly used cultures (as mentioned in help):");
            Console.WriteLine("  en-US      - English (United States)");
            Console.WriteLine("  ja-JP      - Japanese (Japan)");
            Console.WriteLine("  ko-KR      - Korean (Korea)");
            Console.WriteLine("  zh-CN      - Chinese (Simplified, PRC)");
            Console.WriteLine("  zh-Hans    - Chinese (Simplified)");
            Console.WriteLine("  zh-SG      - Chinese (Simplified, Singapore)");
            Console.WriteLine("  zh-TW      - Chinese (Traditional, Taiwan)");
            Console.WriteLine("  zh-Hant    - Chinese (Traditional)");
            Console.WriteLine("  zh-HK      - Chinese (Traditional, Hong Kong SAR)");
            Console.WriteLine("  zh-MO      - Chinese (Traditional, Macao SAR)");
            Console.WriteLine();

            // すべてのカルチャーを取得（.NET 10対応）
            // SpecificCultures と NeutralCultures を明示的に取得
            CultureInfo[] specificCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            CultureInfo[] neutralCultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
            
            // 重要なカルチャーが実際に存在するか確認
            var importantCultureNames = new[] { "zh-CN", "zh-Hans", "zh-SG", "zh-TW", "zh-Hant", "zh-HK", "zh-MO", "ja-JP", "ko-KR", "en-US" };
            var missingCultures = new List<string>();
            var foundCultures = new List<string>();
            
            foreach (var cultureName in importantCultureNames)
            {
                bool foundInSpecific = specificCultures.Any(c => c.Name.Equals(cultureName, StringComparison.OrdinalIgnoreCase));
                bool foundInNeutral = neutralCultures.Any(c => c.Name.Equals(cultureName, StringComparison.OrdinalIgnoreCase));
                
                if (foundInSpecific || foundInNeutral)
                {
                    foundCultures.Add(cultureName);
                }
                else
                {
                    // .NET 10では名前が変わっている可能性があるので、類似のものを探す
                    var similar = specificCultures.Concat(neutralCultures)
                        .Where(c => c.Name.Contains(cultureName.Split('-')[0], StringComparison.OrdinalIgnoreCase))
                        .Select(c => c.Name)
                        .Distinct()
                        .ToList();
                    
                    if (similar.Any())
                    {
                        missingCultures.Add($"{cultureName} (alternatives: {string.Join(", ", similar)})");
                    }
                    else
                    {
                        missingCultures.Add(cultureName);
                    }
                }
            }

#if DEBUG
            // デバッグ情報: 重要なカルチャーが見つからなかった場合
            if (missingCultures.Any())
            {
                Console.WriteLine("[DEBUG] Some important cultures were not found in GetCultures():");
                foreach (var missing in missingCultures)
                {
                    Console.WriteLine($"  - {missing}");
                }
                Console.WriteLine();
            }
#endif

            // すべてのカルチャーを取得して表示
            CultureInfo[] allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            var sortedCultures = allCultures
                .Where(c => !string.IsNullOrEmpty(c.Name))
                .OrderBy(c => c.Name)
                .ToList();

            Console.WriteLine($"All available cultures (Total: {sortedCultures.Count}):");
            Console.WriteLine($"  - Specific cultures: {specificCultures.Length}");
            Console.WriteLine($"  - Neutral cultures: {neutralCultures.Length}");
            Console.WriteLine("(You can use any of these culture names with /ci: option)");
            Console.WriteLine();

            // 中国語関連のカルチャーを強調表示
            var chineseCultures = sortedCultures
                .Where(c => c.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            if (chineseCultures.Any())
            {
                Console.WriteLine($"Chinese-related cultures ({chineseCultures.Count}):");
                foreach (var culture in chineseCultures.Take(20)) // 最初の20件のみ表示
                {
                    string typeInfo = specificCultures.Contains(culture) ? "[Specific]" : 
                                     neutralCultures.Contains(culture) ? "[Neutral]" : "[Other]";
                    Console.WriteLine($"  {culture.Name,-15} {typeInfo,-12} - {culture.DisplayName}");
                }
                if (chineseCultures.Count > 20)
                {
                    Console.WriteLine($"  ... and {chineseCultures.Count - 20} more");
                }
                Console.WriteLine();
            }

            // Display all cultures in 4 columns
            Console.WriteLine("All cultures:");
            int columns = 4;
            int rows = (sortedCultures.Count + columns - 1) / columns;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    int index = row + col * rows;
                    if (index < sortedCultures.Count)
                    {
                        Console.Write(sortedCultures[index].Name.PadRight(20));
                    }
                }
                Console.WriteLine();
            }
            
            Console.WriteLine();
            Console.WriteLine("Note: You can specify culture names like 'zh-CN', 'zh-Hans', 'zh-TW', etc.");
            Console.WriteLine("      Both specific cultures (e.g., zh-CN) and neutral cultures (e.g., zh-Hans) are supported.");
            Console.WriteLine("      If a culture name is not listed above but is valid, you can still use it with /ci: option.");
        }

        /// <summary>
        /// 使用可能なエンコーディング情報の一覧を表示する
        /// </summary>
        public void ShowAvailableEncodings()
        {
            Console.WriteLine("Available Encodings for /c: and /w: options:");
            Console.WriteLine("=============================================");
            Console.WriteLine();

            EncodingInfo[] encodings = Encoding.GetEncodings();
            var sortedEncodings = encodings.OrderBy(e => e.Name).ToList();

            // Display in 3 columns with encoding name and code page
            int columns = 3;
            int rows = (sortedEncodings.Count + columns - 1) / columns;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    int index = row + col * rows;
                    if (index < sortedEncodings.Count)
                    {
                        EncodingInfo enc = sortedEncodings[index];
                        string displayText = $"{enc.Name} ({enc.CodePage})";
                        Console.Write(displayText.PadRight(30));
                    }
                }
                Console.WriteLine();
            }
        }

    }
}
