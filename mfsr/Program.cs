using System;
using System.Reflection;
using System.Text;
using MF.Shared;

/// <summary>
/// I replace multiple strings in multiple files.
/// 複数のファイルの複数の文字列を置き換えます。
/// </summary>
namespace mfsr
{
    /// <summary>
    /// RMSMF (Replace Multiple Strings in Multiple Files)
    /// 複数のファイルの複数の文字列を置き換える。
    /// </summary>
    class Program
    {
        /// <summary>
        /// Multiple word multiple file replacement
        /// 複数の単語の複数のファイルの置換。
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // System.Text.Encoding.CodePagesパッケージのエンコーディングプロバイダーを登録
            // これにより、EUC-KR (51949), Shift_JIS (932), GB18030など
            // .NET Core/.NET 5+ でデフォルトでサポートされていないエンコーディングが使用可能になる
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // バージョン情報の表示
            Assembly thisAssem = typeof(Program).Assembly;
            AssemblyName thisAssemName = thisAssem.GetName();
            
            Version ver = thisAssemName.Version ?? new Version(1, 0, 0, 0);
            String copyright = "";
            
            // Native AOT 対応: GetCustomAttributes が空の配列を返す可能性があるため安全にアクセス
            var copyrightAttributes = thisAssem.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (copyrightAttributes != null && copyrightAttributes.Length > 0)
            {
                copyright = ((AssemblyCopyrightAttribute)copyrightAttributes[0]).Copyright;
            }

            CommandOptions commandOptions = null;


            try
            {
                // 引数なしの場合は簡易バージョン表示
                if (args.Length == 0)
                {
                    VersionWriter.WriteVersion(false, thisAssemName.Name, ver, copyright);
#if DEBUG
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
#endif
                    return;
                }

                // コマンドオプション取得（カルチャー設定、ヘルプ・バージョン表示を含む）
                commandOptions = new CommandOptions(args);

                // ヘルプまたはバージョンが表示された場合は終了
                if (commandOptions.HelpOrVersionDisplayed)
                {
#if DEBUG
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
#endif
                    return;
                }

                commandOptions.ReadReplaceWords();
                commandOptions.ReadFileNameList();

                // ファイルの文字列置換処理の実行
                ReplaceStringsInFiles replace = new ReplaceStringsInFiles(commandOptions.ReplaceWords, commandOptions.Files, commandOptions.EnableBOM);

                replace.Replace(commandOptions.ReadEncoding, commandOptions.WriteEncoding, commandOptions.WriteNewLine);

                // 正常に処理を完了した
                Console.WriteLine(ValidationMessages.ProcessingSuccessful);
            }
            catch (RmsmfException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(ValidationMessages.UnexpectedErrorOccurred, ex.Message));
                Console.WriteLine(ex.ToString());
                return;
            }
        }
    }
}
