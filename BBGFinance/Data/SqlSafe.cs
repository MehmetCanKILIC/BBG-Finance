namespace BBGFinance.Data
{
    /// <summary>
    /// JP_ROIBEDS'te birçok kolon (tutar/adet alanları ve bazı ID'ler) VARCHAR olarak
    /// tutulduğundan, hem toplama/karşılaştırma hem de tablolar arası JOIN yaparken tip
    /// uyuşmazlığı hatalarını önlemek için kullanılan yardımcı SQL ifadeleri.
    /// </summary>
    internal static class SqlSafe
    {
        /// <summary>VARCHAR sayısal bir alanı güvenli biçimde FLOAT'a çevirir (boş/sayı
        /// olmayan değer hata yerine NULL olur).</summary>
        public static string Num(string kolonIfadesi)
        {
            return "TRY_CONVERT(FLOAT, NULLIF(LTRIM(RTRIM(" + kolonIfadesi + ")), ''))";
        }

        /// <summary>Bir ifadeyi trim'lenmiş NVARCHAR'a çevirir - JOIN/karşılaştırmalarda
        /// iki tarafın gerçek tipi (INT/VARCHAR) farklı olsa da güvenli eşitlik sağlar.</summary>
        public static string Txt(string kolonIfadesi)
        {
            return "LTRIM(RTRIM(CONVERT(NVARCHAR(100), " + kolonIfadesi + ")))";
        }

        /// <summary>İki kolon/ifadeyi tip farkı gözetmeksizin metin bazında eşitler.</summary>
        public static string JoinEq(string a, string b)
        {
            return Txt(a) + " = " + Txt(b);
        }
    }
}
