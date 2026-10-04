# Sweet Bazaar: Görsel İstek Dosyası (yapay zekâ ile üretim için)

Bu dosya, oyunun **gerçek görsellerini** yapay zekâ araçlarıyla (Midjourney, DALL·E / ChatGPT görsel, Ideogram, Leonardo vb.)
üretmek için bir kılavuzdur. Şu an oyundaki görseller kodla çizilen **yer tutuculardır**; aşağıdaki isimle bir PNG dosyası
`Game/Assets/_Game/Resources/Art/` klasörüne konunca oyun **otomatik olarak** onu kullanır, kodla çizileni bırakır.
Dosya yoksa yer tutucu çizim kullanılmaya devam eder, yani görselleri **tek tek, parça parça** ekleyebilirsin.

## 1. Üretim yöntemi (adım adım)

1. **Önce tek bir "stil kartı" üret.** Aşağıdaki "Ortak stil metni"ni kullanarak 3–4 deneme yap, en beğendiğini seç. Tüm diğer
   çizimleri bu görseli **stil referansı** olarak vererek üret (Midjourney: `--sref <görsel adresi>`; ChatGPT/DALL·E: görseli
   yükleyip "bu stilde" de; Leonardo/Ideogram: "style reference"). Bu, çizimlerin birbirine uymasını sağlar; en önemli adım budur.
2. Her çizimi **ayrı ayrı** üret, en iyi sonucu seç.
3. **Arka planı sil** (şeffaf yap): remove.bg, Photoshop "Remove background", Canva "Background Remover" veya Ideogram'ın şeffaf
   arka plan seçeneği. Kenarlarda beyaz/siyah halka kalmadığından emin ol.
4. **Boyutla:** Her çizim için aşağıda yazılı **tam piksel boyutuna** getir (tuval boyutu; çizim tuvale ortalı, kenarlardan biraz
   boşluk). PNG olarak, **şeffaf arka planla** kaydet.
5. **Adlandır** (aşağıdaki listede yazan isimle, küçük harf) ve `Game/Assets/_Game/Resources/Art/` klasörüne koy.
6. Bana "şu dosyaları ekledim" de. Oyunda nasıl göründüğünü ekran görüntüsüyle kontrol edip gerekirse boyut/konum ayarını yaparım.

**Lisans:** Kullandığın aracın **ticari kullanıma izin verdiğini** (ücretli planın şartları) kontrol et. Hangi aracı, hangi plan ve tarihte
kullandığını ve istemleri (prompt) bir yere kaydet; mağaza/telif sorusu çıkarsa kanıt olur. Üretilen görsellerde **yazı olmasın**
(yapay zekâ yazıyı bozar; yazıları oyun kendisi ekler). **Marka, çizgi film karakteri veya bir sanatçı adı** kullanma.

## 2. Ortak stil metni (her istemin başına ekle)

> Cute casual mobile game art, warm cozy Turkish bazaar candy shop theme, soft hand-painted look with smooth shading and gentle
> dark-brown outlines, rich saturated warm colors (cream, honey wood, turquoise, rose pink, gold), light coming from the top
> left, clean readable shapes, no text, no watermark, isolated on a plain flat background

Stil kartı için örnek: *"Ortak stil metni + a small Turkish delight (lokum) shop counter with a few candy boxes, front view"*.

## 3. Üretilecek çizimler

Öncelik sırası: **A (lokumlar) → B (kutu ve paket) → C (lokumcu) → D (dükkân) → E (diğer)**. Önce A ve B'yi yap; oyunun
ana ekranı bunlara bağlı. Hepsi **şeffaf arka planlı PNG**.

### A. Lokumlar (10 adet) — tahtadaki ana oyun parçası

Dosyalar: `candy_00.png` … `candy_09.png`. **Boyut: 512 × 384 piksel** (4:3). Tek bir lokum küpü, hafif üstten ve önden bakışla,
tuvalin ortasında, kenarlarda boşluk. **Hepsi aynı boyutta ve aynı açıda** olmalı. Her çeşit **hem renkle hem desenle** ayırt
edilmeli (renk körü oyuncular için): küçük ekranda bir bakışta fark edilmeli.

| Dosya | Çeşit | Renk | Desen / ayırt edici ayrıntı |
|---|---|---|---|
| `candy_00` | Gül | pembe | üzeri pudra şekeri, noktalı |
| `candy_01` | Fıstıklı | yeşil | içinde ve üstünde fıstık parçaları |
| `candy_02` | Hindistancevizli | beyaz/krem | üzeri rendelenmiş hindistancevizi benekleri |
| `candy_03` | Nar | koyu kırmızı | nar tanesi benzeri parlak noktalar |
| `candy_04` | Limon | sarı | çapraz çizgili, limon kabuğu parıltısı |
| `candy_05` | Kahveli | kahverengi | yatay çizgili, kahve çekirdeği ipucu |
| `candy_06` | Portakallı | turuncu | halka şeklinde portakal dilimi deseni |
| `candy_07` | Naneli | nane yeşili/turkuaz | artı (haç) biçimli nane yaprağı deseni |
| `candy_08` | Böğürtlenli | mor | dama/kareli desen |
| `candy_09` | Yaban mersinli | mavi | ortasında elmas biçimli parlak desen |

İstem şablonu (çeşidi değiştir):
> [Ortak stil metni], a single square piece of Turkish delight (lokum), slightly top-down three-quarter view, [renk ve desen],
> dusted with powdered sugar, glossy highlights, soft shadow, centered, 4:3 composition

Not: Aynı çizimde 3–4 çeşit isteyip sonra tek tek kırpma. Her lokumu ayrı üret ya da bir sayfada üret, **hepsini aynı boyuta**
kırp ve ortala.

### B. Kutu ve paket

Kutunun yüksekliği kapasiteye göre değişir; şu an tüm bölümlerde kapasite 4. Oran **1 : 2,72** (genişlik : yükseklik).

| Dosya | Boyut | Ne |
|---|---|---|
| `box_frame.png` | **512 × 1392** | Ön görünüşlü, üstü açık, dar ve uzun **ahşap lokum kutusu**. İçi (lokumların oturduğu alan) koyu ahşap; dört lokum alt alta sığacak şekilde. Kenarlarda ahşap çerçeve. |
| `box_parcel.png` | **512 × 1392** | Aynı boyutta, **kapanmış hediye paketi**: altın/krem kâğıt, kırmızı kurdele, küçük bir kurdele fiyongu; tüm kutuyu örter. Ortada bir lokum etiketi için boş bir alan bırak (oyun oraya çeşidin lokumunu koyar). |

İstem örneği (kutu):
> [Ortak stil metni], a tall narrow open wooden box for candies, front view, wood planks with a rim, empty dark inside, centered,
> 1:2.7 portrait composition

İstem örneği (paket):
> [Ortak stil metni], a tall narrow gift-wrapped parcel, gold and cream wrapping paper, red ribbon with a small bow, front view,
> centered, 1:2.7 portrait composition

### C. Lokumcu (kazanma ekranındaki karakter)

Dostça, bıyıklı, fesli, önlüklü bir lokumcu; elinde lokum tepsisi. **İki kare** gerekli: el sallama animasyonu bu iki kare arasında geçiş yapar.

| Dosya | Boyut | Ne |
|---|---|---|
| `lokumcu_1.png` | **720 × 860** | Ayakta, mutlu; sağ eli **aşağıda**, solda tepsi. |
| `lokumcu_2.png` | **720 × 860** | Aynı karakter, aynı duruş ve boyut; sağ eli **yukarıda, el sallıyor**. |

Aynı karakteri iki farklı pozda tutarlı üretmek zordur: önce `lokumcu_1`'i üret, sonra onu referans verip "same character, raising
right hand to wave" iste. Tutarlı çıkmazsa, `lokumcu_1`'i görsel düzenleme aracında (Photoshop, Photopea) kol bölgesini
değiştirerek `lokumcu_2` yap. Yüzün, kıyafetin ve ayakların iki karede **piksel piksel aynı yerde** olması en iyisidir.

İstem örneği:
> [Ortak stil metni], friendly cartoon Turkish candy maker, big smile, thick mustache, red fez hat with tassel, blue shirt and
> white apron, holding a tray of colorful lokum candies, full body, standing, facing the viewer

### D. Lokum dükkânı (altın harcadığın ekran)

Dükkân resmi **katmanlardan** oluşur: bir arka plan ve her **alan** için 3 tarz seçeneği. Her katman **aynı tuval boyutunda
(1600 × 1040 piksel)**, şeffaf ve **diğer katmanlarla hizalı** olmalı (ilgili nesne tuvaldeki kendi bölgesinde durur, geri kalan
şeffaf). Böylece oyun, oyuncunun seçtiği tarzları üst üste koyar.

Bölgeler (1600 × 1040 tuvalde, sol-alttan ölçülü yaklaşık yerleşim; ayrıntı için **bana yer şablonu sor**, bir kılavuz resim üretirim):

| Alan | Dosyalar (tarz 0, 1, 2) | Tuvaldeki yer |
|---|---|---|
| Arka plan | `shop_bg.png` | tüm tuval: çarşı sokağı, gökyüzü, uzak çatılar, kaldırım; **dükkân nesnesi yok** |
| Dış cephe | `shop_facade_0.png`, `_1`, `_2` | arka duvar: duvar, kapı, pencere, tente/kemer (alt-orta, geniş) |
| Tezgâh | `shop_counter_0.png`, `_1`, `_2` | ön-sol/orta alt: tezgâh + üzerinde birkaç kutu lokum |
| Vitrin | `shop_display_0.png`, `_1`, `_2` | tezgâhın üstü: cam/ahşap/altın vitrin, içinde lokumlar |
| Tabela | `shop_sign_0.png`, `_1`, `_2` | üst-orta: **yazısız** tabela (yazıyı oyun ekler) |
| Çay köşesi | `shop_tea_0.png`, `_1`, `_2` | sağ alt: çay masası/taburesi/minderi, çay bardakları |
| Süsler | `shop_decor_0.png`, `_1`, `_2` | tuvalin üst yarısı/kenarları: bayraklar / fenerler / çiçekler |

Tarz fikirleri (her alan için 3 seçenek; oyuncu birini seçer, kurduktan sonra ücretsiz değiştirebilir):
- **Tezgâh:** 0 ahşap tezgâh · 1 mermer tezgâh · 2 bakır işlemeli tezgâh
- **Vitrin:** 0 cam vitrin · 1 ahşap raflı vitrin · 2 altın çerçeveli vitrin
- **Tabela:** 0 ahşap tabela · 1 renkli çini tabela · 2 altın tabela
- **Çay köşesi:** 0 masa ve taburelerle · 1 şemsiyeli masa · 2 yer minderli köşe
- **Dış cephe:** 0 çizgili tente · 1 kemerli cephe · 2 taş kemer ve küçük kubbe
- **Süsler:** 0 renkli bayraklar · 1 fenerler · 2 çiçek saksıları

Toplam 1 arka plan + 18 katman = **19 dosya**. İlk denemede hepsini üretmek zorunda değilsin: arka plan + her alandan 1 tarz yeterli;
eksik tarzlar yer tutucuyla görünür.

İstem örneği (tabela, tarz 2):
> [Ortak stil metni], an ornate golden hanging shop sign board with no text, decorative Ottoman border, two ropes, centered,
> transparent background

### E. Diğer (sonra)

| Dosya | Boyut | Ne |
|---|---|---|
| `coin.png` | 256 × 256 | altın para simgesi |
| `bg_game.png` | 1080 × 1920 | oyun ekranı arka planı (çarşı rafı/ahşap duvar), düşük kontrast; **şeffaf değil** |
| `app_icon.png` | 1024 × 1024 | uygulama simgesi (mağaza için), köşeleri yuvarlatılmamış kare |
| mağaza görselleri | ayrıca | Google Play öne çıkan görsel (1024 × 500) ve ekran görüntüleri; yayın zamanı |

Düğme ve kart çerçeveleri gibi **arayüz** çizimleri ikinci aşamadır; önce A–D.

## 4. Kontrol listesi (her dosya için)

- [ ] Şeffaf arka plan, kenarlarda beyaz/siyah halka yok
- [ ] Tam piksel boyutu, dosya adı doğru ve küçük harfli
- [ ] Çizimde yazı/harf yok
- [ ] Aynı gruptaki çizimler (10 lokum, iki lokumcu karesi, dükkân katmanları) aynı stil, boyut ve ışık yönünde
- [ ] Lokumlar küçültülünce (yaklaşık 80 piksel genişlik) hâlâ renk ve desenle ayırt ediliyor
- [ ] Kullanılan aracın ticari kullanım şartı ve istemler kaydedildi

## 5. Oyun tarafı (bilgi)

Oyun, `Resources/Art/<ad>.png` dosyasını bulursa onu kullanır (yoksa kodla çizilen yer tutucu). Yükleme tek bir yerden yapılır
(`ArtLibrary`). Boyutlandırma koddan olur: çizimin piksel boyutu önemli değil, **oranı** doğru olsun.
