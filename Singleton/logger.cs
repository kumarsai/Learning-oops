namespace Singleton
{
    internal class Logger
    {
        private static readonly Lazy<Logger> logger = new Lazy<Logger>(() => new Logger());

        private static int counter = 0;

        private Logger()
        {
            counter++;
            Console.WriteLine("Logger instance created. Count: " + counter);
        }

        public static Logger Instance
        {
            get
            {
                //if (logger == null)
                //{
                //    logger = new Logger();
                //}
                //return logger;
                return logger.Value;
            }
        }
        public int Level { get; private set; }


        //public static Logger Getinstance()
        //{
        //    if (logger == null)
        //    {
        //        logger = new Logger();
        //    }

        //    return logger;
        //}

        public int Counter()
        {
            return Level++;
        }
    }
}

