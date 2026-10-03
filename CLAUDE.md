# Sweet Bazaar: Lokum Sort — Claude Code Proje Notları

Bu dosya, projeyi açan her Claude oturumunun ilk okuması gereken dosyadır.
Oyun tasarımının ayrıntıları `docs/TASARIM.md` dosyasındadır; ona da mutlaka bak.

## Proje özeti

- **Oyun:** Sweet Bazaar: Lokum Sort — lokum dükkânı temalı, 2D mobil sıralama (sort) bulmacası.
- **Platformlar:** Google Play (önce) ve Apple App Store.
- **Hedef:** Kaliteli, cilalı, para kazanan bir oyun. Hızlı ama özensiz bir prototip değil.
- **Gelir modeli:** Hibrit — ödüllü reklamlar (ana gelir), sınırlı geçiş reklamları, reklam kaldırma satın alımı.

## Kullanıcı ve çalışma şekli

- Proje sahibi: Recep. Sistem yöneticisi; Git, komut satırı, sunucu kavramlarına yabancı değil,
  ancak **programlama ve oyun geliştirme deneyimi yok**.
- **İletişim dili Türkçe.** Açıklamalar sade olmalı; teknik terimler ilk kullanıldığında kısaca açıklanmalı.
- **Çalışma izni (kullanıcı 2026-10-01'de kalıcı olarak verdi):** Her adım için onay sorma. Proje dosyalarını
  oluştur/değiştir, komut çalıştır, testler geçince commit et ve özel GitHub deposuna `git push` yap.
  Oyun tasarımı kararlarını bu türün (sıralama bulmacaları) yaygın uygulamalarına göre kendin ver, kararı
  `docs/TASARIM.md`'ye yaz ve kullanıcıya kısaca özetle; kullanıcının oyun geliştirme deneyimi yok, istemiyor da.
  **Yine de önceden sor:** geri alınamaz/yıkıcı işler (force push, silme, geçmişi yeniden yazma), para harcatan
  şeyler, gizli bilgiler ve hesap ayarları, proje dışına dokunan işler ve oyunun kimliğini değiştiren büyük
  kararlar (ad, tür, gelir modeli, motor). Büyük işleri küçük, kontrol edilebilir adımlara böl; her adım sonunda
  ne yaptığını ve ölçtüğünü dürüstçe özetle (sorunları da).
- Kodu Claude yazar; kullanıcı oyunu çalıştırıp test eder ve geri bildirim verir.
  Oyunun "eğlenceli olup olmadığına" kullanıcı karar verir.
- Hataları kullanıcıdan ekran görüntüsü istemek yerine mümkün olduğunca motorun log dosyalarından
  ve komut satırı çıktılarından kendin oku.

## Geliştirme ortamı

- İşletim sistemi: **Windows**. Kullanıcı **iki farklı bilgisayarda** çalışacak.
- Git 2.56 kurulu; Git LFS etkin; varsayılan dal adı `main`; `pull.ff = only`; editör VS Code.
- Kod ve tüm proje dosyaları **özel (private) bir GitHub deposunda** tutulur.
- Proje klasörü OneDrive/Dropbox/Google Drive gibi senkronize edilen bir yerde **olmamalı**.
- iOS derlemesi için Mac yok; ileride bulut derleme (GitHub Actions + GameCI veya Codemagic) kullanılacak.

## Oyun motoru

- **Karar verildi: Unity 6.3 LTS — sürüm 6000.3.25f1.**
- Kurulu modüller: Android Build Support (OpenJDK, Android SDK & NDK Tools).
- Proje şablonu: Universal 2D. Unity projesi depo içinde `Game/` klasöründedir.
- Aynı sürüm iki bilgisayarda da kurulu olmalı; sürüm değiştirmeden önce kullanıcıya sor.
- Hata ayıklama: Editör logu `%LOCALAPPDATA%\Unity\Editor\Editor.log`; komut satırı derleme/test çıktıları da buradan okunur.
- Komut satırı testleri için editör o proje açıkken çalıştırılamaz (proje kilidi); önce editörün kapalı olduğunu kullanıcıdan teyit et.
- **Testleri çalıştırma:** `powershell -NoProfile -ExecutionPolicy Bypass -File tools\run-tests.ps1`
  (çıkış kodu 0 = hepsi geçti, 2 = test hatası). Sonuç ve log: `%TEMP%\SweetBazaar-tests\`.
  `-Filter <ad>` yalnızca eşleşen testleri çalıştırır; ölçüm testi: `-Filter SolverBenchmark` (çıktı sonuç XML'inde).
  Oyun akışı testleri (PlayMode): `-Platform PlayMode`. Her iki grubu da çalıştır; ikisi de geçmeli.
- **Görünümü gözle denetle:** `powershell -NoProfile -ExecutionPolicy Bypass -File tools\preview.ps1` PNG'ler üretir
  (yol çıktıda); `Read` aracıyla görüntüleyebilirsin. Görünümü değiştirdiysen mutlaka çalıştır.
- **Android APK (telefonda deneme):** Unity menüsü *Sweet Bazaar > Build Android APK* (çıktı `Game\Builds\Android\SweetBazaar-debug.apk`,
  depoya girmez). **Uyarı:** `tools\build-apk.ps1` ile komut satırından derleme bu bilgisayarda Gradle aşamasında
  `java.io.IOException: Unable to establish loopback connection` ile duruyor (Unity'nin JDK 17'sinde `Pipe.open`/`Selector.open`
  başarısız; normal soketler çalışıyor; bilgisayarda CrowdStrike Falcon var). Kullanıcının kendi Unity editöründen derlemenin
  çalışıp çalışmadığı denenecek; çalışmazsa bulut derleme (GitHub Actions + GameCI) planı.
- **Unity'siz hızlı kontrol (editör açıkken de çalışır):** `dotnet run -c Release --project tools\CoreBench -- <komut>` —
  `curve 1 80` (bölüm üretim istatistiği), `verify-levels <levels.json>` (tüm bölümleri yeniden çöz),
  `build-levels <levels.json> <adet> [rebuild]` (bölüm dosyasını Unity'siz üret; varsayılan yalnızca ekler), `oneempty`.
  `difficulty <levels.json> [pencere]` zorluk grafiği, `cells` ayar merdiveni, `curve 1 80` üretim istatistiği. Bölüm dosyasını
  değiştirdiysen `verify-levels` çalıştır. Unity testleri yine de (editör kapalıyken) çalıştırılmalı.
- **Kullanıcı Unity'de Play'deyken betik dosyası DEĞİŞTİRME:** editör oyun çalışırken yeniden derler ve oyunun çalışma
  anı durumu sıfırlanır (NullReferenceException yağar). Önce sor/bekle ya da yalnızca bölüm/doküman dosyalarına dokun.
- **Belirli bir bölümü denemek:** Unity'de `Main` sahnesinde *Game* nesnesini seç, Inspector'daki *Debug Start Level* kutusuna bölüm
  numarasını yaz, Play (kayıtlı ilerlemeye dokunmaz). Kayıtlı bölümü silmek için menü *Sweet Bazaar > Reset Saved Level*.
- **Başka editör metodu çalıştırma:** `tools\run-editor-method.ps1 -Method <Ad.Alan.Metot> [-Graphics]`.
- **Bölüm dosyası üretme:** `powershell -NoProfile -ExecutionPolicy Bypass -File tools\build-levels.ps1 [-Count 200] [-Rebuild]`
  (editör kapalıyken). Varsayılan yalnızca ekler; `-Rebuild` yayındaki bölümleri değiştirir, kullanıcıya sormadan kullanma.
- Kod yapısı: `Game/Assets/_Game/Scripts/Core` (saf mantık, Unity'ye erişemez), `Scripts/Game`
  (Unity tarafı), `Tests/EditMode` (testler). Ayrıntı: `docs/TASARIM.md` bölüm 12.
- Android paket adı: `com.gameworld.sweetbazaar`; ekran dikey kilitli.

## Git kuralları

- Her oturumun başında: uzak depodan son hali çek (`git pull`).
- Her adım bitip testler geçince: commit et ve gönder (`git push`); ayrıca sormana gerek yok (bkz. "Çalışma izni").
- Commit mesajları kısa ve Türkçe olabilir; ne yapıldığını açıkça söylemeli.
- `pull.ff = only` olduğu için çekme başarısız olursa **kendi başına birleştirme yapma**;
  durumu kullanıcıya açıkla ve birlikte çöz.
- Motorun geçici/önbellek klasörleri (Unity: Library, Temp, Obj, Build; Godot: .godot) depoya girmez.
  Uygun `.gitignore` dosyasını oluştur.
- Görsel, ses, font gibi büyük ikili dosyalar Git LFS ile izlenir (`.gitattributes`).

## Gizli bilgiler — ASLA depoya girmez

- Android imza dosyası (keystore) ve şifreleri.
- Reklam ağı uygulama kimlikleri, mağaza API anahtarları, sertifikalar.
- Bunlar depo dışında tutulur; kullanıcıya ayrıca güvenli yedekleme yapmasını hatırlat.
- Örnek/şablon dosyalar (örn. `secrets.example`) depoya konabilir, gerçek değerler konmaz.

## Reklam kuralı

- Geliştirme ve test sürecinde **yalnızca reklam ağının resmi test reklam kimlikleri** kullanılır.
- Gerçek reklam kimlikleri yalnızca mağazaya yüklenecek sürüme eklenir.
  (Kullanıcının kendi gerçek reklamlarına tıklaması hesabın kapatılmasına yol açabilir.)

## Kod mimarisi ilkeleri

- **Oyun mantığı motordan bağımsız olmalı:** kutular, lokumlar, hamle kuralları, kazanma kontrolü,
  bölüm üretici ve çözücü ayrı, saf kod modülleri olarak yazılır. Motor tarafı sadece görüntü,
  animasyon, ses ve dokunmatik girişle ilgilenir. Böylece ileride motor değişirse mantık korunur.
- **Bölümler veriyle tanımlanır** (ör. JSON). Bölüm üretici, her bölümün çözülebilir olduğunu
  bir çözücüyle doğrular.
- Oyun mantığı için otomatik testler yazılır ve komut satırından çalıştırılabilir olmalı.
- **Çok dil baştan kurulur:** oyundaki hiçbir metin koda gömülmez; tüm metinler çeviri tablosundan gelir.
- Kod içi isimler (değişken, fonksiyon, dosya) İngilizce; kullanıcıya görünen metinler çeviri tablosunda.

## Belgeleme kuralı

- Önemli her karar `docs/TASARIM.md` dosyasına yazılır. Bir şey önemliyse depoda yazılı olmalı.
- Bu dosyanın sonundaki "Mevcut durum" bölümü her oturum sonunda güncellenir.

## Mevcut durum

- [x] Oyun fikri, tema ve isim belirlendi (bkz. docs/TASARIM.md)
- [x] Git kuruldu ve yapılandırıldı
- [x] GitHub hesabı açıldı
- [x] GitHub iki adımlı doğrulama (2026-10-01 kullanıcı tamamladı; oyun dosyaları ondan sonra yüklendi)
- [x] GitHub harcama bütçesi adımı gereksiz (ödeme yöntemi eklenmedi; LFS ücretsiz kotası 10 GiB depolama / 10 GiB aylık trafik)
- [x] Oyun motoru kararı: Unity 6.3 LTS (6000.3.25f1), Android modülleri kurulu; Unity lisansı doğrulandı
- [x] GitHub'da boş, private SweetBazaar deposu oluşturuldu
- [x] .gitignore ve .gitattributes hazırlandı
- [x] Depo iskeleti commit edildi ve GitHub'a gönderildi (uzak depo bağlı)
- [x] Unity projesi oluşturuldu (Game/, Universal 2D), Android hedefi ve paket adı ayarlandı
- [x] Kod iskeleti (Core / Game / Tests) ve komut satırı test altyapısı (tools/run-tests.ps1)
- [x] Unity projesi ve kod iskeleti GitHub'a gönderildi
- [x] docs/KURULUM.md (ikinci bilgisayar için kurulum notları; henüz ikinci bilgisayarda denenmedi)
- [x] Temel oyun mantığı (A): kutu, tahta, hamle kuralı, kazanma/takılma, geri alma, LevelDefinition + 56 test
- [x] Çözücü (B): `Solver.Solve` (BFS, en kısa çözüm, durum üst sınırı) + testler. Ölçüm (rastgele tahta, 2 boş kutu,
  editörde): 10 çeşitte 12–57 bin durum, 0,6–3,6 sn. Az boş kutulu/zor tahtalar henüz ölçülmedi.
- [x] Bölüm üretici (C): `LevelGenerator` + `LevelCurve` (taslak zorluk eğrisi) + testler; ilk 120 bölüm üretildi (bkz. TASARIM.md bölüm 5)
- [x] JSON bölüm dosyaları (D): `LevelPackJson` + `tools\build-levels.ps1`; 200 bölüm üretildi ve tam doğrulandı
  (`Resources/Levels/levels.json`, yalnızca ekleyen üretim). Lokum çeşidi en çok 10.
- [x] Zorluk eğrisi güçlendirildi: 25'ten itibaren 1 boş kutulu zor bölümler (200 bölümün 64'ü); 200 bölüm yeniden
  üretildi ve `verify-levels` ile doğrulandı. Unity testleri editör kapanınca çalıştırılacak (bkz. aşağıdaki not).
- [x] **Zorluk sistemi yeniden kuruldu (2026-10-03):** zorluk CasualBot kazanma oranıyla ölçülüyor; LevelCurve her bölüme
  düşen bir hedef veriyor, üretici hedefe ±4 puan yakın bölümleri kabul ediyor; rahatlatıcı bölüm yok. 200 bölüm yeniden
  üretildi, erify-levels ve Unity testleri (EditMode 160, PlayMode 19) geçti. Ayrıntı: TASARIM.md bölüm 5.
- [ ] **Kullanıcı geri bildirimi bekleniyor:** Yeni bölümler (Unity'de Play, Main sahnesi; kayıtlı ilerleme varsa bölüm
  numarasını başa almak için PlayerPrefs'i silmek gerekebilir) gerçekten giderek zorlaşıyor ve "çok basit" bölüm kalmadı mı?
  Hangi bölümler çok kolay/zor geldi? Bot ayarı (CasualBot) ve hedef noktaları buna göre ayarlanır.
- [ ] Açık: ~130. bölümden sonra çeşit sayısı 6–7'ye düşüyor (zorluk boş kutu azlığından); 200'den sonra zorluk ayırt
  edilemiyor (yeni mekanik gerekir). Telefonda APK denemesi (bkz. Android APK notu).
- [x] Çok dil altyapısı: `Localizer` + `Resources/Localization/{en,tr}.json`; metinler yalnızca `LocKeys` anahtarlarıyla
- [x] Unity tarafı (E): tahta görünümü, dokunma girişi, animasyonlar, arayüz, ana sahne (`Scenes/Main.unity`);
  ilk oynanabilir sürüm. Yer tutucu (kodla çizilen) görseller; 15 PlayMode + 140 EditMode test geçiyor
  (bkz. TASARIM.md bölüm 13). **Kullanıcı Unity'de `Main` sahnesini açıp Play'e basarak deneyebilir.**
- [ ] Sıradaki: ses efektleri/müzik, ayarlar menüsü (dil seçimi), reklam (AdMob test kimlikleriyle), reklam kaldırma satın
  alımı, kayıt/ilerleme iyileştirmeleri, gerçek görseller, Android'de telefonda deneme (APK)
- [ ] İlk oynanabilir ekran
