using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Shared.Helpers
{
    public static class FunctionsHelper
    {
        // 🔐 Encrypt
        public static string Encrypt(string plainText, string passPhrase)
        {
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;

                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    using (var memoryStream = new MemoryStream())
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                        cryptoStream.FlushFinalBlock();

                        var cipherTextBytes = saltStringBytes
                            .Concat(ivStringBytes)
                            .Concat(memoryStream.ToArray())
                            .ToArray();

                        return Convert.ToBase64String(cipherTextBytes);
                    }
                }
            }
        }

        // 🔐 Decrypt
        public static string Decrypt(string cipherText, string passPhrase)
        {
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;

                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    using (var memoryStream = new MemoryStream(cipherTextBytes))
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        var plainTextBytes = new byte[cipherTextBytes.Length];
                        var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                        return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }

        // 📂 ذخیره فایل
        public static string SaveFile(IFormFile file, string fullFolderPath, bool alterName)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            if (!Directory.Exists(fullFolderPath))
                Directory.CreateDirectory(fullFolderPath);

            var fileName = alterName
                ? $"{Path.GetRandomFileName()}{Path.GetExtension(file.FileName)}"
                : Path.GetFileName(file.FileName);

            var filePath = Path.Combine(fullFolderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return filePath;
        }

        // 🗑 حذف فایل
        public static bool DeleteFileByPath(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return false;

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }

            return false;
        }

        // ✂️ کوتاه‌سازی متن
        public static string Truncate(string input, int length)
        {
            if (input.Length <= length)
                return input;

            return input.Substring(0, length) + "...";
        }

        // 🕒 تبدیل Unix ↔ DateTime
        public static DateTime UnixTimeToDateTime(long unixtime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return sTime.AddSeconds(unixtime);
        }

        public static long ConvertToUnixTime(DateTime datetime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(datetime - sTime).TotalSeconds;
        }

        // 📅 تاریخ فارسی
        public static string GetPersianDateTime(DateTime dateTime, bool isLongTime, bool isPersianMonth)
        {
            PersianCalendar persianCalendar = new PersianCalendar();
            int year = persianCalendar.GetYear(dateTime);
            int month = persianCalendar.GetMonth(dateTime);
            int day = persianCalendar.GetDayOfMonth(dateTime);
            int hour = persianCalendar.GetHour(dateTime);
            int minute = persianCalendar.GetMinute(dateTime);
            int second = persianCalendar.GetSecond(dateTime);

            if (isPersianMonth)
            {
                string[] months = { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
                string persianMonth = months[month - 1];
                return isLongTime
                    ? $"{day} {persianMonth} {year} - {hour}:{minute}:{second}"
                    : $"{day} {persianMonth} {year}";
            }

            return isLongTime
                ? $"{year}/{month}/{day} - {hour}:{minute}:{second}"
                : $"{year}/{month}/{day}";
        }

        public static DateTime ConvertToGregorian(string date)
        {
            int[] StartDateToArray = date.Split('/').Select(id => Convert.ToInt32(id)).ToArray();
            int year = StartDateToArray[0];
            int month = StartDateToArray[1];
            int day = StartDateToArray[2];
            DateTime dt = new DateTime(year, month, day, new PersianCalendar());
            return dt;
        }

        // 🕒 محاسبه فاصله زمانی
        public static string CalculatePassedTime(this DateTime dateTime)
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            var ts = new TimeSpan(DateTime.Now.Ticks - dateTime.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return ts.Seconds == 1 ? "لحظه ای قبل" : ts.Seconds + " ثانیه قبل";

            if (delta < 2 * MINUTE)
                return "یک دقیقه قبل";

            if (delta < 45 * MINUTE)
                return ts.Minutes + " دقیقه قبل";

            if (delta < 90 * MINUTE)
                return "یک ساعت قبل";

            if (delta < 24 * HOUR)
                return ts.Hours + " ساعت قبل";

            if (delta < 48 * HOUR)
                return "دیروز";

            if (delta < 30 * DAY)
                return ts.Days + " روز قبل";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "یک ماه قبل" : months + " ماه قبل";
            }

            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "یک سال قبل" : years + " سال قبل";
        }

        // 💰 تبدیل به ریال فارسی
        public static string GetPersianCurrency(string value)
        {
            try
            {
                CultureInfo persianCulture = new CultureInfo("fa-IR");
                persianCulture.NumberFormat.CurrencyPositivePattern = 3;
                persianCulture.NumberFormat.CurrencyNegativePattern = 3;
                persianCulture.NumberFormat.CurrencySymbol = "ریال";
                long longValue = Convert.ToInt64(value);
                string persianCurrency = longValue.ToString("C0", persianCulture);
                return persianCurrency;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 📝 تبدیل متن به SefUrl
        public static string SerializeSefUrl(string text)
        {
            string serializedText = text.Trim()
                .Replace("/", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("+", "")
                .Replace(".", "")
                .Replace("?", "")
                .Replace("#", "")
                .Replace(" ", "-");

            while (serializedText.Contains("--"))
                serializedText = serializedText.Replace("--", "-");

            return serializedText;
        }

        // 🔢 تبدیل به عدد فارسی
        public static string ToPersianNumber(string input)
        {
            string[] persian = { "۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹" };

            for (int j = 0; j < persian.Length; j++)
                input = input.Replace(j.ToString(), persian[j]);

            return input;
        }


        private const int Keysize = 256;
        private const int DerivationIterations = 1000;
    }
}
