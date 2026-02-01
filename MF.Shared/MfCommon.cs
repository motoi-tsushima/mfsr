using System;
using System.Collections.Generic;
using System.Text;

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


    }
}
