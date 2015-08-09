using NorthPole.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthPole.Utils
{
    public static class DisplayUtils
    {
        public static String GetEndStatusString(AccountContext accountContext)
        {
            StringBuilder sb = new StringBuilder();
            AccountCredits aCredits = accountContext.AccountCredits;
            sb.AppendLine(accountContext.Email + " - Complete");
            sb.AppendLine("Credits: " + aCredits.CurrentCredits);
            sb.AppendLine("Offer Status: " + aCredits.OfferCredits + " / " + aCredits.OfferMaxCredits + " - " + getCompletedString(aCredits.OfferCredits));
            sb.AppendLine("Desktop Status: " + aCredits.PCSearchCredits + " / " + aCredits.PCSearchMaxCredits + " - " + getCompletedString(aCredits.OfferCredits));
            sb.AppendLine("Mobile Status: " + aCredits.MobileSearchCredits + " / " + aCredits.MobileSearchMaxCredits + " - " + getCompletedString(aCredits.OfferCredits) + "\n");
            return sb.ToString();
        }

        private static String getCompletedString(int credit)
        {
            if (credit == -1)
            {
                return "Incomplete";
            }
            else
            {
                return "Complete";
            }
        }
    }

}
