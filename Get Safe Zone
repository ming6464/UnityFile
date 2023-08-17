private void UpdateHeightBlackBanner()
    {
        if(blackBannerRect == null) return;
        // Lấy kích thước khu vực an toàn của màn hình
        Rect safeArea = Screen.safeArea;
        
        float screenHeight = Screen.currentResolution.height;
        
        // Tính toán độ dài của khu vực không an toàn phía trên
        float topUnsafeAreaHeight = screenHeight - safeArea.y - safeArea.height;
        
        Vector2 sizeDelta = blackBannerRect.sizeDelta;
        sizeDelta.y = topUnsafeAreaHeight;
        blackBannerRect.sizeDelta = sizeDelta;
    }
