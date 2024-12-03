using System;
using System.Collections.Generic;
using System.Text;
using Tup.Tars;

namespace AllLive.Core.Models.Tars
{
    public class HYGetCdnTokenReq:TarsStruct
    {
        public string url { get; set; } = "";
        public string cdn_type { get; set; } = "";
        public string stream_name { get; set; } = "";
        public long presenter_uid { get; set; } = 0;

        public override void ReadFrom(TarsInputStream _is)
        {
            url = _is.Read(url, 0, isRequire: false);
            cdn_type = _is.Read(cdn_type, 1, isRequire: false);
            stream_name = _is.Read(stream_name, 2, isRequire: false);
            presenter_uid = _is.Read(presenter_uid, 3, isRequire: false);
        }

        public override void WriteTo(TarsOutputStream _os)
        {
            _os.Write(url, 0);
            _os.Write(cdn_type, 1);
            _os.Write(stream_name, 2);
            _os.Write(presenter_uid, 3);
        }
    }
}

