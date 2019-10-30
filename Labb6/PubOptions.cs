namespace Labb6
{
    public class PubOptions
    {
        private double _BartenderGlassTiming;
        private double _BartenderPourTiming;
        private double _WaitressClearTiming;
        private double _WaitressPlaceTiming;
        private double _BouncerMinTiming;
        private double _BouncerMaxTiming;
        private double _PatronArriveTiming;
        private double _PatronTableTiming;
        private double _PatronMinDrinkTiming;
        private double _PatronMaxDrinkTiming;

        public double BartenderGlassTiming { get { return _BartenderGlassTiming/Speed; } set { _BartenderGlassTiming = value; } }
        public double BartenderPourTiming { get { return _BartenderPourTiming/Speed; } set { _BartenderPourTiming = value; } }
        public double WaitressClearTiming { get { return _WaitressClearTiming/Speed; } set { _WaitressClearTiming = value; } }
        public double WaitressPlaceTiming { get { return _WaitressPlaceTiming/Speed; } set { _WaitressPlaceTiming = value; } }
        public double BouncerMinTiming { get { return _BouncerMinTiming/Speed; } set { _BouncerMinTiming = value; } }
        public double BouncerMaxTiming { get { return _BouncerMaxTiming/Speed; } set { _BouncerMaxTiming = value; } }
        public double PatronArriveTiming { get { return _PatronArriveTiming/Speed; } set { _PatronArriveTiming = value; } }
        public double PatronTableTiming { get { return _PatronTableTiming/Speed; } set { _PatronTableTiming = value; } }
        public double PatronMinDrinkTiming { get { return _PatronMinDrinkTiming/Speed; } set { _PatronMinDrinkTiming = value; } }
        public double PatronMaxDrinkTiming { get { return _PatronMaxDrinkTiming/Speed; } set { _PatronMaxDrinkTiming = value; } }
        public double NumberOfGlasses { get; internal set; }
        public double MaxNumberOfChairs { get; internal set; }

        public bool BadGuyBouncer { get; internal set; }
        public bool CouplesNight { get; internal set; }
        public double Speed { get; internal set; }

        public PubOptions()
        {
            BartenderGlassTiming = 3000;
            BartenderPourTiming = 3000;
            WaitressClearTiming = 10000;
            WaitressPlaceTiming = 15000;
            BouncerMinTiming = 3000;
            BouncerMaxTiming = 10000;
            PatronArriveTiming = 1000;
            PatronTableTiming = 4000;
            PatronMinDrinkTiming = 20000;
            PatronMaxDrinkTiming = 30000;
            NumberOfGlasses = 8;
            MaxNumberOfChairs = 9;
            BadGuyBouncer = false;
            CouplesNight = false;
            Speed = 1;
        }
    }
}