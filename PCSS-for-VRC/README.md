Unity 2019.4.31f1以降
VRChat SDK3-AVATAR 2022.06.03.04.45以降
導入方法
PCSSLiltoonプラグインをUnityプロジェクトにインポートします。
アバターのマテリアルにliltoonシェーダーを適用します。
同じマテリアルにPCSSLiltoon.shaderを追加で適用します。
アバターを照らすライトに、PCSSLightPlugin.csスクリプトをアタッチします。
設定方法
PCSSLiltoon.shader
PCSSLiltoon.shaderには、以下の設定項目があります。

Softness: 影の柔らかさを調整します。値が大きいほど影がぼやけます。
Intensity: 影の強度を調整します。値が大きいほど影が濃くなります。
PCSSLightPlugin.cs
PCSSLightPlugin.csには、以下の設定項目があります。

Resolution: シャドウマップの解像度を設定します。
Blocker Sample Count: ブロッカーサンプリングの数を設定します。
PCF Sample Count: PCFサンプリングの数を設定します。
Max Static Gradient Bias: 静的なバイアスの最大値を設定します。
注意事項
本プラグインは、liltoonシェーダーがアバターに適用されている必要があります。
パフォーマンスへの影響を最小限に抑えるため、適切な設定が必要です。
すべてのアバターやシーンで期待通りの結果が得られるとは限りません。
トラブルシューティング
影が表示されない場合は、ライトの設定や、シェーダーの適用順序を確認してください。
パフォーマンスが低下する場合は、解像度やサンプリング数を下げてみてください
