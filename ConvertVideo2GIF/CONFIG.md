# 設定檔說明 (Configuration Guide)

## 初次使用設定 (First-time Setup)

1. 複製 `appsettings.sample.json` 檔案，並重新命名為 `appsettings.json`
2. 開啟 `appsettings.json`，修改 `WorkingDirectory` 為你的影片工作目錄
3. 確保目錄路徑使用雙反斜線 `\\` 或單斜線 `/`

## 設定檔範例 (Configuration Example)

```json
{
  "AppSettings": {
    "WorkingDirectory": "C:\\Users\\YourUsername\\Downloads\\"
  }
}
```

### Windows 範例
```json
{
  "AppSettings": {
    "WorkingDirectory": "D:\\Videos\\"
  }
}
```

或使用斜線:
```json
{
  "AppSettings": {
    "WorkingDirectory": "D:/Videos/"
  }
}
```

## 注意事項 (Notes)

- `appsettings.json` 已加入 `.gitignore`，不會被提交到 Git
- `appsettings.sample.json` 是範例檔案，會被提交到 Git
- 如果 `appsettings.json` 不存在或設定錯誤，程式會使用預設的 Downloads 資料夾
- 請確保設定的目錄路徑存在且有讀寫權限

## 設定項目說明 (Configuration Settings)

| 設定項目 | 說明 | 必填 | 預設值 |
|---------|------|------|--------|
| WorkingDirectory | 影片處理的工作目錄 | 否 | %UserProfile%\Downloads\ |
