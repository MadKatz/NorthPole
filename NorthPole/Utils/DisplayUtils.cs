using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthPole.Utils
{
    public static class DisplayUtils
    {
        /// <summary>
        /// Returns a empty string.
        /// </summary>
        /// <param name="length">number of empty spaces.</param>
        /// <returns></returns>
        private static string GetEmptyString(int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(" ");
            }
            return sb.ToString();
        }
        /// <summary>
        /// Returns centered string with empty spaces around it.
        /// </summary>
        /// <param name="num_spaces_front">number of spaces in front of the string.</param>
        /// <param name="num_spaces_back">number of spaces behind the string.</param>
        /// <param name="str">the string to center.</param>
        /// <returns></returns>
        public static string GetCenteredString(int num_spaces_front, int num_spaces_back, string str)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetEmptyString(num_spaces_front));
            sb.Append(str);
            sb.Append(GetEmptyString(num_spaces_back));
            return sb.ToString();
        }

        public static String GetAccountStatString(AccountInfo accountInfo, int maxStrLength)
        {
            StringBuilder sb = new StringBuilder();
            int currentRP_string_count = 14;
            int PC_string_count = 9;
            int mobile_string_count = 9;
            int offer_string_count = 10;
            // Set Account Name
            sb.Append("# " + accountInfo.AccountName);
            int maxlength = maxStrLength;
            maxlength = maxlength - accountInfo.AccountName.Length;
            sb.Append(GetEmptyString(maxlength));
            sb.Append(" :");
            // Set Current RP 
            int temp = currentRP_string_count - accountInfo.Current_RP.ToString().Length;
            int num_of_whitespaces = temp / 2;
            int extra = temp % 2;
            sb.Append(GetCenteredString(num_of_whitespaces, num_of_whitespaces + extra, accountInfo.Current_RP.ToString()));
            sb.Append(":");
            // Set PC
            temp = PC_string_count - accountInfo.GetPC_String().Length;
            num_of_whitespaces = temp / 2;
            extra = temp % 2;
            sb.Append(GetCenteredString(num_of_whitespaces, num_of_whitespaces + extra, accountInfo.GetPC_String()));
            sb.Append(":");
            // Set Mobile
            temp = mobile_string_count - accountInfo.GetMobile_String().Length;
            num_of_whitespaces = temp / 2;
            extra = temp % 2;
            sb.Append(GetCenteredString(num_of_whitespaces, num_of_whitespaces + extra, accountInfo.GetMobile_String()));
            sb.Append(":");
            // Set Offer
            temp = offer_string_count - accountInfo.GetOffer_String().Length;
            num_of_whitespaces = temp / 2;
            extra = temp % 2;
            sb.Append(GetCenteredString(num_of_whitespaces, num_of_whitespaces + extra, accountInfo.GetOffer_String()));
            sb.Append(":");
            // Set Total
            sb.Append(GetEmptyString(2));
            sb.Append(accountInfo.GetTotal_String());

            return sb.ToString();
        }

         /// <summary>
        /// Returns longest account name from the account dictionary.
        /// </summary>
        /// <returns></returns>
        public static int GetMaxAccountStrLength(IEnumerable<KeyValuePair<String, String>> collection)
        {
            int maxlength = 0;
            foreach (var account in collection)
            {
                if (account.Key.Length > maxlength)
                {
                    maxlength = account.Key.Length;
                }
            }
            return maxlength;
        }

        public static String GetStatHeaderString(int maxStringLength)
        {
            string account_String = "Account";
            StringBuilder sb = new StringBuilder();
            sb.Append("# ");

            int maxlength = maxStringLength;
            maxlength = maxlength - account_String.Length;

            for (int i = 0; i < (maxlength / 2) - 1; i++)
            {
                sb.Append(" ");
            }
            sb.Append(account_String);
            for (int i = 0; i < (maxlength / 2) + 1; i++)
            {
                sb.Append(" ");
            }
            sb.Append(" :  Current RP  :   PC    :  Mobile :  Offers  :  Total  ");
            return sb.ToString();
        }
    }

}
