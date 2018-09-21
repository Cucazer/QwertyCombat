using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qwerty
{
    class SoundEventArgs: EventArgs
    {
        public Stream AudioStream { get; }

        public SoundEventArgs(Stream audioStream)
        {
            this.AudioStream = audioStream;
        }
    }
}
