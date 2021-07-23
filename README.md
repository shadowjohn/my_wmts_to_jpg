# my_wmts_to_jpg
將 wmts 第 16 階，指定的四角範圍坐標，轉成一張 jpg 縮圖 (800 x ???) 大小

Usage:

wmts_to_jpg.exe

Usage :
  wmts_to_jpg.exe "URL" "LT_X" "LT_Y" "RB_X" "RB_Y" "OUTPUT.JPG"
  wmts_to_jpg.exe test
  wmts_to_jpg.exe "https://wmts.nlsc.gov.tw/wmts?layer=B5000" "289115.13" "2605063.03" "291660.12" "2602287.44" "B5000.jpg"
  wmts_to_jpg.exe "https://wmts.nlsc.gov.tw/wmts?layer=TOPO50K_109" "289115.13" "2605063.03" "291660.12" "2602287.44" "out.jpg"
  wmts_to_jpg.exe "https://wmts.nlsc.gov.tw/wmts/B5000/{Style}/{TileMatrixSet}/{TileMatrix}/{TileRow}/{TileCol}" "289115.13" "2605063.03" "291660.12" "2602287.44" "out.jpg"
  wmts_to_jpg.exe "https://c.tile.openstreetmap.org/${z}/${x}/${y}.png" "289115.13" "2605063.03" "291660.12" "2602287.44" "out.jpg"
  wmts_to_jpg.exe "https://c.tile.openstreetmap.org/${z}/${x}/${y}.png" "121.383" "23.548" "121.408" "23.523" "out.jpg"

設定檔參數：
wmts_to_jpg.exe.config
<!--輸出庫存的目錄-->
    <add key="tmp_path" value="C:\temp\wmts_to_jpg_tmp" />
    
縮圖參考：
  <img src="screenshot/01.png">
  <img src="screenshot/02.png">
  <img src="screenshot/03.png">
  <img src="screenshot/out_osm.jpg">

Todo：
<ul>
  <li>1、參數改用 options</li>
  <li>2、wmts getcapabilities 改用 geo package</li>
  <li>3、自定傳入坐標系統類型</li>
  <li>4、合併圖資時，直接開計算後限制的大小，其他等比例縮放，減少記憶體使用</li>
</li>