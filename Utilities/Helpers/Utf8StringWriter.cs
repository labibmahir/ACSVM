using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Helpers
{
    public class Utf8StringWriter : StringWriter
    {
        private readonly Encoding stringWriterEncoding;
        public Utf8StringWriter()
            : base()
        {
            this.stringWriterEncoding = Encoding.UTF8;
        }

        public override Encoding Encoding
        {
            get
            {
                return this.stringWriterEncoding;
            }
        }
    }
}
