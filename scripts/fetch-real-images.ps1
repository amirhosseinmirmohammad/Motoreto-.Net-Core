# دانلود عکس‌های واقعی محصول و مپ به دسته‌های فروشگاه
$ErrorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$j = Get-Content "$env:TEMP\dummy_full.json" -Raw | ConvertFrom-Json
$byCat = @{}
foreach ($p in $j.products) {
    if (-not $byCat.ContainsKey($p.category)) { $byCat[$p.category] = [System.Collections.Generic.List[object]]::new() }
    $byCat[$p.category].Add($p)
}

# مپ زیردسته‌های فروشگاه → دسته‌های dummyjson
$map = @{
    'گوشی موبایل'              = @('smartphones')
    'هدفون و هندزفری'          = @('mobile-accessories')
    'پاوربانک'                 = @('mobile-accessories')
    'قاب و گلس'                = @('mobile-accessories')
    'لپ‌تاپ'                   = @('laptops')
    'کیبورد و ماوس'            = @('mobile-accessories', 'laptops')
    'مانیتور'                  = @('tablets', 'laptops')
    'هارد و SSD'               = @('laptops', 'tablets')
    'لوازم برقی آشپزخانه'      = @('kitchen-accessories', 'home-decoration')
    'جاروبرقی'                 = @('home-decoration', 'kitchen-accessories')
    'آبمیوه‌گیری و مخلوط‌کن'    = @('kitchen-accessories')
    'اتو و بخارپز'             = @('home-decoration', 'kitchen-accessories')
    'پوشاک مردانه'             = @('mens-shirts', 'tops')
    'پوشاک زنانه'              = @('womens-dresses', 'tops')
    'کفش'                      = @('mens-shoes', 'womens-shoes')
    'ساعت'                     = @('mens-watches', 'womens-watches')
    'عطر و ادکلن'              = @('fragrances')
    'مراقبت پوست'              = @('skin-care', 'beauty')
    'لوازم آرایش'              = @('beauty')
    'لوازم کمپینگ'             = @('sports-accessories')
    'دوچرخه'                   = @('sports-accessories', 'motorcycle')
    'لوازم بدنسازی'            = @('sports-accessories')
    'کتاب چاپی'                = @('tops', 'home-decoration')
    'نوشت‌افزار'               = @('mobile-accessories')
    'کوله و کیف'               = @('womens-bags', 'mens-shoes')
    'عروسک و پولیشی'           = @('groceries', 'home-decoration')
    'بازی فکری'                = @('groceries', 'home-decoration')
    'سیسمونی'                  = @('groceries', 'beauty')
}

# خواندن محصولات فروشگاه از دیتابیس (id, subcategory)
$rows = podman exec shop-pg psql -U shop_user -d shop -A -t -c "SELECT p.id, p.slug, c.name FROM products p JOIN categories c ON p.category_id = c.id WHERE c.parent_id IS NOT NULL ORDER BY p.id;"
if (-not $rows) { Write-Host 'NO ROWS'; exit 1 }

$dir = "src\Shop.Presentation\wwwroot\images\products\real"
New-Item -ItemType Directory -Force -Path $dir | Out-Null

$cursors = @{}
$downloaded = 0; $failed = 0

foreach ($row in $rows) {
    $parts = $row.Trim() -split '\|'
    if ($parts.Count -lt 3) { continue }
    $id = $parts[0]; $slug = $parts[1]; $sub = $parts[2]

    $cats = $map[$sub]
    if (-not $cats) { $cats = @('home-decoration') }

    # انتخاب تصویر بعدی از استخر دسته (round-robin)
    $key = $sub
    if (-not $cursors.ContainsKey($key)) { $cursors[$key] = 0 }
    $idx = $cursors[$key]; $cursors[$key] = $cursors[$key] + 1

    $imgUrl = $null
    foreach ($c in $cats) {
        if (-not $byCat.ContainsKey($c)) { continue }
        $pool = $byCat[$c]
        $p = $pool[$idx % $pool.Count]
        $imgUrl = if ($p.images.Count -gt 1) { $p.images[1] } else { $p.images[0] }
        if ($idx -ge $pool.Count * 3) { $p = $pool[(Get-Random -Maximum $pool.Count)]; $imgUrl = $p.images[0] }
        break
    }
    if (-not $imgUrl) { $failed++; continue }

    $file = Join-Path $dir "$id.webp"
    if (-not (Test-Path $file)) {
        try {
            curl.exe -sL --max-time 25 $imgUrl -o $file
            if ((Get-Item $file).Length -lt 1000) { Remove-Item $file -Force; $failed++; continue }
        } catch { $failed++; continue }
    }
    $downloaded++
    $rel = "/images/products/real/$id.webp"
    podman exec shop-pg psql -U shop_user -d shop -A -t -c "UPDATE products SET main_image = '$rel' WHERE id = $id;" | Out-Null
    podman exec shop-pg psql -U shop_user -d shop -A -t -c "UPDATE product_images SET url = '$rel' WHERE id = (SELECT id FROM product_images WHERE product_id = $id ORDER BY sort_order LIMIT 1);" | Out-Null
}

Write-Host "downloaded+updated: $downloaded, failed: $failed"
