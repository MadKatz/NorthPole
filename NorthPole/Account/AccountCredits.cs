using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthPole.Account
{
    public class AccountCredits
    {
        private int currentCredits;

        public int CurrentCredits
        {
            get { return currentCredits; }
            set { currentCredits = value; }
        }
        private int pcSearchCredits;

        public int PCSearchCredits
        {
            get { return pcSearchCredits; }
            set { pcSearchCredits = value; }
        }
        private int pcSearchMaxCredits;

        public int PCSearchMaxCredits
        {
            get { return pcSearchMaxCredits; }
            set { pcSearchMaxCredits = value; }
        }
        private int mobileSearchCredits;

        public int MobileSearchCredits
        {
            get { return mobileSearchCredits; }
            set { mobileSearchCredits = value; }
        }
        private int mobileSearchMaxCredits;

        public int MobileSearchMaxCredits
        {
            get { return mobileSearchMaxCredits; }
            set { mobileSearchMaxCredits = value; }
        }

        private int offerCredits;

        public int OfferCredits
        {
            get { return offerCredits; }
            set { offerCredits = value; }
        }
        private int offerMaxCredits;

        public int OfferMaxCredits
        {
            get { return offerMaxCredits; }
            set { offerMaxCredits = value; }
        }

        public AccountCredits()
        {
            CurrentCredits = -1;
            MobileSearchMaxCredits = -1;
            MobileSearchCredits = -1;
            PCSearchCredits = -1;
            PCSearchMaxCredits = -1;
        }
    }
}
