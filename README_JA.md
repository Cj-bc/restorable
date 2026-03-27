# Restorable

[English version is here / 英語版](./README.md)

## 概要

ランタイム上で調整可能な設定項目を作成するための、簡易的なパッケージです。
値をJSONでシリアライズし、指定したファイルに保存・そこからの読み込みを行います。

主な構成要素は2つです:

- **`Restorable` インターフェース** — 状態を保存したいクラスに実装します。以下の2つのメソッドが必要です:
  - `MakeSnapshot()` — 現在の状態を `JsonObject` にシリアライズします。
  - `Restore(JsonObject snapshot)` — 保存済みの `JsonObject` から状態を復元します。

- **`RestorableStore` ScriptableObject** — `Restorable` オブジェクトのコレクションを管理し、スナップショットのディスクへの読み書きを担います。

## 使用方法

### 1. `Restorable` インターフェースを実装する

```csharp
using System.Text.Json.Nodes;

public class MyGameState : Restorable
{
    private int _score;
    private string _playerName;

    public JsonObject MakeSnapshot()
    {
        return new JsonObject
        {
            ["score"] = _score,
            ["playerName"] = _playerName,
        };
    }

    public void Restore(JsonObject snapshot)
    {
        _score = snapshot["score"]?.GetValue<int>() ?? 0;
        _playerName = snapshot["playerName"]?.GetValue<string>() ?? "";
    }
}
```

### 2. `RestorableStore` アセットを作成する

Unity エディタのプロジェクトウィンドウで右クリックし、以下を選択します:

```
Create > Restorable > Store
```

インスペクタで次の項目を設定します:

| フィールド | 説明 |
|-----------|------|
| **Raw Config Path** | セーブファイルのパス（ファイル名または相対パス） |
| **Path Root** | セーブファイルのベースディレクトリ: `Absolute`（絶対パス）、`PersistentDataPath`、`DataPath` のいずれか |

### 3. オブジェクトを登録してセーブ・ロードする

```csharp
[SerializeField] private RestorableStore _store;

void Start()
{
    // セーブ・ロード対象のオブジェクトを登録する
    _store.Register(myGameState);
}

void SaveGame()
{
    _store.Save();
}

void LoadGame()
{
    _store.Restore();
}
```

インスペクタ上の `RestorableStore` コンポーネントのコンテキストメニューから **Save** / **Restore** を直接実行することもできます。

## インストール

Unity Package Manager でこのリポジトリの Git URL を指定してパッケージを追加するか、`Packages/com.github.cj-bc.unity.restorable` ディレクトリをプロジェクトの `Packages` フォルダにコピーしてください。

**依存パッケージ:**
- `org.nuget.system.text.json` 10.0.2 — [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) でインストールする必要があります
