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
- **Zorluk ölçütü (2026-10-03):** Yalnızca "en az hamle sayısı" zorluğu iyi ölçmüyordu (kullanıcı: "level arttıkça
  zorlaşmıyor, ara ara çok basit level geliyor"; ölçüm de bunu doğruladı: aynı aşamadaki bölümlerde bot %0 ile %83
  arasında değişiyordu). Artık zorluk **CasualBot** ile ölçülür: kısa vadeli akıllı alışkanlıkları olan (aynı çeşidin
  üstüne koy, kutu tamamla, boş kutuyu harcama, son hamleyi geri alma), ileriyi hesaplamayan sanal bir oyuncu; 100 kez
  oynar, **kazanma oranı** (%100 = çok kolay, %0 = çok zor) bölümün zorluğudur. Tamsayı hesabı ve sabit rastgele
  üreteçle her cihazda aynı sonucu verir; ölçülen değer levels.json'a (otWinRate) yazılır.
- **Eğri** (tek yerde ayarlanır: `LevelCurve.cs`): Her bölüme **yalnızca düşen** bir hedef kazanma oranı verilir:
  bölüm 1–5: %100 (öğretici); **6'dan itibaren belirgin şekilde zorlaşır**: 6: %96; 10: %88; 15: %78; 20: %70; 30: %58;
  45: %45; 65: %33; 90: %22; 120: %13; 160: %8; 200: %5; sonrası yavaşça %3 (2026-10-04'te kullanıcı isteğiyle öğretici
  20'den 5 bölüme indirildi). Ayar (çeşit, boş kutu) merdivenden seçilir; üretici yalnızca **hedefe ±4 puan** yakın
  bölümleri kabul eder. Rastgele bölümlerin ayar içi dağılımı çok geniş (ör. 8 çeşit/2 boş kutu: %10–%85), bu bant
  o dalgalanmayı keser. Rahatlatıcı bölüm **yoktur**. Ayar merdiveni (ortalama bot kazanma oranı): 3 boş kutu, 2–4 çeşit
  (öğretici, %100) → 2 boş kutu: 4 çeşit %99,6, 5: %94, 6: %80, 7: %62, 8: %44, 9: %25, 10: %16 → 1 boş kutu: 6 çeşit
  %12, 7: %7, 8: %3,5, 9: %2,9, 10: %1,4. Boş kutular yalnızca azalır, aynı boş kutuyla çeşit yalnızca artar.
  **Hiçbir bölüm 10'dan fazla lokum çeşidi kullanmaz** (görsel bütçesi; türün ortak uygulaması 8–12 renk).
- **1 boş kutulu bölümler:** Rastgele dağıtılan tahtaların yalnızca küçük bir kısmı çözülebilir (6 çeşitte ~1/8,
  10 çeşitte ~1/135), ama çözümsüzler çözücüyle milisaniyede elendiği için çok aday denenir (8000'e kadar). Geriye
  doğru karıştırma (çözülmüş durumdan ters hamlelerle) denendi ve **işe yaramadı**: kurallar sıkı olduğu için ~10
  adımda tıkanıyor, bölümler sığ kalıyor; kod kaldırıldı.
- **Ölçüm:** Bir bölümün üretimi 0 ms ile birkaç sn arası (1 boş kutulu 10 çeşitte daha uzun); bu yüzden **bölümler oyun
  içinde üretilmez, önceden üretilip JSON dosyası olarak oyuna konur.** Böylece bölümler sabitlenir ve üretici
  sonradan değişse de yayındaki bölümler değişmez.
Bölüm dosyası (`Game/Assets/_Game/Resources/Levels/levels.json`):

- Biçim sürümü 1; her satırda bir bölüm (numara, tohum, kutu kapasitesi, en az hamle, kutuların içeriği alttan üste).
  Oyun bunu `Resources.Load<TextAsset>("Levels/levels")` ile okuyacak; okuma/yazma kodu `Core` içinde (`LevelPackJson`).
- **Üretme:** `powershell -File tools\build-levels.ps1 [-Count 200] [-Rebuild]`. Varsayılan **yalnızca ekler**:
  dosyadaki bölümlere dokunmaz, eksik numaraları üretir. `-Rebuild` tüm bölümleri değiştirir; yalnızca yayından
  önce veya bilerek kullanılır. Unity içinden: menü *Sweet Bazaar > Build Level Pack*.
- **Doğrulama:** Testler dosyayı okur, her bölümü geçerlilik açısından denetler, örnek bölümleri çözücüyle yeniden çözer.
  Tam doğrulama (hepsini çözme, ~3 dk): `tools\run-tests.ps1 -Filter AllLevels_AreSolvable`.
- **Durum (2026-10-04):** 200 bölüm yeniden üretildi; hepsinin çözülebildiği, kayıtlı en kısa hamle sayısının ve bot
  kazanma oranının doğru olduğu, başlangıçta paketlenmiş kutu olmadığı doğrulandı. Ölçülen bot kazanma oranı (10 bölümlük
  pencere ortalaması): 1–10: %97; 11–20: %76; 21–30: %64; 31–40: %53; 41–50: %46; 61–70: %33; 81–90: %24; 101–110: %16;
  121–130: %11; 141–150: %8; 191–200: %3,6. Çeşit sayısı: 1–5: 2–4; 6–10: 5; 11–20: 6–7; 21–60: 7–8; 81–100: 9;
  101–110: 10. 1 boş kutulu bölümler ~115. bölümden başlıyor (6–7 çeşit).
- **Bilinen sınırlar:** (1) Bot bir insan değildir; "bot kazanma oranı" insan zorluğunun bir göstergesi, kesin ölçüsü
  değil — kullanıcı geri bildirimiyle ayarlanacak. (2) ~115. bölümden sonra çeşit sayısı 10'dan 6–7'ye düşer (zorluk
  boş kutu azlığından gelir) ve en kısa çözüm ~31'den ~20 hamleye iner; oyuncuya "geriye gidiyor" gibi gelebilir.
  (3) Bot %0'ın altına inemez; 200. bölümden sonra zorluk ayırt edilemez, daha ileri zorluk yeni mekaniklerden
  (kapalı/kilitli kutu, bu belgenin 5. bölümü) gelmeli. (4) İlk 5 bölümde bot hep kazanır (öğretici, bilerek kolay).
- **Ölçüm aracı:** `tools\CoreBench` (`dotnet run`) oyun mantığını Unity olmadan derler ve paralel çalışır:
  `build-levels`, `verify-levels`, `difficulty` (zorluk grafiği), `cells` (ayar merdiveni), `curve`.
  Unity editörü açıkken de çalışır.
## 6. Elde tutma katmanı: Dükkânın büyümesi (seçimli, 2026-10-04)

- Bölüm geçtikçe altın kazanılır. **Hiçbir şey kendiliğinden kurulmaz**: oyuncu altınla ne yapacağını kendisi seçer
  (kullanıcı geri bildirimi: "dükkânı geliştir deyip otomatik geçiyorsa kötü, seçenek sunarak geliştirilmeli").
- Dükkânın **6 alanı** var: Tezgâh, Vitrin, Tabela, Çay köşesi, Dış cephe, Süsler. Her alanın **3 tarzı** var
  (toplam 18 seçenek). Oyuncu önce alanı, sonra tarzı seçer; resim seçilen tarzı önizler, altın harcanmaz.
- **Bedel kurulan alan sayısına göre** artar (hangi alan olduğuna göre değil): 50, 160, 420, 1000, 2200 altın (tezgâh baştan kurulu).
  **Ekonomi hesabı (2026-10-04):** bölüm geliri 10 + 2×çeşit (+5) ≈ başta 14, sonra 24–30 altın; 200 bölümde toplam ≈ 5000–6000.
  Eski fiyatlar (toplam 3160) dükkânı ~115. bölümde bitiriyordu ve altın işe yaramaz hâle geliyordu. Yeni fiyatlarla alımlar yaklaşık
  4., 11., 31., 70. ve 150. bölümde gelir (her alım öncekinden uzun sürer; bir test denetler).
- **Altın hiçbir zaman işe yaramaz hâle gelmez:** serbest hakları biten oyuncu altınla ekstra hak satın alır (Geri al 12 altın, Ekstra kutu 30 altın;
  rozet fiyatı altın renginde gösterir). Bu, ileride altın paketi satışı (gerçek parayla) ve ödüllü reklamla altın için de zemin hazırlar.
  Dükkân içeriği veriyle tanımlı; yeni alan/tarz eklemek (mevsimlik süsler vb.) için görsel gerekir, görseller gelince genişletilecek.
  Kurulmuş bir alanın tarzını değiştirmek **ücretsizdir**.
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
- **Haklar (her bölümde yenilenir, 2026-10-04):** **Geri al: 5**, **Ekstra kutu: 1**. Kalan sayı düğmelerin köşesinde kırmızı bir
  rozetle görünür; hak bitince düğme pasifleşir. Eklenmiş ekstra kutuyu geri almak **ücretsizdir** (geri alma hakkı
  harcamaz, ekstra kutu hakkını geri verir). "Yeniden başla" haklar baştan verir. Hak bitince ileride ödüllü reklam izleyerek
  hak kazanılacak (`Allowance.Grant`; reklam bağlantısı sonraki adım). Çözücü/bot zorluk ölçümü hak kullanmaz.
  **İlk 5 bölüm (öğretici, `LevelCurve.TutorialLevels`) sınırsız haktır ve rozet gösterilmez** (yeni oyuncu 5/1 sayılarını anlamıyordu).
- **Dükkânın büyümesi (seçimli, 2026-10-04):** Bölüm bitince **altın** kazanılır:
  `10 + 2 × lokum çeşidi (+5 bonus: geri al/ekstra kutu kullanmadan bitirilirse)`. Altın sol üstte sayaçla, **Dükkân**
  düğmesiyle görünür (kazanma ekranından da "Dükkâna git"). Dükkân ekranı: üstte resim, altında altın ve kurulan alan sayısı,
  6 alan düğmesi (yeşil nokta = kurulu), seçilen alanın 3 tarz kartı, bilgi satırı ve tek eylem düğmesi
  ("Kur (N altın)", kuruluysa "Bu tarzı kullan"). Altın yetmezse düğme pasif ve eksik miktar yazılır. Bedel/kurallar: bölüm 6.
  Tempo: tüm dükkân yaklaşık ilk 150 bölümde kurulur (altın ekstra haklara da harcanırsa daha geç). Seçimler (`shop.gold`, `shop.styles`) cihazda saklanır. Mantık `Core/Shop.cs`.
- **Görsel altyapısı (2026-10-04):** Gerçek çizimler kodu değiştirmeden eklenir: `Assets/_Game/Resources/Art/<ad>.png`
  varsa `ArtLibrary` onu kullanır, yoksa kodla çizilen yer tutucu kalır. Ad listesi, boyutlar ve yapay zekâ ile üretim
  yönergesi `docs/GORSEL_ISTEK.md` dosyasındadır (lokum `candy_NN`, `box_frame`, `box_parcel`, `coin`, `lokumcu_1/2`,
  `shop_<alan>_<tarz>`, `shop_bg`). Tabeladaki "Sweet Bazaar" yazısını oyun kendisi çizer.- **Lokumcu (bölüm sonu animasyonu, 2026-10-04):** Bölüm kazanılınca kazanma kartında **lokumcu** aşağıdan zıplayarak
  gelir, el sallar, "Aferin!" der; üstten lokumlar yağar ve kazanılan altın sayaçla yukarı sayılır. Yer tutucu çizgi film
  karakteri (fes, bıyık, önlük; kodla çizilir) — gerçek çizim gelince yalnızca `Lokumcu.cs` değişir. Kedi maskot fikri
  (bölüm 6) hâlâ karara bağlanmadı.
- **İlerleme:** Bulunulan bölüm ve seçilen dil cihazda saklanır. Son bölümden sonra bölüm 1'e dönülür (yeni
  bölümler eklendikçe uzar).
  **Test kolaylığı:** Unity'de "Game" nesnesini seçip Inspector'daki *Debug Start Level* kutusuna bir sayı yazınca oyun o bölümden
  başlar ve kayıtlı ilerlemeye/dile dokunmaz; menü *Sweet Bazaar > Reset Saved Level* kayıtlı bölümü unutturur (sonraki başlangıç bölüm 1).
- **Görsel (yer tutucu, 2026-10-04 "3 boyutlu his" turu):** Kullanıcı geri bildirimi: düz renkli arayüz amatör görünüyordu. Kodla çizilen
  yer tutucular şimdi: kalın/parlak "oyuncak" düğmeler (alt dudak, üst ışık çizgisi, basınca yazı çöker), ahşap çerçeveli kart,
  ahşap bölüm tabelası, koyu altın hapı, **Lilita One** yazı tipi (`Resources/Fonts`, SIL Open Font License, Türkçe karakterli),
  koyu kenarlı beyaz yazı, degrade + oryantal kafes desenli + köşeleri koyu arka plan (`BackgroundArt`), her kutu sırasının altında
  ahşap **raf**, kutu/raf gölgeleri, kutuların cam gibi yansıması, daha hacimli lokumlar. Her çeşit hem renkle hem desenle ayırt edilir
  (nokta, parça, benek, tohum, çapraz çizgi, yatay çizgi, halka, artı, dama, baklava) — renk körü oyuncular için.
  Bu hâli de geçicidir: profesyonel görünüm için yapay zekâ/ressam çizimleri `Resources/Art/` ile gelecek (`bg_game` zaten bağlı;
  düğme/kart çizimleri için 9 parçalı kesim ayarı gerekir, çizimler gelince bağlanacak). **Ses yok** (sonraki adım).- **Sahne:** `Assets/_Game/Scenes/Main.unity` (kamera + `GameBootstrap`); Play'e basınca oyun kendiliğinden kurulur.
  Sahneyi yeniden üretmek: menü *Sweet Bazaar > Create Main Scene*.
- **Kontrol aracı:** `tools\preview.ps1` oyun ekranını birkaç durumda PNG'ye çizer (bölümler, seçim, paketlenmiş kutular,
  kazanma, takılma, Türkçe); oynamadan görünüm denetimi için.
