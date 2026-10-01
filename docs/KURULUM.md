# Kurulum: Yeni (ikinci) bilgisayarda çalışma ortamı

Bu belge, projeyi başka bir Windows bilgisayarda sıfırdan ayağa kaldırmak içindir.
Aynı adımlar disk bozulması gibi durumlarda da geçerlidir.

## 1. Gerekli programlar

| Program | Not |
|---|---|
| **Git for Windows** (2.5x veya üstü) | Git LFS ve Git Credential Manager (GitHub girişi) bununla birlikte gelir. |
| **VS Code** | Kod düzenleyici. |
| **Unity Hub** | Unity Editor'ü buradan kurulur. |
| **Unity Editor 6000.3.25f1** (Unity 6.3 LTS) | Sürüm **birebir aynı** olmalı. Hub'ın listesinde yoksa Unity'nin sürüm arşivinden "Install with Hub" ile eklenir. |

Unity Editor kurulurken şu modüller seçilmeli:

- **Android Build Support**
  - OpenJDK
  - Android SDK & NDK Tools

iOS modülü bu bilgisayarda gerekmez (iOS derlemesi bulutta yapılacak).

Unity Hub'a **aynı Unity hesabıyla** giriş yapılır ve Personal lisans etkinleştirilir.

## 2. Git ayarları (bir kez)

```powershell
git config --global user.name "Ad Soyad"
git config --global user.email "ornek@eposta.com"
git config --global pull.ff only
git lfs install
```

- `pull.ff only`: Çekme işlemi sessizce birleştirme commit'i oluşturamaz; ayrışma varsa durur ve birlikte çözülür.
- `git lfs install`: Büyük dosyalar (görsel, ses, font) için LFS filtresini kurar.

## 3. Projeyi indirme

Proje klasörü **OneDrive, Dropbox, Google Drive gibi senkronize edilen bir yerde olmamalıdır.**
Önerilen konum: `C:\Projeler\SweetBazaar`.

```powershell
git clone https://github.com/recepsonmez06-source/SweetBazaar.git C:\Projeler\SweetBazaar
```

İlk işlemde tarayıcıda GitHub girişi açılır. Depo özel (private) olduğu için giriş yapılmadan indirilemez.

## 4. Unity projesini açma

1. Unity Hub → **Add → Add project from disk** → `C:\Projeler\SweetBazaar\Game` klasörünü seç (depo kökünü değil, `Game` klasörünü).
2. Editör sürümü olarak **6000.3.25f1** seçili olmalı.
3. İlk açılış uzun sürer (birkaç dakika): `Library` klasörü bu bilgisayarda sıfırdan üretilir. Bu klasör depoda yoktur, normaldir.
4. **File → Build Profiles → Android → Switch Platform.** Build hedefi depoda saklanmaz, her bilgisayarda bir kez yapılır.

## 5. Kontrol: testleri çalıştırma

Unity editörü kapalıyken:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:\Projeler\SweetBazaar\tools\run-tests.ps1
```

Beklenen sonuç: `Unity exit code: 0` ve `Failed: 0`.

## 6. Depoda olmayan, elle taşınması gerekenler

Bunlar bilerek Git'e girmez (bkz. `CLAUDE.md`, "Gizli bilgiler"):

- Android imza dosyası (keystore) ve şifreleri
- Reklam ağı gerçek uygulama kimlikleri, mağaza API anahtarları, sertifikalar

Güvenli bir yerden (parola yöneticisi, şifreli yedek) bu bilgisayara ayrıca aktarılır.
Geliştirme sürecinde yalnızca reklam ağının resmi **test** kimlikleri kullanılır.

## 7. İki bilgisayarla çalışma kuralları

- **Oturumun başında:** `git pull`. Başarısız olursa kendi başına birleştirme yapma; durumu anlat ve birlikte çöz.
- **Oturumun sonunda:** Unity'de `Ctrl+S` ile sahneyi kaydet, commit et ve `git push`.
- **Aynı anda yalnızca bir bilgisayarda çalış.** Unity sahne ve ayar dosyaları metin olsa da elle birleştirmesi zordur.
- Büyük bir çekmeden önce Unity editörünü kapatmak en güvenlisidir.
- Bir bilgisayarda bırakılan bitmemiş iş, push edilmeden diğerine geçilmemelidir.
