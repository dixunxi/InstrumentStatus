using InstrumentStatusService.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstrumentStatusTest
{
    [TestClass]
    public class InstrumentStatusController_Test
    {
        [TestMethod]
        public void FileModified_Test()
        {
            using (InstrumentStatusController controller = new InstrumentStatusController())
            {
                FileInfo fileInfo = new FileInfo(@"C:\temp\test123.csv");

                controller.FileModified(controller.GetDirectories().Where(i => i.instrument_id == controller.GetInstrument(Environment.MachineName).id).FirstOrDefault().id, fileInfo);
            }
        }
    }
}
