# StockAPI

6. 訪問Swagger文檔：
   瀏覽器中打開 `https://stockscanner.azurewebsites.net/swagger`

## 主要功能
 
- **財務連續成長指標分析**: 
  - EPS持續成長的公司
  - 營業利益率(Operating Margin)持續成長分析
  - 毛利率(Gross Margin)持續成長分析
  - 淨利率(Net Profit Margin)持續成長分析


## 技術架構
- **後端框架**: ASP.NET Core Web API
- **資料庫**: Microsoft SQL Server
- **ORM**: Entity Framework Core
- **認證機制**: JWT (JSON Web Token)
- **API文檔**: Swagger/OpenAPI
- **效能優化**: 響應壓縮、查詢優化


### 股票分析

- `GET /api/StockAnalysis/growing-financials`: 根據多維財務指標篩選公司

### 認證

- `POST /api/Auth/register`: 用戶註冊
- `POST /api/Auth/login`: 用戶登入





