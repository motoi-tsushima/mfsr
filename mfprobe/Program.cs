using System;
using System.Reflection;
using System.Text;
using MF.Shared;

/// <summary>
/// mfprobe (Multi-File Probe)
/// 複数のファイルの以下の内容を確認します。
/// (1) BOMの有無
/// (2) 改行コードの種類（CRLF、LF、CR）
/// (3) 文字コード（UTF-8、UTF-16LE、UTF-16BE、Shift_JIS, JIS, EUC-JP, ASCIIなど） 
/// </summary>
namespace mfprobe
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // System.Text.Encoding.CodePagesパッケージのエンコーディングプロバイダーを登録
            // これにより、EUC-KR (51949), Shift_JIS (932), GB18030など
            // .NET Core/.NET 5+ でデフォルトでサポートされていないエンコーディングが使用可能になる
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //Show version
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

                //コマンドオプション取得（カルチャー設定、ヘルプ・バージョン表示を含む）
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

                commandOptions.ReadSearchWords();
                commandOptions.ReadFileNameList();

                //ファイルのプローブ実行
                ProbeFiles probe = new ProbeFiles(
                    commandOptions.SearchWords, commandOptions.Files,
                    commandOptions.EnableProbe, commandOptions.OutputFileNameListFileName,
                    commandOptions.FilesEncoding);

                probe.Probe(commandOptions.ReadEncoding);

                //正常に処理を完了した。
                Console.WriteLine("");
                Console.WriteLine(ValidationMessages.SearchComplete);
            }
            catch (RmsmfException ex)
            {
                Console.WriteLine(ex.Message);
#if DEBUG
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
#endif
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(ValidationMessages.UnexpectedErrorOccurred, ex.Message));
                Console.WriteLine(ex.ToString());
#if DEBUG
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
#endif
                return;
            }

#if DEBUG
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
#endif
        }
    }
}
