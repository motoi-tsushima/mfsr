using System;
using System.IO;
using System.Text;
using SnowStack.EncodingProbe;

namespace MF.Shared
{
    /// <summary>
    /// エンコーディング判定結果
    /// </summary>
    public class EncodingDetectionResult
    {
        /// <summary>判定されたエンコーディング（判定不可の場合はnull）</summary>
        public Encoding Encoding { get; set; }

        /// <summary>BOMの有無</summary>
        public bool BomExists { get; set; }

        /// <summary>コードページ</summary>
        public int CodePage { get; set; }

        /// <summary>エンコーディング判定情報</summary>
        public EncodingInformation EncodingInfo { get; set; }
    }

    /// <summary>
    /// エンコーディング判定ヘルパークラス
    /// ファイルのエンコーディング判定ロジックを共通化
    /// </summary>
    public static class EncodingHelper
    {
        /// <summary>
        /// ファイルストリームからエンコーディングを判定または指定されたエンコーディングを使用
        /// </summary>
        /// <param name="fs">ファイルストリーム</param>
        /// <param name="fileName">ファイル名（エラーメッセージ用）</param>
        /// <param name="specifiedEncoding">指定されたエンコーディング（nullの場合は自動判定）</param>
        /// <param name="detectionMode">自動判定モード</param>
        /// <param name="culture">自動判定に使用するカルチャー名（例: "ja-JP"）。null の場合は現在のカルチャーを使用する。</param>
        /// <returns>エンコーディング判定結果</returns>
        public static EncodingDetectionResult DetectOrUseSpecifiedEncoding(
            FileStream fs,
            string fileName,
            Encoding specifiedEncoding,
            MfCommon.EncodingDetectionType detectionMode,
            string culture = null)
        {
            var result = new EncodingDetectionResult();

            // 読み込みエンコーディングの有無で分岐
            if (specifiedEncoding == null)
            {
                // エンコーディング指定が無い場合：自動判定
                DetectEncodingFromFile(fs, fileName, detectionMode, culture, result);
            }
            else
            {
                // エンコーディング指定が有る場合
                UseSpecifiedEncoding(fs, specifiedEncoding, result);
            }

            return result;
        }

        /// <summary>
        /// ファイルから自動的にエンコーディングを判定
        /// </summary>
        private static void DetectEncodingFromFile(
            FileStream fs,
            string fileName,
            MfCommon.EncodingDetectionType detectionMode,
            string culture,
            EncodingDetectionResult result)
        {
            // 読み取りファイルの文字エンコーディングを判定する
            long fileLength = fs.Length;

            // ファイルサイズ検証：2GB以上のファイルはエラー
            if (fileLength > FileConstants.MaxFileSize)
            {
                throw new RmsmfException(string.Format(ValidationMessages.FileTooLarge, fileName));
            }

            int fileSize = (int)fileLength;
            byte[] buffer = new byte[fileSize];
            int readCount = fs.Read(buffer, 0, fileSize);

            // ファイルポジションを先頭に戻す（StreamReaderが正しく読めるようにする）
            fs.Position = 0;

            var encodingInfomation = EncodingProbe.Detect(buffer, new EncodingDetectorOptions
            {
                Strategy = MfCommon.ToDetectionStrategy(detectionMode),
                Culture = culture
            });
            result.BomExists = encodingInfomation.Bom;
            result.CodePage = encodingInfomation.CodePage;
            result.EncodingInfo = encodingInfomation;

            // エンコーディングオブジェクトの作成
            result.Encoding = CreateEncodingFromCodePage(result.CodePage, fileName);
        }

        /// <summary>
        /// 指定されたエンコーディングを使用
        /// </summary>
        private static void UseSpecifiedEncoding(
            FileStream fs,
            Encoding specifiedEncoding,
            EncodingDetectionResult result)
        {
            result.Encoding = specifiedEncoding;

            byte[] bomBuffer = new byte[FileConstants.BomBufferSize];
            for (int i = 0; i < FileConstants.BomBufferSize; i++)
            {
                bomBuffer[i] = 0xFF;
            }
            fs.Read(bomBuffer, 0, FileConstants.BomBufferSize);
            fs.Position = 0;

            ByteOrderMarkDetection bomJudg = new ByteOrderMarkDetection();

            if (bomJudg.IsBOM(bomBuffer))
            {
                result.BomExists = true;
                result.CodePage = specifiedEncoding.CodePage;
            }
            else
            {
                result.BomExists = false;
                result.CodePage = specifiedEncoding.CodePage;
            }
        }

        /// <summary>
        /// コードページからエンコーディングオブジェクトを作成
        /// </summary>
        private static Encoding CreateEncodingFromCodePage(int codePage, string fileName)
        {
            if (codePage <= 0)
            {
                return null;
            }

            try
            {
                return Encoding.GetEncoding(codePage);
            }
            catch (ArgumentException)
            {
                // サポートされていないコードページの場合はnullを設定
                // （例: EUC-TW (51950) は .NET Framework 4.8/4.8.1 でサポートされていない）
                Console.WriteLine($"Warning: Code page {codePage} is not supported. Skipping {fileName}");
                return null;
            }
            catch (NotSupportedException)
            {
                Console.WriteLine($"Warning: Code page {codePage} is not supported. Skipping {fileName}");
                return null;
            }
        }

        /// <summary>
        /// BOM表示文字列を取得
        /// </summary>
        public static string GetBomDisplayString(bool bomExists)
        {
            return bomExists ? "BOM exists" : "No BOM";
        }

        /// <summary>
        /// エンコーディング判定結果の表示行を生成（エンコーディング不明時用）
        /// </summary>
        public static string CreateUnknownEncodingDisplayLine(
            string fileName,
            bool bomExists,
            int codePage)
        {
            string dispBOM = GetBomDisplayString(bomExists);
            string lineBreakType = "EOL Unknown";
            string encodingName = "encoding Unknown";

            // コードページからエンコーディング名を取得を試みる
            if (codePage > 0)
            {
                encodingName = GetEncodingWebName(codePage);
            }

            return $"{fileName}\t,{encodingName}\t,{lineBreakType}\t,{dispBOM}";
        }

        /// <summary>
        /// コードページからエンコーディング名を取得する
        /// </summary>
        /// <param name="codePage">コードページ</param>
        /// <returns>エンコーディング名</returns>
        private static string GetEncodingWebName(int codePage) =>
            codePage switch
            {
                20127 => "us-ascii",
                50220 => "iso-2022-jp",
                50225 => "iso-2022-kr",
                50227 => "x-cp50227",
                50229 => "iso-2022-tw",
                65001 => "utf-8",
                20932 => "euc-jp",
                51936 => "euc-cn",
                51949 => "euc-kr",
                51950 => "euc-tw",
                932 => "shift_jis",
                949 => "cp949",
                936 => "gbk",
                54936 => "gb18030",
                950 => "big5",
                1200 => "utf-16",
                1201 => "unicodeFFFE",
                12000 => "utf-32",
                12001 => "utf-32BE",
                _ => "I do not know.",
            };

    }
}
