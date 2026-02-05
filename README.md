
使用プラグイン
https://github.com/Velorexe/Unity-Android-Bluetooth-Low-Energy
https://github.com/Velorexe/Unity-Android-Bluetooth-Low-Energy-Java-Library

アルゴリズム
BLE通信を定期的に行い，発見した端末をDeviceEntityとして保存．
DeviceEntityは通信時のRSSIとGPS情報をセットにしてリストに保存する．座標が全く同じ場所で取ったRSSIは平均化して保存
Visualizerは毎フレーム事にEntity一つ分の書き込みを裏バッファに行い，規定数(60)またはDeviceEntityリストの長さぶんの書き込みを終えたら表バッファと交換する．
