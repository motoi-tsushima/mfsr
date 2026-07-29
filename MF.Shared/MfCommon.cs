using System;
using System.Collections.Generic;
using System.Text;
using SnowStack.EncodingProbe;

namespace MF.Shared
{
    public static class MfCommon
    {
        public enum HelpCommandType { Mfsr, MfProbe }

        /// <summary>
        /// 文字エンコーディングの自動判定モードタイプ
        /// </summary>
        public enum EncodingDetectionType
        {
            Normal = 0,
            FirstParty = 1,
            ThirdParty = 3
        }

        /// <summary>
        /// 文字エンコーディングの自動判定モードを EncodingProbe の判定戦略 (DetectionStrategy) に変換する
        /// </summary>
        /// <param name="detectionMode">自動判定モード</param>
        /// <returns>対応する判定戦略</returns>
        public static DetectionStrategy ToDetectionStrategy(EncodingDetectionType detectionMode)
        {
            switch (detectionMode)
            {
                case EncodingDetectionType.FirstParty:
                    return DetectionStrategy.NativeOnly;
                case EncodingDetectionType.ThirdParty:
                    return DetectionStrategy.UtfUnknownOnly;
                case EncodingDetectionType.Normal:
                default:
                    return DetectionStrategy.Combined;
            }
        }
    }
}
