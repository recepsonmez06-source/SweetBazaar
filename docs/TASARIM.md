# Sweet Bazaar: Lokum Sort — Oyun Tasarım Belgesi

Durum: İlk taslak. "Taslak" olarak işaretli maddeler kullanıcıyla henüz kesinleşmedi.

## 1. Kimlik

- **Ad:** Sweet Bazaar
- **Mağaza adı (öneri):** Sweet Bazaar: Lokum Sort
  - Not: Sadece "Lokum" adı başka bir oyunda kullanılıyor; kullanılmayacak.
  - Yayından önce mağazalarda benzer isim kontrolü yapılacak.
- **Tür:** Hibrit-casual sıralama (sort) bulmacası, 2D
- **Tema:** Lokum dükkânı; Türk çarşısı atmosferi
- **Hedef kitle:** Her yaştan, kısa oturumlarla oynayan bulmaca severler; dünya geneli

## 2. Neden bu tür

- Sıralama bulmacaları 2025–2026'da gelirini hızla artıran, oyuncu başı geliri yüksek bir tür.
- Kodlaması sade; bölümler algoritmayla üretilebilir.
- Lokum temalı bir sıralama oyununa piyasada rastlanmadı; tema farklılık sağlıyor.
- Risk: Tür kalabalık; öne çıkmak için görsellik, his ve elde tutma çok iyi olmalı.

## 3. Temel oynanış

- Tezgâhta kutular var. **Her kutu 4 lokum** alır; lokumlar kutuda üst üste dizilidir.
- Oyuncu bir kutuya, sonra hedef kutuya dokunur; kaynak kutunun **en üstündeki lokum**
  (ve hemen altında aynı çeşitten olanlar) hedef kutuya geçer.
- **Kural:** Lokum yalnızca aynı çeşidin üstüne veya boş kutuya konabilir; hedefte yer olmalıdır.
- Bir kutu 4 aynı çeşit lokumla dolunca **kapağı kapanır ve paketlenir** (kutlama animasyonu, ses).
- Tüm çeşitler paketlenince bölüm kazanılır.
- Geçerli hamle kalmazsa oyuncu takılmış olur: geri al, ekstra kutu veya yeniden başla seçenekleri.

Kesinleşen kural ayrıntıları (2026-10-01, `Scripts/Core` içinde kodlandı ve testlendi):

- **Kısmi taşıma var:** Kaynağın üstündeki aynı çeşit lokumlar birlikte taşınır; hedefte yer azsa
  sığan kadarı taşınır, kalanı kaynakta durur. Oyuncu taşınacak sayıyı seçmez.
- **"Takıldın" yalnızca geçerli hamle kalmayınca** gösterilir. Hamle varken çözümsüz duruma girilmesi
  şimdilik algılanmaz; ileride çözücüyle geliştirilebilir.
- **Kapalı (paketlenmiş) kutu donar:** Ne kaynak ne hedef olabilir; kazanmada sayılır.
- **Kazanma:** Her kutu ya boştur ya da kapalıdır.
- Başlangıçta zaten dolu ve tek çeşitli kutu kapalı sayılır (üretici bunu üretmeyecek).
- Aynı kutuya hamle geçersizdir (ekranda "seçimi iptal et" anlamına gelir).
- Geri alma mantık katmanında sınırsızdır; geri alma ve ekstra kutu hakkının sınırı ile reklam bağlantısı oyun katmanında olacak.
- Bilinen sınır: Hamlenin hiçbir şey değiştirmediği "boşa" hamleler (ör. tek çeşitli, dolu olmayan bir kutuyu
  boş kutuya taşımak) geçerlidir. Çözücü bunları eleyecek; oyuncu açısından "takıldı" sayılmaz.

## 4. Lokum çeşitleri ve erişilebilirlik

- Her çeşit **hem renkle hem de görsel bir ayrıntıyla** ayırt edilir (renk körü oyuncular için):
  - Gül — pembe, pudra şekerli
  - Fıstıklı — yeşil, fıstık parçalı
  - Hindistancevizli — beyaz, rendeli
  - Nar — kırmızı
  - Limon — sarı
  - Diğerleri ilerledikçe eklenir (ör. kahveli, portakallı, naneli) — taslak
- Sadece renge dayalı bir ayrım yapılmaz.

## 5. Bölüm ilerleyişi (taslak)

- **Bölüm 1–5:** Öğretici. 2–3 çeşit, bol boş kutu. Kurallar oynayarak öğretilir.
- Sonra çeşit sayısı kademeli artar, boş kutu sayısı azalır.
- **Yaklaşık her 15–20 bölümde yeni bir öğe** tanıtılır. Aday öğeler:
  - Kapalı kutular (içindekiler görünmez, üstteki açıldıkça görünür)
  - Kilitli kutular (belirli bir kutu paketlenince açılır)
  - İki katlı kutular
- **Zorluk dalgalı artar:** birkaç kolay bölüm → bir zor bölüm → rahatlatan bölüm.
- Bölümler algoritmayla üretilir; her bölüm bir çözücüyle **çözülebilirlik** açısından doğrulanır.
- Zorluk ölçütleri (çeşit sayısı, boş kutu, minimum hamle sayısı vb.) veriye bağlı ve ayarlanabilir olmalı.

Bölüm üretici (2026-10-01, `Scripts/Core`: `LevelGenerator`, `LevelCurve`, `DifficultyProfile`):

- **Yöntem:** Lokumlar bölüm numarasından türeyen tohumla (seed) rastgele kutulara dağıtılır; çözücü bölümü
  doğrular. Çözülemeyen veya hamle sayısı aralığın dışında kalan aday atılır, sıradaki denenir.
  Aynı bölüm numarası her zaman aynı bölümü verir (kendi tohumlu rastgele sayı üretecimiz kullanılır).
- **Zorluk ölçütü:** Çözücünün bulduğu en az hamle sayısı, çeşit sayısı, boş kutu sayısı.
- **İlk taslak eğri** (tek yerde ayarlanır: `LevelCurve.cs`): 1–5 öğretici (2 çeşit, 3 boş kutu, kısa çözüm);
  6–20: 4→6 çeşit; 21–50: 6→9; 51+: 8→10 çeşit; hep 2 boş kutu. Dalga: her 5. bölüm zor (+1 çeşit, daha uzun
  çözüm şartı), ondan sonraki bölüm rahatlatıcı (−1 çeşit). **Hiçbir bölüm 10'dan fazla lokum çeşidi kullanmaz**
  (görsel bütçesi; sınıfın ortak uygulaması 8–12 renk).
- **Ölçüm (editörde):** Bir bölümün üretimi 0 ms ile birkaç sn arası sürüyor; bu yüzden **bölümler oyun içinde
  üretilmez, önceden üretilip JSON dosyası olarak oyuna konur.** Böylece bölümler sabitlenir ve üretici sonradan
  değişse de yayındaki bölümler değişmez.

Bölüm dosyası (`Game/Assets/_Game/Resources/Levels/levels.json`):

- Biçim sürümü 1; her satırda bir bölüm (numara, tohum, kutu kapasitesi, en az hamle, kutuların içeriği alttan üste).
  Oyun bunu `Resources.Load<TextAsset>("Levels/levels")` ile okuyacak; okuma/yazma kodu `Core` içinde (`LevelPackJson`).
- **Üretme:** `powershell -File tools\build-levels.ps1 [-Count 200] [-Rebuild]`. Varsayılan **yalnızca ekler**:
  dosyadaki bölümlere dokunmaz, eksik numaraları üretir. `-Rebuild` tüm bölümleri değiştirir; yalnızca yayından
  önce veya bilerek kullanılır. Unity içinden: menü *Sweet Bazaar > Build Level Pack*.
- **Doğrulama:** Testler dosyayı okur, her bölümü geçerlilik açısından denetler, örnek bölümleri çözücüyle yeniden çözer.
  Tam doğrulama (hepsini çözme, ~3 dk): `tools\run-tests.ps1 -Filter AllLevels_AreSolvable`.
- **Durum (2026-10-01):** 200 bölüm üretildi ve hepsinin çözülebildiği, kayıtlı en kısa hamle sayısının doğru olduğu
  tam doğrulamayla görüldü. Çeşit ve en az hamle: 1–5: 2 çeşit, 4–7 hamle (ort. 5); 6–20: 3–6, 8–20 (ort. 13);
  21–50: 5–9, 13–29 (ort. 21); 51–100: 7–10, 21–33 (ort. 27); 101–200: 9–10, 24–34 (ort. 30).
- **Bilinen sınırlar:** (1) 1 boş kutulu rastgele tahtaların çoğu çözümsüz çıktı (8 denemenin 1'i kabul edildi);
  daha zor bölümler için "karıştırma" yerine çözümden geriye doğru kurma gibi başka bir yöntem gerekebilir.
  (2) Çeşit sayısı 10 ile sınırlı olduğundan zorluk yaklaşık 65. bölümden sonra platoya giriyor (en az hamle
  ortalama ~30, zor bölümler ~31–33, rahatlatıcılar ~25); daha ileri zorluk bu belgenin 5. bölümündeki yeni
  mekaniklerden (kapalı/kilitli kutu) gelmeli. (3) Zorluk sayıları tahmindir; oynayarak ayarlanacak.

## 6. Elde tutma katmanı: Dükkânın büyümesi (taslak)

- Bölüm geçtikçe altın kazanılır.
- Altınla dükkân geliştirilir: küçük tezgâh → vitrin → tabela → çay köşesi → çarşıda büyük dükkân
  → tanınmış şekerci.
- Fikir (karar verilmedi): Dükkânda yaşayan, oyuncuya eşlik eden bir kedi maskot.

## 7. Para kazanma

- **Ödüllü reklam (ana gelir):** Takılınca reklam izleyip ekstra boş kutu veya geri alma hakkı.
- **Geçiş reklamı:** İlk 10 bölümden sonra, birkaç bölümde bir, bölüm aralarında. Oyuncuyu
  bıktırmayacak sıklıkta; veriye göre ayarlanacak.
- **Satın alma:** Reklamları kaldırma (küçük ücret). İleride ipucu/ekstra kutu paketleri.
- Reklam ağı: Google AdMob ile başlanacak; oyun büyüyünce mediation (birden çok ağ).
- Reklam panelinde kumar, bahis, flört vb. kategoriler engellenecek (aile dostu oyun).

## 8. Dil desteği

- Oyun ilk açılışta **telefonun diline göre** başlar; o dil yoksa İngilizce.
- **Oyuncu ayarlar menüsünden dili değiştirebilir;** seçim kaydedilir ve sonraki açılışlarda korunur.
- İlk sürüm: **Türkçe ve İngilizce.**
- Sonraki diller (taslak): Almanca, İspanyolca, Portekizce, Fransızca, Japonca.
- Mağaza sayfaları (ad, açıklama, ekran görüntüsü metinleri) da her dil için ayrı hazırlanır.

## 9. Görsel ve ses yönü (taslak)

- Sıcak, renkli, temiz 2D çizgi film tarzı; küçük ekranda bir bakışta okunabilir.
- Lokumların yer değiştirmesi, kutunun kapanması ve paketlenmesi **tatmin edici** hissettirmeli;
  bu anlar tanıtım videolarında da kullanılacak.
- Görseller: hazır asset paketleri ve/veya tasarımcı; lisansları ticari kullanıma uygun olmalı.
- Sesler: yumuşak, çarşı atmosferli müzik; dokunma, yerleşme ve paketleme efektleri.

## 10. Yayın planı

1. Oynanabilir prototip → kullanıcı testleri.
2. Android'de kapalı test.
3. **Soft launch:** birkaç ülkede sınırlı yayın; 1. gün elde tutma ve reklam verileri izlenir
   (1. gün elde tutma hedefi yaklaşık %35–40).
4. Verilere göre iyileştirme → dünya geneli yayın → iOS.
5. Düzenli güncellemeler: yeni bölümler, özel gün temaları.

## 11. Açık sorular

- ~~Oyun motoru~~ — **Karar verildi (2026-10-01): Unity 6.3 LTS, sürüm 6000.3.25f1**, Android Build Support
  kurulu. Unity projesi depoda `Game/` klasöründe tutulur; dokümanlar ve ileride CI ayarları köktedir.
- Kedi maskot olacak mı?
- Kesin bölüm öğeleri ve sıraları
- Görsellerin kaynağı (asset paketi mi, tasarımcı mı?)
- Mağaza adının son kontrolü

## 12. Teknik kararlar

- **Motor:** Unity 6.3 LTS, sürüm 6000.3.25f1; şablon Universal 2D (URP). Karar tarihi: 2026-10-01.
- **İlk hedef platform: Android.** Unity projesi tek ve ortaktır; iOS sonradan aynı projeden, Mac
  gerektirdiği için bulut derlemeyle (GitHub Actions + GameCI veya Codemagic) üretilecek.
- **Android paket adı (bundle ID): `com.gameworld.sweetbazaar`.** Google Play'de yayından sonra
  değiştirilemez. Şirket adı: Game World; uygulama adı (cihazda görünen): Sweet Bazaar.
  iOS paket adı iOS aşamasında ayrıca belirlenecek.
- **Ekran yönü:** Dikey (portrait) kilitli.
- **Depo yapısı:** Köke dokümanlar ve araçlar (`docs/`, `tools/`), Unity projesi `Game/` altında.
- **Kod yapısı** (`Game/Assets/_Game/`):
  - `Scripts/Core` (`SweetBazaar.Core`): saf oyun mantığı; `noEngineReferences` açık, yani
    `UnityEngine`'e erişemez. Kutular, hamle kuralları, kazanma kontrolü, bölüm üretici, çözücü burada.
  - `Scripts/Game` (`SweetBazaar.Game`): görüntü, animasyon, ses, dokunmatik giriş. Core'a bağımlıdır, tersi olamaz.
  - `Tests/EditMode` (`SweetBazaar.Core.Tests`): Core'un otomatik testleri.
- **Testler:** `tools\run-tests.ps1` ile komut satırından çalışır (editör o proje açıkken çalışmaz).
- **Sürüm denetimi:** Yalnızca Git (Unity Version Control kullanılmıyor). Büyük ikili dosyalar Git LFS ile.

## 13. Oyun ekranı ve kontroller (ilk oynanabilir sürüm, 2026-10-01)

Karar ve davranışlar bu türün (su/top sıralama oyunları) yaygın uygulamalarına göre verildi:

- **Kontrol:** Dokunarak seç, dokunarak bırak. Dolu bir kutuya dokununca üstteki aynı çeşit lokumlar yukarı kalkar;
  başka bir kutuya dokununca geçerliyse taşınır. Aynı kutuya tekrar dokunmak veya boş yere dokunmak seçimi bırakır.
  Geçersiz hedefe dokunulursa seçim o kutuya geçer (alınamayacak bir kutuysa sallanır). Paketlenmiş kutu seçilemez.
- **Animasyon:** Lokumlar yay çizerek uçar (arka arkaya), kutu dolunca altın paket kurdelesiyle kapanır ve küçük
  bir zıplama yapar. Animasyon sürerken dokunma yok sayılır.
- **Düzen:** Tek ekran, dikey. Satır başına en çok 4 kutu (az sütun = büyük, kolay dokunulur kutu); kutular
  satırlara eşit dağıtılır; kamera üst/alt arayüze yer bırakacak şekilde kutuları ortalar. Çentik/yuvarlak köşeler
  için güvenli alan kullanılır. 9:16, 9:20 ve 3:4 oranlarında her kutunun ekrana sığdığı testle doğrulandı.
- **Arayüz:** Üstte bölüm ve hamle sayısı, sağ üstte dil düğmesi (geçilecek dilin kendi adını gösterir),
  altta Geri al / Ekstra kutu / Yeniden başla. Kazanınca "Sonraki bölüm" kartı; takılınca üstte uyarı şeridi
  (tahta görünür kalır).
- **Geri al:** Sınırsız (şimdilik). **Ekstra kutu:** Bölüm başına 1 kez (şimdilik ücretsiz; ileride ödüllü reklam
  karşılığı). Geri al, ekstra kutuyu da geri alır ve hakkı geri verir. Reklam bağlantısı sonraki adım.
- **İlerleme:** Bulunulan bölüm ve seçilen dil cihazda saklanır. Son bölümden sonra bölüm 1'e dönülür (yeni
  bölümler eklendikçe uzar).
- **Görsel:** Henüz yer tutucu; lokumlar, kutu ve paket kodla çizilir. Her çeşit hem renkle hem desenle ayırt edilir
  (nokta, parça, benek, tohum, çapraz çizgi, yatay çizgi, halka, artı, dama, baklava) — renk körü oyuncular için.
  Gerçek görseller gelince yalnızca `CandyArt` değişir. **Ses yok** (ses dosyaları gerekir; sonraki adım).
- **Sahne:** `Assets/_Game/Scenes/Main.unity` (kamera + `GameBootstrap`); Play'e basınca oyun kendiliğinden kurulur.
  Sahneyi yeniden üretmek: menü *Sweet Bazaar > Create Main Scene*.
- **Kontrol aracı:** `tools\preview.ps1` oyun ekranını birkaç durumda PNG'ye çizer (bölümler, seçim, paketlenmiş kutular,
  kazanma, takılma, Türkçe); oynamadan görünüm denetimi için.
