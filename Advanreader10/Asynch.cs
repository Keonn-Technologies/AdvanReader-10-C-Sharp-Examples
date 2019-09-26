using System;
using System.Threading;
using ThingMagic;


namespace Asynch
{
    class Asynch
    {
        static void Usage()
        {
            Console.WriteLine(String.Join("\r\n", new string[]
            {
                "Usage:  scheme://authority/path",
                "tmr:///com1 or",
                "eapi:///com1 or",
                "eapi:///dev/ttyUSB0 for Linux"
            }));
            Environment.Exit(1);
        }

        static void Main(string[] args)
        {
            //ensure the serial port is specified
            if (args.Length < 1)
                Usage();

            string URI = args[0];
            const int sleepTime = 10000;  //miliseconds

            try
            {

                /* 
                 * The using statement calls the Dispose method on the object and it also causes the object to go out 
                 * of scope as soon as Dispose is called. 
                 */
                using (Reader reader = Reader.Create(URI))
                {
                    reader.Connect();

                    //The region of operation should be set
                    Reader.Region[] readerSupportedRegions = (Reader.Region[])reader.ParamGet("/reader/region/supportedRegions");
                    if (readerSupportedRegions.Length < 1)
                        throw new FAULT_INVALID_REGION_Exception();

                    //set region to EU3
                    reader.ParamSet("/reader/region/id", readerSupportedRegions[5]);

                    int[] antennaList = { 1 };
              
                    // uncomment the following line to check detected antennas 
                    //int[] antennaList = (int[])reader.ParamGet("/reader/antenna/portList");

                    //use the antenna #1 by default, use GEN2 protocol, don't use any filter
                    SimpleReadPlan plan = new SimpleReadPlan(antennaList, TagProtocol.GEN2, null, null, 1000);
                    reader.ParamSet("/reader/read/plan", plan);

                    //tag listener
                    reader.TagRead += delegate (Object sender, TagReadDataEventArgs e)
                    {
                        Console.WriteLine("Reading: " + e.TagReadData);
                    };

                    //exception listener
                    reader.ReadException += new EventHandler<ReaderExceptionEventArgs>(RException);

                    //read asyncronously
                    reader.StartReading();

                    Thread.Sleep(sleepTime);

                    //reads are repeated until the stopReading() method is called.
                    reader.StopReading();

                    //release resources that the API has acquired (serial device, network connection...)
                    reader.Destroy();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

        }

        private static void RException(object sender, ReaderExceptionEventArgs e)
        {
            Console.WriteLine("Error: " + e.ReaderException.Message);
        }

        private static void Reader_TagRead(object sender, TagReadDataEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
