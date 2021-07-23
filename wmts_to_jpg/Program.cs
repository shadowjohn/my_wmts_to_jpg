using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using utility;
using wmts_to_jpg.App_Code;
using CoordinateTransformation;
using System.Drawing;

namespace wmts_to_jpg
{
    public class Program
    {
        public myinclude my = new myinclude();
        //輸出暫存的目錄
        public string TMP_PATH = "";
        public App app = null;
        //WMTS 網址
        public string URL = "https://wmts.nlsc.gov.tw/wmts?layer=B5000";
        //坐標 3826
        public Dictionary<string, double> p3826 = new Dictionary<string, double>();
        //坐標 4326
        public Dictionary<string, double> p4326 = new Dictionary<string, double>();
        //範圍抓取 xyz ZOOM,LT_X,LT_Y,RB_X,RB_Y
        public Dictionary<string, int> ptile = new Dictionary<string, int>();
        //X有幾張
        public int how_many_x;
        //Y有幾張
        public int how_many_y;
        //共幾張
        public int total_pics;
        //合併後的圖資
        public Bitmap bp;
        //面積
        public double AREA;
        //輸出檔案
        public string OUTPUT_FILE = "out.jpg";
        public string MESSAGE = @"
Usage :
  wmts_to_jpg.exe ""URL"" ""LT_X"" ""LT_Y"" ""RB_X"" ""RB_Y"" ""OUTPUT.JPG""
  wmts_to_jpg.exe test
  wmts_to_jpg.exe ""https://wmts.nlsc.gov.tw/wmts?layer=B5000"" ""289115.13"" ""2605063.03"" ""291660.12"" ""2602287.44"" ""B5000.jpg""
  wmts_to_jpg.exe ""https://wmts.nlsc.gov.tw/wmts?layer=TOPO50K_109"" ""289115.13"" ""2605063.03"" ""291660.12"" ""2602287.44"" ""out.jpg"" 
  wmts_to_jpg.exe ""https://wmts.nlsc.gov.tw/wmts/B5000/{Style}/{TileMatrixSet}/{TileMatrix}/{TileRow}/{TileCol}"" ""289115.13"" ""2605063.03"" ""291660.12"" ""2602287.44"" ""out.jpg"" 
  wmts_to_jpg.exe ""https://c.tile.openstreetmap.org/${z}/${x}/${y}.png"" ""289115.13"" ""2605063.03"" ""291660.12"" ""2602287.44"" ""out.jpg"" 
  wmts_to_jpg.exe ""https://c.tile.openstreetmap.org/${z}/${x}/${y}.png"" ""121.383"" ""23.548"" ""121.408"" ""23.523"" ""out.jpg"" 
";

        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            Program F1 = new Program();
            //輸出暫存的目錄
            F1.TMP_PATH = F1.my.getSystemKey("tmp_path");
            if (!F1.my.is_dir(F1.TMP_PATH))
            {
                F1.my.mkdir(F1.TMP_PATH);
            }
            //preset
            F1.p3826["LT_X"] = 289115.13;
            F1.p3826["LT_Y"] = 2602287.44;
            F1.p3826["RB_X"] = 291660.12;
            F1.p3826["RB_Y"] = 2605063.03;
            F1.app = new App(F1);
            F1.app.input_vertify(args); //處理輸入參數
            //修正四角位置
            F1.p3826 = F1.my.fix_LT_RB(F1.p3826);
            //定義 4326
            F1.p4326 = F1.my.p3826_to_p4326(F1.p3826);

            //取得 16 階抓取的 XYZ 範圍
            F1.ptile = F1.my.p4326_to_ptile(16, F1.p4326);

            //取得 x、y、共有幾張
            F1.how_many_x = (F1.ptile["RB_X"] - F1.ptile["LT_X"]) + 1;
            F1.how_many_y = (F1.ptile["RB_Y"] - F1.ptile["LT_Y"]) + 1;
            F1.total_pics = (F1.how_many_x * F1.how_many_y);

            //面積不能過大
            utility.PointF[] p = F1.my.p3826_to_pointf(F1.p3826);
            F1.AREA = F1.my.area_of_polygon(p);
            F1.my.echo("");
            F1.my.echo("URL：" + F1.URL);
            F1.my.echo("坐標 (EPSG:3826)：" + F1.my.json_encode(F1.p3826));
            F1.my.echo("坐標 (EPSG:4326)：" + F1.my.json_encode(F1.p4326));
            F1.my.echo("面積：" + string.Format("{0:0.00}", F1.AREA) + " 平方公尺");
            F1.my.echo("XYZ：" + F1.my.json_encode(F1.ptile));
            F1.my.echo("X：" + F1.how_many_x);
            F1.my.echo("Y：" + F1.how_many_y);
            F1.my.echo("共幾張：" + F1.total_pics);
            F1.my.echo("");
            //開始下載
            F1.my.echo("圖資暫存位置：" + F1.TMP_PATH);
            F1.app.downloadTiles();
            //合併圖資
            F1.app.mergeImage();
            //調整圖資大小
            //新大小為 800x ???
            //原圖大小
            int w = F1.how_many_x * 256;
            int h = F1.how_many_y * 256;
            //新大小
            int w_new = 800;
            int h_new = w_new * h / w; //w/h = 800/? , ? = 800*h/w
            F1.my.echo("輸出圖片大小：[ " + w_new + " , " + h_new + " ]");
            F1.my.echo("");
            F1.bp = (Bitmap)F1.my.resizeImage(F1.bp, new Size(w_new, h_new));
            F1.bp.Save(F1.OUTPUT_FILE);
            F1.bp.Dispose();
            F1.my.echo("輸出檔案：" + F1.OUTPUT_FILE);            
        }
    }
}
