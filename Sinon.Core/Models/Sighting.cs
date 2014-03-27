using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinon.Core.Models
{
    public class Sighting
    {
        public int Id { get; set; }

        public int StationId { get; set; }

        public int NetworkId { get; set; }

        public string StationName { get; set; }

        public string DateTime { get; set; }

        public int PushChannelId { get; set; }
    }
}
