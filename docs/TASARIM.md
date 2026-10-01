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
