using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace wmts_to_jpg.App_Code
{
    public class App
    {
        Program theform = null;
        public App(Program form)
        {
            theform = form;
        }
        public void input_vertify(string[] args)
        {
            if (args.Count() < 1)
            {
                theform.my.echo(theform.MESSAGE);
                theform.my.exit();
            }
            if (args.Count() == 6)
            {
                if (theform.my.is_string_like(args[0], "https://") || theform.my.is_string_like(args[0], "http://"))
                {
                    theform.URL = args[0];
                }
                //處理傳參數
                theform.p3826["LT_X"] = Convert.ToDouble(args[1]);
                theform.p3826["LT_Y"] = Convert.ToDouble(args[2]);
                theform.p3826["RB_X"] = Convert.ToDouble(args[3]);
                theform.p3826["RB_Y"] = Convert.ToDouble(args[4]);

                //判斷是不是真的 3826
                if (theform.p3826["LT_X"] < 300)
                {
                    theform.p3826 = theform.my.p4326_to_p3826(theform.p3826);
                }

                theform.OUTPUT_FILE = args[5];
            }
            else if (args.Count() == 1 && args[0] == "test")
            {

            }
            else
            {
                theform.my.echo(theform.MESSAGE);
                theform.my.exit();
            }
            if (!theform.my.is_string_like(theform.URL, "https://") && !theform.my.is_string_like(theform.URL, "http://"))
            {
                theform.my.echo(theform.MESSAGE);
                theform.my.exit();
            }
            fix_URL();
        }
        public void fix_URL()
        {
            //處理 URL 問題
            theform.URL = theform.URL.Replace("$", ""); //osm $ 的問題
            theform.URL = Regex.Replace(theform.URL, "{z}", "{TileMatrix}", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{x}", "{TileCol}", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{y}", "{TileRow}", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{TileMatrixSet}", "GoogleMapsCompatible", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{Style}", "default", RegexOptions.IgnoreCase);

            //如果 URL 沒有 {TileMatrix} 代表還要再從 capabilities 反解
            if (!theform.my.is_string_like(theform.URL, "{TileMatrix}"))
            {

                //嘗試下載
                string data = theform.my.b2s(theform.my.file_get_contents(theform.URL));
                //如果不是 xml 就失敗
                if (!theform.my.is_string_like(data, "xml version="))
                {
                    theform.my.echo("\r\nCan't understand capabilities...\r\n" + theform.URL + "\r\n");
                    theform.my.exit();
                }
                //取 <Contents> 前後，用 </Layer> 裁切, 且 layer = <ows:Identifier>B100000</ows:Identifier>
                var uri = new Uri(theform.URL);
                //uri 必需要有 layer=xxxx
                var Q = theform.my.QueryParse(theform.URL);
                string _Layer = Q["layer"].ToString();
                if (!Q.ContainsKey("layer"))
                {
                    theform.my.echo("\r\nUrl need layer...\r\n" + theform.URL + "\r\n");
                    theform.my.exit();
                }
                //裁切
                string _xml_contents = theform.my.get_between(data, "<Contents>", "</Contents>").Trim();
                if (_xml_contents == "")
                {
                    theform.my.echo("\r\nNo <Contents>...</Contents>\r\n" + theform.URL + "\r\n");
                    theform.my.exit();
                }
                var m = theform.my.explode("</Layer>", _xml_contents);
                bool isFound = false;
                for (int i = 0, max_i = m.Count(); i < max_i; i++)
                {
                    //符合 <ows:Identifier>B100000</ows:Identifier>
                    if (theform.my.is_string_like(m[i], "<ows:Identifier>" + _Layer + "</ows:Identifier>"))
                    {
                        isFound = true;
                        //抓 URL
                        theform.URL = theform.my.get_between(m[i], "resourceType=\"tile\" template=\"", "\"/>");
                        break;
                    }
                }
                if (isFound == false)
                {
                    theform.my.echo("\r\nNo layer found..." + _Layer + "\r\n");
                    theform.my.exit();
                }
            } //從 Capabilities 抓完

            //再次處理 URL 問題
            theform.URL = Regex.Replace(theform.URL, "{z}", "{TileMatrix}", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{x}", "{TileCol}", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{y}", "{TileRow}", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{TileMatrixSet}", "GoogleMapsCompatible", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{Style}", "default", RegexOptions.IgnoreCase);

        }
        public bool downloadTiles()
        {
            //下載圖資
            string mn = theform.my.mainname(theform.OUTPUT_FILE);
            string OP = theform.TMP_PATH + "\\" + mn;
            if (!theform.my.is_dir(OP))
            {
                theform.my.mkdir(OP);
            }
            bool check = true;
            int step = 1;
            int max_step = (theform.ptile["RB_X"] - theform.ptile["LT_X"] + 1) * (theform.ptile["RB_Y"] - theform.ptile["LT_Y"] + 1);
            for (int x = theform.ptile["LT_X"]; x <= theform.ptile["RB_X"]; x++)
            {
                for (int y = theform.ptile["LT_Y"]; y <= theform.ptile["RB_Y"]; y++)
                {
                    string _URL = theform.URL;
                    string _x = x.ToString();
                    string _y = y.ToString();
                    _URL = _URL.Replace("{TileMatrix}", "16"); // 只抓16階
                    _URL = _URL.Replace("{TileCol}", _x);
                    _URL = _URL.Replace("{TileRow}", _y);
                    string OPMN = OP + "\\" + _y + "_" + _x + ".png";

                    theform.my.echo("( " + step + " / " + max_step + " ) : " + _URL);
                    step++;
                    if (theform.my.is_file(OPMN))
                    {
                        continue;
                    }
                    theform.my.file_put_contents(OPMN, theform.my.file_get_contents(_URL));
                }
            }
            return check;
        }
        public bool mergeImage()
        {
            bool check = true;
            string mn = theform.my.mainname(theform.OUTPUT_FILE);
            string OP = theform.TMP_PATH + "\\" + mn;
            theform.bp = new Bitmap(theform.how_many_x * 256, theform.how_many_y * 256);
            Graphics gr = Graphics.FromImage(theform.bp);
            for (int x = theform.ptile["LT_X"]; x <= theform.ptile["RB_X"]; x++)
            {
                for (int y = theform.ptile["LT_Y"]; y <= theform.ptile["RB_Y"]; y++)
                {
                    string _x = x.ToString();
                    string _y = y.ToString();
                    string OPMN = OP + "\\" + _y + "_" + _x + ".png";
                    int paste_x = (x - theform.ptile["LT_X"]) * 256;
                    int paste_y = (y - theform.ptile["LT_Y"]) * 256;
                    Bitmap img = new Bitmap(OPMN);
                    gr.DrawImage(img, paste_x, paste_y);
                    img.Dispose();
                }
            }
            gr.Dispose();            
            return check;
        }
    }
}
