using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinon.Core.Models
{
    public class PushChannel
    {
        public int Id { get; set; }

        public string Type { get; set; }

        public string Uri { get; set; }

        public PushChannel()
        {
            Type = "MPNS";
        }
    }
}
