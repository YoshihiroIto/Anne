確認ポイント
------------

* 別アプリでリポジトリ操作が行われた時
* libgit2sharp が ssh を扱えない。どうする？



機能
----

* ブランチ切り替え
* push
* pull
* グラフ表示
* 複数リポジトリ対応
* summry, discription の過去から入力


アイデア
--------

* ジョブ実行直後にファイル更新監視相当同様に扱う
    * レスポンス向上



改善ポイント
------------

* ステージングのUI反映のラグ
* 部分比較が正しく行われない
* ジョブキューサマリーで例外
* Gravatar 読み込み時例外の対処
* ステージ後の変更が正しく扱われていない
* 例外的に無視拡張子のファイルを追加した時に困った
	* リストに出てこない



System.Windows.Data Error: 4 : Cannot find source for binding with reference 'ElementName=CommitList'. BindingExpression:Path=SelectedItem; DataItem=null; target element is 'ListViewItem' (Name=''); target property is 'NoTarget' (type 'Object')
