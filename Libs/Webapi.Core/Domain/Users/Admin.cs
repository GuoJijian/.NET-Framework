using System;

namespace Webapi.Core.Domain
{
    public class Admin : User
    {
        public bool BuildIn { get; set; }
        public DateTime Datetime { get; set; }
        public string Remark { get; set; }
        public string Roles { get; set; }

        public string Telephone { get; set; }

        public string IdNumber { get;set; }

        public string idcFrontUrl { get; set; }

        public string idcBackUrl { get; set; }

        /// <summary>
        /// 小型文本  最大长度为64Unicode字符
        /// </summary>
        public string SText1 { get; set; }

        public string SText2 { get; set; }

        public string SText3 { get; set; }

        /// <summary>
        /// 中型文本  最大长度为512
        /// </summary>
        public string MText1 { get; set; }

        public string MText2 { get; set; }

        public string MText3 { get; set; }

        /// <summary>
        /// 大型文本 最大长度为2048
        /// </summary>
        public string LText1 { get; set; }

        public string LText2 { get; set; }

        public string LText3 { get; set; }

        /// <summary>
        /// 不限
        /// </summary>
        public string UText1 { get; set; }

        public string UText2 { get; set; }

        public string UText3 { get; set; }

        public int Num1 { get; set; }

        public int Num2 { get; set; }

        public int Num3 { get; set; }

        public int Num4 { get; set; }

        public int Num5 { get; set; }

        public long LNum1 { get; set; }

        public long LNum2 { get; set; }

        public long LNum3 { get; set; }

        public float DcmVal1 { get; set; }

        public float DcmVal2 { get; set; }

        public float DcmVal3 { get; set; }

        public bool IsTrue1 { get; set; }

        public bool IsTrue2 { get; set; }

        public bool IsTrue3 { get; set; }

        public DateTime Date1 { get; set; }

        public DateTime Date2 { get; set; }

        public DateTime Date3 { get; set; }

    }
}
