using System;
using System.Collections.Generic;
using System.Text;
using Tup.Tars;

namespace AllLive.Core.Models.Tars
{
    public class HYGetCdnTokenResp : TarsStruct
    {
        public string url { get; set; } = "";
        public string cdn_type { get; set; } = "";
        public string stream_name { get; set; } = "";
        public long presenter_uid { get; set; } = 0;
        public string anti_code { get; set; } = "";
        public string sTime { get; set; } = "";
        public string flv_anti_code { get; set; } = "";
        public string hls_anti_code { get; set; } = "";

        public override void ReadFrom(TarsInputStream _is)
        {
            url = _is.Read(url, 0, isRequire: false);
            cdn_type = _is.Read(cdn_type, 1, isRequire: false);
            stream_name = _is.Read(stream_name, 2, isRequire: false);
            presenter_uid = _is.Read(presenter_uid, 3, isRequire: false);
            anti_code = _is.Read(anti_code, 4, isRequire: false);
            sTime = _is.Read(sTime, 5, isRequire: false);
            flv_anti_code = _is.Read(flv_anti_code, 6, isRequire: false);
            hls_anti_code = _is.Read(hls_anti_code, 7, isRequire: false);
        }

        public override void WriteTo(TarsOutputStream _os)
        {
            _os.Write(url, 0);
            _os.Write(cdn_type, 1);
            _os.Write(stream_name, 2);
            _os.Write(presenter_uid, 3);
            _os.Write(anti_code, 4);
            _os.Write(sTime, 5);
            _os.Write(flv_anti_code, 6);
            _os.Write(hls_anti_code, 7);
        }
    }
}
