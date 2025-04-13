# StockAPI

## 專案背景

StockAPI是專為投資者和金融分析師設計的股票資料分析API系統。在當今的金融市場，能夠快速獲取並分析公司財務數據對於做出明智的投資決策至關重要。StockAPI專注於提供公司財務指標的持續成長分析，幫助投資者識別那些具有穩定財務成長性的優質公司。

## 專案目標

- 提供高效、可靠的API接口，使用戶能夠分析公司的財務數據
- 專注於識別連續多年財務指標（如EPS、營業利益率等）持續成長的公司
- 提供靈活的查詢參數，讓用戶可以根據自己的投資策略進行篩選
- 確保資料更新及時，分析結果準確可靠

## 主要功能

- **連續EPS成長分析**: 識別連續多年EPS持續成長的公司
- **多維財務指標分析**: 
  - 營業利益率(Operating Margin)持續成長分析
  - 毛利率(Gross Margin)持續成長分析
  - 淨利率(Net Profit Margin)持續成長分析
- **自定義篩選條件**: 允許設置指標的最低門檻值
- **用戶認證**: 提供安全的用戶註冊和登入功能

## 技術架構

- **後端框架**: ASP.NET Core Web API
- **資料庫**: Microsoft SQL Server
- **ORM**: Entity Framework Core
- **認證機制**: JWT (JSON Web Token)
- **API文檔**: Swagger/OpenAPI
- **效能優化**: 響應壓縮、查詢優化

## API端點

### 股票分析

- `GET /api/StockAnalysis/growing-eps`: 獲取連續EPS成長的公司
- `GET /api/StockAnalysis/growing-financials`: 根據多維財務指標篩選公司

### 認證

- `POST /api/Auth/register`: 用戶註冊
- `POST /api/Auth/login`: 用戶登入

## 安裝與使用

1. Clone此專案到本地：
   ```bash
   git clone https://github.com/yourusername/StockAPI.git
   ```

2. 安裝依賴：
   ```bash
   cd StockAPI
   dotnet run
   ```

3. 設置資料庫連接字串：
   編輯 `appsettings.json` 檔案中的 `ConnectionStrings:DefaultConnection` 設置。

4. 運行數據庫遷移：
   ```bash
   dotnet ef database update
   ```

5. 啟動API服務：
   ```bash
   dotnet run
   ```

6. 訪問Swagger文檔：
   瀏覽器中打開 `https://localhost:5001/swagger`

## 開發計劃

- 增加更多財務指標分析
- 實現股價趨勢分析功能



