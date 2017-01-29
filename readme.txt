MMDX version 2 alpha readme

はじめに
MMDXをダウンロードしていただきありがとうございます。
MMDXとは樋口M氏が作成されたMikuMikuDance(以下MMD)を元にしたアニメーションライブラリです。
MMDのモデル・モーション・アクセサリをWindows(XNA or SlimDX)/XBox360(XNA)で再生することができます。

このライブラリの対象と目的
このライブラリは
MMDを「アニメーション作成ツール」として活用し
それを各種ゲームやツールで再生するための
MMDのC#実装系となります。
そのため
「MMDを活用したゲーム/ツールを作りたい！」
という人向けとなっております。

MMDXのバージョンについて
MMDXは
・MikuMikuDance for XNA 4.0
・MikuMikuDance for SlimDX(DirectX)
の二バージョンが用意されています。
それぞれゲーム向け/ツール向けとなっていますので
用途に合わせてお使いください

注意事項
・C#実装なんで本家(C++)と比べて重たいです。
・凝りまくったモデルつっこむと重たくて死にます。
・(ゲーム用途としては使いにくい重たいモデルがMMD界隈には多いです。もっと軽くして～＞＜)
・私としてはどちらかと言えばゲーム/ゲーム用ツール向けとして作ってるので、使いにくいところがあるかも。
・バグが恐らくあります。人柱募集
・Windows Phone 7は非対応です(持ってないです＞＜)

旧バージョンユーザへ
MikuMikuDance for XNA改めMMDXのメジャーバージョンアップとなります。
XNAの方で4.0にバージョンアップする際に、一から作り直したっぽいので
こちらもそれに合わせて一からつくり直すことにしました。
そのため、結構変わってますのでご注意ください(--;)
MMDX version1からの主な変更点
・.NET Frameworkが3.5→4.0
・XNAが3.1→4.0
・セルフシャドウの未実装
・マルチスレッドクラスの廃止
・物理を最初からマルチスレッド化
・Model/AccessoryがDrawableGameComponent非継承に変更
・MikuMikuDanceXNAクラスがMMDXCoreに変更し、シングルトン化
・ボーン最大数の制限が無くなる(ただし、多いボーンはやっぱ重たいです。実装がC#なのをお忘れなく)
・他、結構変更しています。

ソースコードを解析される方へ
この項を見られている方はある程度実力がある方だと思います。
こちらから、MMDXの解析のためのメモを記述しておきます。
・MMDXはXNA for Windows/XNA for XBox360/SlimDXの3種類のバイナリをほぼ同一のソースからクロスコンパイルで生成している。
・共通のソースコードはBulletX, MikuMikuDanceCore, MMDModelLibrary, MMDMotionLibrary, MMDXResourceの5つである。
・BulletXはBullet Physics Libraryから必要分をほぼそのまま独自移植している。
・毎フレーム処理ではなるべくヒープオブジェクトを生成しないようにしている。これはXBox360のアホGC対策である。(なんとかしろMS)
・物理エンジンの更新は無し。物理エンジンを高速化したい方はC++/clrを使って本家Bulletをくっつけた方がいいかも。(向こうはCPUに合わせてチューニングしてくるので勝てません。ただし、C++/clr使うと箱で動きませんが……)
・本家MMDはエッジをポリゴンの裏返しで実装しているようですが、MMDXではエッジ検出で実装しています。ポリゴンの裏返しだと計算量がどうしても多くなる……
・ゲームに使うため、速さ重視で作ってるところがあります。

既知の問題点
エッジとカリング
エッジの実装方法が本家と違うため、一部モデルでmodel.Culling=falseにしないと正しく表示されないことがあります。
ただ、カリングを行わないと表示されないモデルもあるのでご注意ください

モデルの拡大/縮小について(SlimMMDXのみ)
SlimMMDXでは不透明データにscaleキーと拡大率を入れることにより、モデルの拡大縮小が可能です。
ただし、モデルによってはモデルを作成した職人パワーにより、物理演算に不具合を起こすことが確認されています。
(要はパラメータの微調整が崩れてしまう)

ライセンス表示
The MIT License (MIT)
Copyright (c) 2011 Wilfrem

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

ライセンスがよく分からない人向け
日本語訳: http://sourceforge.jp/projects/opensource/wiki/licenses%2FMIT_license
Wikipedia: http://ja.wikipedia.org/wiki/MIT_License
要は自由に使用・改変等してよい
ただし
・【重要な箇所に著作権表示する】
・【これ使って何か起こっても知らん】
ということです。

ネットでよくある「一番ゆるいライセンス」をなんか英語でゴチャゴチャ言ってるだけなんで
普通に使う分には気にならないと思います。


Coreの不透明データパターン
・StrictFaceVert:キーがあると計算量を大幅に犠牲にして、正確な表情計算を行います。
・・表情計算のところをよく見てもらうと、速度優先でセコイ計算をしているんですが、誤差により計算が不正確になる場合がある。それを正確な計算に切り替えるのがこれ。

更新履歴
v2.04a
足のIKの計算式にバグが見つかったので修正
主に足を使った動きや腰振り系の動きをする時に微妙に動作が本家とズレるバグのfixとなります。
修正箇所
・IKの制限にIK計算時の補正計算の回転軸制限を追加。右足、左足ボーンがY軸回転しないように修正
・上記修正似あわせてIIKLimitterインターフェイスの修正。CCDSolverとDefaultIKLimitterに修正が入りました。

v2.031a
アクセサリの乗算済みアルファの消しが中途半端なバグを修正

v2.03a
XNA版のアルファ計算を乗算済みアルファから乗算しないアルファ計算に修正
上記に伴いXNAでのLat式ミクの表示がオカシイ問題も修正

v2.02a
base表情が無いモデルを入れたときにクラッシュしていたバグを修正
フレーム長が0のモーションを巻き戻す際に無限ループになっていたバグを修正
トラックオプションのNoneが効いていなかったバグを修正。
トラックオプションにExtendedModeを追加。キーフレームが終わったボーンの扱いを指定できる
(これにより、ブレンディングがやり安くなる)
v2.00aの新機能のモーションブレンディングのデモを追加(Demo6)
(モーションブレンディングはモーション間のスムースな切り替えのための機能です)

v2.01a
IMMDMotionTrackのNowFrameにsetアクセッサの追加
MMDXCore/SlimMMDXCoreのUpdateに負の値を入れた際にアニメーションがおかしくなるバグを修正
不透明データをCoreに追加
SlimDX版でデバイスロスト時の関数がまともに動作してないミスをやらかしたので、大幅修正
-デバイスロスト時にSlimMMDXCore.OnLostDeviceを。リセット後にSlimMMDXCore.OnResetDeviceを呼べばいいように修正。

v2.00a
公開
