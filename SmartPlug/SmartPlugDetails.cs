using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contrequarte.SmartPlug.Core
{
    public class SmartPlugDetails
    {
        public string Model { get; private set; }
        public string SoftwareVersion  { get; private set; }
        public string Name  { get; private set; }

        public SmartPlugDetails(string model, string softwareVersion, string name)
        {
            Model = model;
            SoftwareVersion = softwareVersion;
            Name = name;
        }
    }
}
