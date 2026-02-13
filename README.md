# YouTube Analytics Tool - チャンネル分析機能

## プロジェクト概要

YouTube Data API v3を利用したチャンネル分析ツールです。
指定したYouTubeチャンネルの統計情報を取得・分析し、成長傾向や投稿パターンを判定します。

### 本プロジェクトの最終目標（全体）

- チャンネル分析
- 動画ランキング
- 伸び率・初動分析
- 競合比較
- トレンド検知

### 今回の実装スコープ（フェーズ1）

✅ **チャンネル分析機能のみ実装**

以下は**今回実装しない**：
❌ 動画ランキング
❌ 伸び率・初動分析
❌ 競合比較
❌ トレンド検知

-----

## 実現する機能

### チャンネル分析で取得・算出する項目

**基本情報**

- チャンネル名
- 登録者数
- 総再生回数
- 動画本数

**集計指標**

- 直近30日間の投稿動画数
- 直近投稿動画の平均再生数
- 動画ごとの再生数・高評価数・コメント数

**判定結果**

- 成長傾向（伸びている / 停滞 / 減少）
- 投稿頻度の傾向（高頻度 / 中頻度 / 低頻度）
- バズ依存性（バズ依存型 / 安定型）

**推移グラフ**

- 登録者数推移グラフ（期間切り替え: 7日 / 30日 / 90日）
- 再生数推移グラフ（期間切り替え: 7日 / 30日 / 90日）
- データソース: チャンネル分析実行時に `channel_snapshots` テーブルへスナップショットを自動記録
- グラフ描画: Chart.js を使用した折れ線グラフ

-----

### 推移グラフの設計

**スナップショット記録**

- チャンネル分析の実行時に、登録者数・総再生回数をタイムスタンプ付きで `channel_snapshots` テーブルに自動記録する
- 同一チャンネル・同一日のスナップショットは1件のみ保持（UPSERT）
- クォータの追加消費なし（既に取得済みのチャンネル情報を記録するのみ）

**テーブル定義**

```sql
CREATE TABLE IF NOT EXISTS channel_snapshots (
    id              BIGSERIAL PRIMARY KEY,
    channel_id      VARCHAR(50) NOT NULL REFERENCES channels(channel_id),
    subscriber_count BIGINT NOT NULL,
    total_view_count BIGINT NOT NULL,
    recorded_at     DATE NOT NULL DEFAULT CURRENT_DATE,
    UNIQUE (channel_id, recorded_at)
);
```

**期間フィルタ**

| 選択肢 | WHERE句 | 用途 |
|-------|---------|------|
| 7日   | `recorded_at >= CURRENT_DATE - 7` | 直近の短期トレンド確認 |
| 30日  | `recorded_at >= CURRENT_DATE - 30` | 月次レベルの成長把握 |
| 90日  | `recorded_at >= CURRENT_DATE - 90` | 四半期レベルの長期傾向 |

**DTOの構造**

```
TrendGraphDto
├── PeriodDays: int                    # 7 / 30 / 90
├── SubscriberTrend: List<ChannelSnapshotDto>
└── ViewCountTrend: List<ChannelSnapshotDto>

ChannelSnapshotDto
├── RecordedAt: DateTime
├── SubscriberCount: long
└── TotalViewCount: long
```

**AnalysisResultDto への追加**

- `AnalysisResultDto` に `TrendGraph: TrendGraphDto` プロパティを追加
- グラフのデフォルト表示期間は30日

**注意事項**

- スナップショットは分析実行ごとに蓄積されるため、初回分析時はデータ点が1つのみ
- データ点が2つ未満の場合、UI側で「データ蓄積中」の旨を表示する
- 90日分のグラフを完全に表示するには、90日間の分析実行履歴が必要

-----

## 採用技術スタック

|要素         |技術                                      |バージョン|
|-----------|----------------------------------------|-----|
|言語・フレームワーク |.NET                                    |8.0  |
|YouTube API|Google.Apis.YouTube.v3                  |最新   |
|データベース     |PostgreSQL                              |15以降 |
|ORM        |Dapper                                  |2.1以降|
|キャッシュ      |StackExchange.Redis                     |2.7以降|
|DI         |Microsoft.Extensions.DependencyInjection|標準   |
|ログ         |Serilog                                 |最新   |
|リトライ制御     |Polly                                   |8.0以降|

-----

## ディレクトリ構成

```
YouTubeAnalyticsTool/
├── src/
│   ├── YouTubeAnalytics.Web/                    # Presentation層（今回は最小限）
│   │   ├── Program.cs                           # エントリポイント
│   │   ├── appsettings.json                     # 設定ファイル
│   │   └── appsettings.Development.json
│   │
│   ├── YouTubeAnalytics.Application/            # Application層
│   │   ├── Services/
│   │   │   ├── ChannelAnalysisService.cs        # チャンネル分析のユースケース
│   │   │   └── AnalysisCalculator.cs            # 各種指標の計算ロジック
│   │   ├── DTOs/
│   │   │   ├── AnalysisResultDto.cs             # 分析結果の転送オブジェクト
│   │   │   ├── ChannelInfoDto.cs                # チャンネル基本情報DTO
│   │   │   ├── VideoDetailDto.cs                # 動画詳細DTO
│   │   │   ├── ChannelSnapshotDto.cs            # スナップショット1件のDTO
│   │   │   └── TrendGraphDto.cs                 # 推移グラフ用DTO（期間+データ点列）
│   │   └── Interfaces/
│   │       └── IChannelAnalysisService.cs
│   │
│   ├── YouTubeAnalytics.Domain/                 # Domain層
│   │   ├── Entities/
│   │   │   ├── Channel.cs                       # チャンネルエンティティ
│   │   │   ├── Video.cs                         # 動画エンティティ
│   │   │   └── ChannelSnapshot.cs               # チャンネルスナップショット（時系列記録用）
│   │   ├── Services/
│   │   │   ├── GrowthJudgementService.cs        # 成長傾向判定
│   │   │   └── PublishingPatternService.cs      # 投稿パターン判定
│   │   ├── Enums/
│   │   │   ├── GrowthTrend.cs                   # 成長傾向（Growing/Stable/Declining）
│   │   │   ├── PublishingFrequency.cs           # 投稿頻度（High/Medium/Low）
│   │   │   └── ContentStrategy.cs               # コンテンツ戦略（ViralDependent/Stable）
│   │   └── Repositories/
│   │       ├── IChannelRepository.cs
│   │       ├── IVideoRepository.cs
│   │       └── IChannelSnapshotRepository.cs    # スナップショット取得・保存
│   │
│   └── YouTubeAnalytics.Infrastructure/         # Infrastructure層
│       ├── YouTube/
│       │   ├── YouTubeApiClient.cs              # YouTube API呼び出し
│       │   ├── QuotaManager.cs                  # クォータ管理
│       │   └── RateLimiter.cs                   # レート制限制御
│       ├── Persistence/
│       │   ├── Repositories/
│       │   │   ├── ChannelRepository.cs         # チャンネルDB操作
│       │   │   ├── VideoRepository.cs           # 動画DB操作
│       │   │   └── ChannelSnapshotRepository.cs # スナップショットDB操作
│       │   └── Scripts/
│       │       └── InitialSchema.sql            # 初期スキーマ（channel_snapshots含む）
│       ├── Cache/
│       │   └── CacheService.cs                  # Redisキャッシュ操作
│       └── Configuration/
│           └── DependencyInjection.cs           # DI設定
│
├── tests/
│   └── YouTubeAnalytics.Tests/                  # テスト（今回は最小限）
│
├── docs/
│   └── architecture.md                          # アーキテクチャ設計書
│
├── README.md
└── YouTubeAnalyticsTool.sln
```

-----

## 実装ルール

### レイヤー間の依存ルール

```
┌─────────────────────┐
│  Web (Presentation) │
└──────────┬──────────┘
           │ 依存
           ↓
┌─────────────────────┐
│    Application      │ ← ユースケース実行、DTO変換
└──────────┬──────────┘
           │ 依存
           ↓
┌─────────────────────┐
│      Domain         │ ← ビジネスルール、エンティティ
└──────────┬──────────┘
           │ 依存（インターフェースのみ）
           ↓
┌─────────────────────┐
│   Infrastructure    │ ← 外部システム連携、永続化
└─────────────────────┘
```

**絶対禁止**

- Domain層がInfrastructure層に依存すること
- Infrastructure層がApplication層に依存すること

### Infrastructure層のルール

**YouTube API呼び出しは Infrastructure層のみ**

- `YouTubeApiClient.cs` のみがYouTube Data API v3を呼び出す
- 他のレイヤーはYouTubeAPIの存在を知らない
- API呼び出し結果はDomainエンティティ（`Channel`, `Video`）に変換して返す

**必須実装項目**

- クォータ管理（`QuotaManager.cs`）: API呼び出し前にクォータチェック
- レート制限（`RateLimiter.cs`）: 秒間リクエスト数制御、リトライロジック
- キャッシュ（`CacheService.cs`）: 同一データの再取得防止

### Application層のルール

**役割**

- ユースケースの実行（`ChannelAnalysisService.cs`）
- 集計ロジックの実装（`AnalysisCalculator.cs`）
- DTO変換（Domain → DTO）

**禁止事項**

- YouTube APIの直接呼び出し（Infrastructure層経由必須）
- データベースアクセスの直接実行（Repository経由必須）
- ビジネスルールの実装（Domain層に委譲）

**計算ロジックの原則**

- `AnalysisCalculator.cs` は純粋関数（副作用なし）
- 入力: エンティティ集合、出力: 計算結果
- 外部依存なし（DI不要）

### Domain層のルール

**1責務1クラスの徹底**

- `GrowthJudgementService.cs`: 成長傾向判定のみ
- `PublishingPatternService.cs`: 投稿パターン判定のみ
- 1つのサービスで複数の判定をしない

**判定ロジックの設計**

- 閾値は appsettings.json から注入（ハードコード禁止）
- 判定根拠をログ出力（デバッグ用）
- 将来的な判定ロジック変更に備え、Strategyパターンを意識

**エンティティのルール**

- 不変条件（Invariant）を保証
- ビジネスルールの検証（例: 登録者数は非負整数）
- public setterは原則禁止（コンストラクタで設定）

### 共通ルール

**命名規則**

- クラス名: PascalCase（例: `ChannelAnalysisService`）
- メソッド名: PascalCase（例: `AnalyzeChannelAsync`）
- 非同期メソッド: 必ず `Async` サフィックス
- プライベートフィールド: camelCase with underscore（例: `_repository`）

**非同期処理**

- I/O操作は必ず非同期（`async/await`）
- API呼び出し、DB操作、キャッシュアクセスすべて非同期

**エラーハンドリング**

- 各レイヤーで適切な例外をスロー
- Infrastructure層: API固有例外を汎用例外に変換
- Application層: ビジネス例外のスロー
- ログは構造化ログ（Serilog）で記録

**DTOの必須利用**

- Application層からの戻り値は必ずDTO
- エンティティを直接外部に公開しない

-----

## API制限と設計上の注意点

### YouTube Data API v3 クォータ制限

**デフォルト制限**

- 1日あたり: **10,000ユニット**
- リセットタイミング: 太平洋標準時（PST）午前0時

**主要操作のコスト**

|API                 |コスト    |備考                  |
|--------------------|-------|--------------------|
|`channels.list`     |1      |チャンネル情報取得           |
|`videos.list`       |1      |動画詳細取得（最大50件同時）     |
|`playlistItems.list`|1      |プレイリスト内動画ID取得（最大50件）|
|`search.list`       |**100**|検索（高コスト、今回は使用しない）   |

**1チャンネル分析のコスト試算**

- チャンネル情報取得: 1ユニット
- アップロードプレイリストID取得: 1ユニット
- 動画ID一覧取得（50件）: 1ユニット
- 動画詳細取得（50件）: 1ユニット
- **合計: 4ユニット**（動画50件まで）

**1日で分析可能なチャンネル数**

- 理論値: 10,000 ÷ 4 = **2,500チャンネル**
- 実用値: 安全マージンを考慮し **2,000チャンネル**程度

### クォータ節約のための実装方針

**QuotaManager での制御**

```csharp
// クォータチェック例（疑似コード）
await _quotaManager.CheckAndReserveAsync(requiredUnits: 4);
// ↑ クォータ不足の場合は例外スロー
```

**キャッシュ戦略**

- チャンネル基本情報: **24時間キャッシュ**（変化が少ない）
- 動画一覧: **1時間キャッシュ**（新着動画対応）
- 分析結果: **6時間キャッシュ**（再計算コスト削減）

**バッチ取得の徹底**

- 動画詳細は最大50件同時取得（`videos.list` に複数IDを渡す）
- 1件ずつ取得は絶対禁止（50倍のコスト）

### レート制限

**制限値**

- 公式発表はないが、経験的に **秒間10リクエスト程度**が安全

**実装方針**

- `RateLimiter.cs` でトークンバケット方式実装
- 429エラー（Too Many Requests）時は指数バックオフでリトライ
- Pollyのリトライポリシー適用

### データ取得時の注意点

**動画の公開日時**

- `publishedAt` はUTC（協定世界時）
- 日本時間への変換が必要な場合は +9時間

**統計情報の更新頻度**

- 再生数・高評価数: リアルタイムではなく数時間遅延の可能性
- 初動分析（将来機能）では注意が必要

**削除済み動画の扱い**

- プレイリストには存在するがAPI取得時に404
- `videos.list` の結果件数が要求より少ない場合がある
- 欠損データとしてログ記録

**プレミア公開・限定公開の扱い**

- `publishedAt` は予約公開時刻を示す場合がある
- 実際の公開状況は `status.privacyStatus` で判定

-----

## 環境構築手順

### 前提条件

- .NET 8 SDK インストール済み
- PostgreSQL 15以降 インストール済み
- Redis インストール済み
- YouTube Data API v3 のAPIキー取得済み

### APIキーの取得方法

1. [Google Cloud Console](https://console.cloud.google.com/) にアクセス
1. 新規プロジェクト作成
1. 「APIとサービス」→「ライブラリ」から「YouTube Data API v3」を有効化
1. 「認証情報」→「認証情報を作成」→「APIキー」を選択
1. 生成されたAPIキーを `appsettings.json` に設定

### 初期セットアップ

```bash
# リポジトリクローン
git clone <repository-url>
cd YouTubeAnalyticsTool

# NuGetパッケージ復元
dotnet restore

# データベース初期化
psql -U postgres -f src/YouTubeAnalytics.Infrastructure/Persistence/Scripts/InitialSchema.sql

# 設定ファイル編集
# src/YouTubeAnalytics.Web/appsettings.Development.json を編集
# - YouTubeApi:ApiKey
# - DatabaseConfig:ConnectionString
# - CacheConfig:RedisConnectionString

# アプリケーション起動
cd src/YouTubeAnalytics.Web
dotnet run
```

-----

## 設定ファイル（appsettings.json）

```json
{
  "YouTubeApi": {
    "ApiKey": "YOUR_API_KEY_HERE",
    "DailyQuotaLimit": 10000,
    "QuotaAlertThreshold": 8000,
    "RateLimitPerSecond": 10,
    "RetryPolicy": {
      "MaxRetryAttempts": 3,
      "BackoffMultiplier": 2
    }
  },
  "CacheConfig": {
    "RedisConnectionString": "localhost:6379",
    "TTL": {
      "Channel": "24:00:00",
      "Videos": "01:00:00",
      "AnalysisResult": "06:00:00"
    }
  },
  "DatabaseConfig": {
    "ConnectionString": "Host=localhost;Database=youtube_analysis;Username=postgres;Password=yourpassword"
  },
  "AnalysisConfig": {
    "RecentDaysPeriod": 30,
    "GrowthThresholdMultiplier": 1.2,
    "ViralDependency": {
      "TopPercent": 10,
      "ShareThreshold": 50
    },
    "PublishingFrequency": {
      "HighFrequencyPerWeek": 3,
      "MediumFrequencyPerWeek": 1
    }
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { 
        "Name": "File", 
        "Args": { 
          "path": "logs/log-.txt", 
          "rollingInterval": "Day" 
        } 
      }
    ]
  }
}
```

-----

## 実装の流れ（推奨順序）

### ステップ1: Infrastructure層

1. データベーススキーマ作成（`InitialSchema.sql` — `channel_snapshots` テーブル含む）
1. エンティティ定義（`Channel.cs`, `Video.cs`, `ChannelSnapshot.cs`）
1. Repository実装（`ChannelRepository.cs`, `VideoRepository.cs`, `ChannelSnapshotRepository.cs`）
1. `YouTubeApiClient.cs` 実装
1. `QuotaManager.cs` 実装
1. `RateLimiter.cs` 実装
1. `CacheService.cs` 実装

### ステップ2: Domain層

1. Enums定義（`GrowthTrend.cs`, `PublishingFrequency.cs`, `ContentStrategy.cs`）
1. `GrowthJudgementService.cs` 実装
1. `PublishingPatternService.cs` 実装

### ステップ3: Application層

1. DTOs定義（`AnalysisResultDto.cs`, `ChannelSnapshotDto.cs`, `TrendGraphDto.cs` 等）
1. `AnalysisCalculator.cs` 実装
1. `ChannelAnalysisService.cs` 実装（スナップショット記録・推移データ取得を含む）

### ステップ4: Web層

1. DI設定（`Program.cs`）
1. 簡易的な実行エントリポイント作成

### ステップ5: テスト・検証

1. 単体テスト（各Calculator, Serviceのロジック）
1. 統合テスト（実際のAPI呼び出し）
1. クォータ消費量の検証

-----

## テスト用チャンネルID

実装・検証時に使用できる公開チャンネルIDの例：

- `UCZf__ehlCEBPop-_sldpBUQ`: WIRED（英語、テック系）
- `UC_x5XG1OV2P6uZZ5FSM9Ttw`: Google Developers（公式、英語）
- `UCm7zKBNZwzjNv-lbcDkTU4Q`: HikakinTV（日本語、大規模チャンネル）

**注意**: テスト時もクォータを消費するため、無駄な呼び出しは避ける

-----

## トラブルシューティング

### API呼び出しエラー

**403 Forbidden**

- 原因: APIキーが無効、またはクォータ超過
- 対処: APIキーを確認、クォータ使用状況を確認

**404 Not Found**

- 原因: チャンネルIDまたは動画IDが存在しない
- 対処: IDの書式を確認（大文字小文字区別あり）

**429 Too Many Requests**

- 原因: レート制限超過
- 対処: `RateLimiter` が正常動作しているか確認

### データベースエラー

**接続エラー**

- 原因: PostgreSQLが起動していない、接続文字列が間違っている
- 対処: `pg_isready` コマンドで確認、接続文字列を確認

**スキーマエラー**

- 原因: テーブルが存在しない
- 対処: `InitialSchema.sql` を再実行

### Redisエラー

**接続エラー**

- 原因: Redisが起動していない
- 対処: `redis-cli ping` で確認、`redis-server` で起動

-----

## 今後の拡張予定（参考）

今回実装しない機能は、将来以下の順で追加予定：

1. **動画ランキング機能**（フェーズ2）
- 既存の `Video` エンティティ・Repositoryを流用
- `RankingService` を Application層に追加
1. **伸び率・初動分析機能**（フェーズ3）
- `video_snapshots` テーブル追加
- 時系列データ取得ロジック追加
1. **競合比較機能**（フェーズ4）
- 複数チャンネル横断集計
- 既存サービスを流用
1. **トレンド検知機能**（フェーズ5）
- 異常検知アルゴリズム実装
- バッチ処理基盤構築

-----

## ライセンス

MIT License

-----

## 参考リンク

- [YouTube Data API v3 公式ドキュメント](https://developers.google.com/youtube/v3)
- [クォータ管理ガイド](https://developers.google.com/youtube/v3/getting-started#quota)
- [Google.Apis.YouTube.v3 NuGet](https://www.nuget.org/packages/Google.Apis.YouTube.v3/)
