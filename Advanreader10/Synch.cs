using System;
using ThingMagic;


namespace Synch
{
    class Synch
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
            const int readTime = 10000;   //miliseconds

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

                    //use the antenna #1 by default, use GEN2 protocol, don't use any filter
                    SimpleReadPlan plan = new SimpleReadPlan(antennaList, TagProtocol.GEN2, null, null, 1000);
                    reader.ParamSet("/reader/read/plan", plan);

                    TagReadData[] tagsRead = reader.Read(readTime);

                    for (int i = 0; i < tagsRead.Length; i++) Console.WriteLine(tagsRead[i]);

                    //release resources that the API has acquired (serial device, network connection...)
                    reader.Destroy();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

        }
    }
}
