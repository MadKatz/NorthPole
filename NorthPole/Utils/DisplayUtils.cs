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
        private const String INCOMPLETE = "Incomplete";
        private const String COMPLETE = "Complete";

        public static String GetEndStatusString(AccountContext accountContext)
        {
            StringBuilder sb = new StringBuilder();
            AccountCredits aCredits = accountContext.AccountCredits;
            sb.AppendLine(accountContext.Email + " - " + COMPLETE);
            sb.AppendLine("Credits: " + aCredits.CurrentCredits);
            sb.AppendLine("Offer Status: " + GetStatusString(aCredits.OfferCredits, aCredits.OfferMaxCredits));
            sb.AppendLine("Desktop Status: " + GetStatusString(aCredits.PCSearchCredits, aCredits.PCSearchMaxCredits));
            sb.AppendLine("Mobile Status: " + GetStatusString(aCredits.MobileSearchCredits, aCredits.MobileSearchMaxCredits) + "\n");
            return sb.ToString();
        }

        private static String GetStatusString(int currentCredits, int maxCredits)
        {
            String completedString = null;
            if (maxCredits == -1)
            {
                completedString = INCOMPLETE;
            }
            else
            {
                completedString = currentCredits == maxCredits ? COMPLETE : INCOMPLETE;
            }
            String result  = currentCredits + " / " + maxCredits + " - " + completedString;
            return result;
        }
    }

}
