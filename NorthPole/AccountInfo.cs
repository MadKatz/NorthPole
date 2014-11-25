using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthPole
{
    class AccountInfo
    {
        private int current_RP;
        private int current_PC;
        private int max_PC;
        private int current_mobile;
        private int max_mobile;
        private int current_offer;
        private int max_offer;
        private int current_total;
        private int max_total;
        private string accountname;
        private string string_seperator = "/";

        public int Current_RP
        {
            get { return current_RP; }

            set { current_RP = value; }
        }

        public int Current_PC
        {
            get { return current_PC; }

            set { current_PC = value; }
        }

        public int Max_PC
        {
            get { return max_PC; }

            set { max_PC = value; }
        }

        public int Current_Mobile
        {
            get { return current_mobile; }

            set { current_mobile = value; }
        }

        public int Max_Mobile
        {
            get { return max_mobile; }

            set { max_mobile = value; }
        }

        public int Current_Offer
        {
            get { return current_offer; }

            set { current_offer = value; }
        }

        public int Max_Offer
        {
            get { return max_offer; }

            set { max_offer = value; }
        }

        public string AccountName
        {
            get { return accountname; }

            set { accountname = value; }
        }

        public AccountInfo()
        {
            current_RP = -1;
            current_PC = -1;
            max_PC = -1;
            current_mobile = -1;
            max_mobile = -1;
            current_offer = -1;
            max_offer = -1;
            current_total = -1;
            max_total = -1;
            accountname = "accountname_not_set";
        }

        public string GetPC_String()
        {
            return Current_PC + string_seperator + Max_PC;
        }

        public string GetMobile_String()
        {
            return Current_Mobile + string_seperator + Max_Mobile;
        }

        public string GetOffer_String()
        {
            return Current_Offer + string_seperator + Max_Offer;
        }

        public string GetTotal_String()
        {
            SetTotals();
            return current_total + string_seperator + max_total;
        }

        private void SetTotals()
        {
            current_total = Current_PC + Current_Mobile + Current_Offer;
            max_total = Max_PC + Max_Mobile + Max_Offer;
        }
    }
}
