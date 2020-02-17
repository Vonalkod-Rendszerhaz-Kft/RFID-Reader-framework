# Vrh.EventHub.Core
![](http://nuget.vonalkod.hu/content/projectavatars/eventhub.png)

*Ez a leírás a komponens **1.2.0** kiadásáig bezáróan naprakész.
Igényelt minimális framework verzió: **4.5**
Teljes funkcionalitás és hatékonyság kihasználásához szükséges legalacsonyabb framework verzió: **4.5***

**A komponens arra szolgál, hogy egységes és egyszerű megvalósítási alapja legyen .Net alkalmazás modulok közti kommunikációnak. Attól függetlenül, hogy azok közös alkalmazástérben működnek-e, vagy sem.**

A komponenssel kapcsolatban az alábbi fő jellemzők megvalósítását tűztük ki célul:
* .Net alapú alkalmazások, alkalmazás modulok realtime kommunikációs pltaformja lehessen, elősegítve ezzel MicroService elvű architektúrák kialakítását.
* Egységes és egyszerű használati keretet adjon a felhasználási helyeknek, melyre építve különféle csatornákon való kommunikáció kialakítására legyen lehetőség, miközben a használati mód változatlan marad.
* Fenti csatornák tetszőlegesen kialakíthatóak legyenek, és ezekkel az EventHub megoldás utólag is tetszőlegesen bővíthető legyen. (Csatorna implementációk fejlesztésének is keretet adjon.)
* A megoldás .Net típusokkkal működjön, és a használt átviteli csatornától függetlenül típusbiztonságot garantáljon. (Typesafe)
* A megoldás a konkrét kommunikáció tekintetében konfigurációmentes, és típusvezérelt legyen. Így biztosítva eszközt a kód szintű egyezmények kényszerítésére, és betartására. (Contracts)

## A komponens használata
A komponens kommunikációra való használatához a **Vrh.EventHub.Core** nuget csomagot kell telepíteni. Jellemzően nem telepítjük külön ezt a csomagot, hanem valamelyik használni kívánt csatorna implemntáció függőségeként annak a Nuget csomagja "húzza le". 

A felhasználás helyén a **Vrh.EventHub.Core** névtért kell includolni:
```csharp
using Vrh.EventHub.Core;
```
Az EventHub szolgáltatásai az **EventHubCore** statikus típuson keresztül érhetőek el a felhasználó alkalmazás számára. Mielőtt rátérnénk ennek részleteire célszerű az EventHub terminológiájában használt alapvető fogalmak tárgyalását megejteni a jobb érthetőség kedvéért:
* Az EventHub - a microservice elvekkel összhangban - a kommunikációban részt vevő két oldal kapcsolati rendszerét abból a nézőpontból határozza meg, hogy ki nyújt szolgáltatást kinek. De megengedi, hogy ugyanazon két végpont közti kommunikációban mindkét végpont egyaránt vállaljon szolgáltatást nyújtó és azt igénybe vevő szerepkört (duplex szolgáltatáscsatorna). A szolgáltatást nyújtó oldalt az EventHub terminológijában **fogadó oldal**nak (**Receiver**), míg az azt igénybe vevő oldalt **küldő oldal**nak (**Sender**) nevezzük.
* Fenti tulajdonsága miatt az EventHub felett egyaránt nagy szabadsággal implementálhatóak **szinkron-**, **aszinkron-**, és **visszajelzés nélküli** architektúrák és ezek legkülönfélébb keveréke, ami által az EventHub egy teljesen szabadon kialakítható hibrid, hálós kommunikációs felületet biztosít.
* **Szinkron architektúrának** nevezzük azt a megvalósítást, ahol a küldő oldal megvárja, amíg a fogadó oldal feldolgozza az üzenetét és arra egy adott választ, vagy hibajelzést küld vissza.
* **Aszinkron architektúrának** nevezzük azt a megvalósítást, amikor a küldő oldal elküld egy üzenetet, de nem vár rá választ közvetlen (szinkron) módon a küldés helyén. A választ az EventHub keret aszinkron módon továbbítja majd az eredetileg elküldött üzenetre, amikor a fogadó oldal feldolgozza azt, és kezdeményezi a válasz (aszinkron) elküldését a feladónak. A fogadó oldal majd az eredeti küldő oldal lesz ezen kommunikációs aktus tekintetében, és a fogadásnak mint független eseménynek kell gondoskodnia bármilyen üzenetpárosítási mechanizmusról, vagy blokkolás megszüntetésről az eredeti küldő oldalon. A teljes értékű aszinkron architektúrák implementációja rendkívül bonyolult lehet. Mivel az EventHub.Core a szinkron kommunikációt is egy elfedett kétirányú aszinkron kommunikáció felett valósítja meg, amely nem blokkolja az infrastruktúrát (csak a küldő oldalt) ezért párhuzamos teljesítmény, vagy skálázódási megfontolások nem igénylik aszinkron architektúra kialakítását az EventHub-on! Ha kérés/válasz jellegű, vagy visszajelzett implementációkra van szükségünk, preferáljuk az EventHub szinkron eszközeit!
* **Visszajelzés nélküli architektúrának** azt nevezzük, amikor a küldő oldal egyszerűen csak bedobálja az üzeneteit a kommunikációs csatornára, és azok feldolgozásáról semmilyen visszajelzést nem vár.
* Egy EventHub üzenetet a terminológiánkban **üzenet**nek (**message**) nevezünk.
* Egy olyan üzenetet amelyre valamilyen módon (szinkron, vagy aszinkron) választ várunk **kérésnek** (**request**), a rá adandó választ **válasz**nak (**response**) nevezzük.
* Az EventHub-on a **request/response definíciók mindig párban értendőek**. Ha megengedett, hogy egy adott kérésre többféle típusú válasz létezhessen akkor azok külön request/response definíciót jelentenek. Tehát az üzenetpárok esetén a típusvezérlést mindig a kérés és az arra adott válasz típusának párja adja.
* A **típus vezéreltség** azt jelenti az EventHub-on, hogy a küldő oldal mindig egy egyértelmű típusú üzenetet küld (request/response pár esetén egyértelműen meghatározott típusú kérést küld, és arra egy egyértelműen meghatározott típusú választ vár). A fogadó oldalon pedig egy adott típusú üzenetet (request/response párnál a kérés és a válasz típusa együttesen adja a definíciót) mindig ugyanaz a kezelő (handler) dolgoz fel.
* Az EventHub-on **Handler**-nek (**Kezelő**nek) nevezzük azokat a metódusokat melyek egy adott típusú üzenet, vagy üzenetpár feldolgozását végzik.
* A **típus biztonság** (**Typesafe**) azt jelenti az EventHub-on, hogy a keret garantálja, hogy ha egy adott típusú üzenetet elküldünk a kommunikációs csatorna egyik végpontján, akkor az a másik oldalra a küldött típussal azonos típusként, és ennek a típusnak a feldolgozását végző kezelőbe (handler) érkezik.
* Az EventHub-on **végpont**nak (**EndPoint**) nevezzük azokat az alkalmazás elemeket, amelyek egy közös kommunikációs csatorna felett működnek. Az EventHub-on nem létezik olyan megkötés, amelyik azt mondaná, hogy egy csatornának két végpontból kell állnia. Tetszőleges háló jellegű kommunikációs architektúrák felépítésére alkalmas, melyen akár ugyanazt az aspektust tekintve is lehet nem csak tetszőleges számú kliens (küldő), de tetszőleges számú fogadó végpont is.
* Az EventHub terminológiájában **Egyezmény**nek (**Contract**) nevezük azt az elméleti definíciót, melyet egy adott küldő/fogadó pár a közlekedő kommunikációs üzenetek típusán, illetve azok viszonyán keresztül meghatároz. Ezt az egyezményt kódoldalon egy osztályhierarchia képében formalizálhatjuk. Egy adott kommunikációs csatorna tetszőleges számú ilyen egyezményt valósíthat meg.
* Az EventHub használatához mindig definiálnunk kell a használni kívánt átviteli csatornát is. Ezt **EventHub Protkoll**nak, vagy csatorna típusnak nevezzük.
* Egy adott EventHub kommunikációs csatornát egyszerűen annak nevével (**ChannelName**) azonosítunk. Keret szinten semmilyen megkötés nem létezik az EventHub csatornák nevére vonatkozóan, az egyediségükön túl. Az egyediség case-sensitive stringként értendő.
* Egy teljes EventHub kommunikációs kapcsolatot tehát a használt átviteli csatorna típusa, a csatorna neve és a rajta használt szerződés(ek) együttesen határozzák meg.
* Az EventHub-ban azt a metodust, amelyet egy adott üzenettípus feldolgozására kijelölünk **Handler**-nek (kezelőnek) nevezünk.
* Azt az aktust, amivel az EventHub keret tudomására juttatjuk, hogyha egy adott csatornán adott típusú üzenet érkezik, azt melyik kezelőhöz kell továbbítania kezelő regisztrációnak (**Handler registration**) nevezzük.
* Azt az aktust, amikor egy EventHub végponton (endpoint) bejegyzünk egy adott üzenetet fogadó, azt feldolgozó kezelőt (handler) egy csatornára (csatorna típus + csatorna azonosító) **publikáció**nak nevezzük, ami azt jelenti, hogy az adott képességet (hogy az üzenetet fogadja és meghatározott módon feldolgozza) publikáltuk az EventHub infrastruktúrán a megadott csatorna felett.
* Az EventHub tulajdonképpeni használata üzenetek küldéséből és/vagy kezelők regisztrációjából áll.

### Jelenleg létező csatorna implementációk
Az EnventHub csatorna tehát a közvetítő kommunikációs közeget jelenti. A különféle kommunikációs csatornák az EventHub használati implementációjára és elveire nincsenek kihatással. Kihatással lehetnek ugyanakkor a kommunikáció viselkedésére, teljesítményére és az üzenettovábbítás infrastrukturális és belső viselkedést meghatározó részleteire.
A csatorna implementációk az EventHub használati módjában majd ott jutnak szerephez, hogy az EventHubCore szolgáltatásai mindig generikus hívásokon keresztül érhetőek el, amelyek a csatorna típusát is kijelölik az ennek meghatározását szolgáló típusparaméterükben (Type parameter).
Az alábbiakban felsorolt létező protokoll implementációknál megadjuk a használandó típus paramétert amely az adott csatornát kijelöli.
#### Redis Pub/Sub csatorna: Vrh.EventHub.Protocols.RedisPubSub
Az átvitel a [Redis](http://www.redis.io) Key-value store adattároló megoldásba épített [Pub/Sub üzenetközvetítő szolgáltatásra](https://redis.io/topics/pubsub) épül. Közismertnek tekintve, nem tárgyaljuk ennek részletes jellemzőit e-helyen, de kiemeljük pár tulajdonságát, amelyik a csatorna implementációnk viselkedését is meghatározza:
* Rendkivül gyors
* Csatorna alapú, ahol adott csatornára tetszőleges számú Publikáló (Publisher) és feliratkozó (Subscriber) jelentkezhet fel. Így tetszőleges háló rendszerű, irányfüggetlen kommunikáció kialakítására éppúgy alkalmas, mint egy-egy végpont közti egyirányú kommunikációra.
* A publikálók számára azt garantálja, hogy a csatornába küldött üzeneteiket az összes a küldés pillanatában a csatornára csatlakozó feliratkozó megkapja. A feliratkozók számára pedig azt garantálja, hogy ha egy Publisher üzenetet küld a csatornára akkor aki feliratkozott rá az meg is kapja azt.
* Fontos kritérium ugyanakkor az egyidejűség. Nincs üzenettartás, vagy bármilyen utólagos üzenettovábbító mechanizmus. Ha valaki feliratkozik egy csatornára (Subscribe), akkor a múltban a csatornán közlekedő üzenetekről semmilyen formában nincs módja információt szerezni.

A Redis Pub/Sub alapú EventHub átviteli csatorna használatához a **Vrh.EventHub.Protocols.RedisPubSub Nuget csomag**ot kell telepíteni, amely függőségként hozza magával az EventHub.Core csomagot is.

A csatorna használatához a **Vrh.EventHub.Protocols.RedisPubSub névtér** include-olandó:
```csharp
using Vrh.EventHub.Protocols.RedisPubSub;
```
A csatorna hivatkozási típusneve: **RedisPubSubChannel**

A csatorna további részleteit, a konfigurációjára vonatkozó információkat a csatorna dokumentációja tartalmazza.

#### Alkalmazáson belüli csatorna: Vrh.EventHub.Protocols.InsideApplication
Ez a csatorna implementáció csak alkalmazáson belül (pontosabban: egyazon .Net alkalmazástéren (Application Domain) belül) képes működni. Mivel csak egyetlen szérializációt és az átviteli költséget spórolja meg (amely a Redis Pub/SUB csatornán rendkívül alacsony), ezért nem nyújt annyival jobb teljesítményt, hogy önmagában emiatt érdemes lenne ezt választani. Használatát inkább a Redis infrastruktúra megspórolása indokolhatja. 

Szem előtt kell azonban tartani, hogy kommunikáció csak azonos alkalmazás téren belüli modulok között lehetséges, és ha szét kell választani az architektúrát valami miatt oly módon, hogy a kommunikáló modulok egy része külön alkalmazástérbe kerül, akkor az eredeti kód refaktorálására lesz szükség.

A csatorna használatához a **Vrh.EventHub.Protocols.InsideApplication nuget csomag** telepítendő, amely függőségként hozza magával az EventHub.Core csomagot is.

A csatorna használatához a **Vrh.EventHub.Protocols.InsideApplication névtér** include-olandó:
```csharp
using Vrh.EventHub.Protocols.InsideApplication;
```

### Az EventHub felhasználása alkalmazás(modul)ok közti kommunikáció kialakítására

Most, hogy az alapterminológiánk nem idegen már, és megismertük a létező csatorna implementációkat is rátérhetünk, hogyan kell használnunk az EventHub-ot, hogy alkalmazások, vagy alkalmazás modulok közti kommunikációt valósítsunk meg.

Az EventHub minden képessége az **EventHubCore** nevű static típuson keresztül érhető el.
Az EventHub legfontosabb használati elve hogy nem kell infrastruktúrát inicializálni a használatához, minden használati eset önmagában teljes definíciót hordoz, és az infrastruktúra elemek a hátérben inicializálásra, nyilvántartásra és újrafelhasználásra kerülnek. Ezen definíciók, részben generikus típus paraméterek megadásán, részben konkrét hívási paramétereken keresztül definiálódnak.

Az infrastruktúra inicializációk pedig az első híváskor automatikusan történnek (az első hívások költségéhez adódik). Az infrastruktúrával való gazdálkodást teljes egészében a komponens és az alatta dolgozó átviteli csatorna implementáció végzi. Ez alól - mint látni fogjuk - egyedül az érkező üzenetek fogadását végző kezelők (handler) regisztrációja, és e regisztrációk megszüntetése képez, illetve képezhet kivételt.
#### Alapvető EventHub típusok
##### Hibakezelés: EventHubException, FatalEventHubException
Minden EventHub müködési hiba egy **EventHubException** típuson keresztül megy vissza a hívó hely felé. Az EventHubException az Exception osztály leszármazottja. A hibakezelés két szintű, létezik egy EventHubException leszármazott, a **FatalEventHubException**, amely olyan végzetes (jellemzően infrastruktúrával kapcsolatos) hibák esetén megy vissza a hívási hely felé, amelyek az EventHub üzenetküldést meggátolják. A különbség még abban van a két hiba típus tekintetében, hogy az EventHub Core belsejélből származó hibák esetén a beállításoktól függően az EventHubException szintű hibák elnyelődhetnek, míg a FatalEventHubException mindig továbbterjed a Core fölé. (Az erre vonatkozó beállítási lehetőséget lásd később a konfigurációt taglaló részben. Megjegyzendő, még, hogy a protokoll csatorna implementációk ettől eltérő viselkedést mutathatnak, annak részleteit lásd az adott csatorna dokumentációjában.) 
##### Request generikus típus (`Request<TRequest, TResponse>`)
Az EventHub a szinkron hívások tekintetében tartalmaz egy builtin üzenetpárosító megoldást, amely két wrapper jellegű üzenettípusra épül, az egyik a kérést (request), a másik a választ (response) hordozza. Ezeket bizonyos implementációk kialakításához ismerni, és használni kell. A használat részleteit az adott helyen tárgyaljuk, de elöljáróban ismerkedjünk meg e két típussal.
A kéréseket az EventHub Core a Request generikus típusba csomagolva továbbítja, a fogadó oldalon mindig találkozunk ezzel a típussal a szinkron szolgáltatáshívások kialakításánál, de felhasználható aszinkron szolgáltatás logikák építésére is. A Request típus teljes definíciója az alábbi:
```csharp
/// <summary>
/// Egy olyan üzenet, amelyre a feldolgozó oldalnak választ kell adnia
/// </summary>
/// <typeparam name="TRequest">Kérés típusa</typeparam>
/// <typeparam name="TResponse">Válasz típusa</typeparam>
public class Request<TRequest, TResponse>
    where TRequest : new()
    where TResponse : new()
{
    /// <summary>
    /// Üzenet azonosító
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Maga az üzenet
    /// </summary>
    public TRequest RequestContent { get; set; }

    /// <summary>
    /// Visszaad egy üres (kitöltetlen) response üzenetet. 
    ///   Amely ehhez az üzenethez (Request) tartozik
    ///   (A típushelyesség elősegítésére és az üzenetpárosításra használandó fel!)
    /// </summary>
    [JsonIgnore]
    public Response<TResponse> MyResponse => Response<TResponse>.ResponseFactory(Id);
}
```
Mint látható a generikus mind a kérés, mind az arra adandó válasz típusát előírja a típusparamétereiben:
* **TRequest type parameter**: A kérés konkrét típusa.
* **TResponse type parameter**: A kérésre adott válasz típusa.
* **Id property**: Üzenet azonosító, a típus egy GUID-dal inicializálja, de egyedi üzenetazonosítás implementációja esetén ez felülírható tetszőleges string tartalommal, az EventHub Core müködése nem függ az azonosító tartalmától, amíg annak egyedisége valamilyen módon garantált, addig garantáltak a belső üzenetpárosító mechanizmusok is a szinkron hívásokra!
* **RequestContent property**: Az a property tárolja a tényleges üzenet (késés) típusnak (TRequest), az adott kérés érdemi tartalmát reprezentáló objektumpéldányát. 
* **MyResponse**: A MyResponse property egy kvázi gyártó függvény, amely garantáltan az adott kéréshez párosított választ (response) fogja visszaadni. Ez a gyakorlatban azt jelenti, hogy egy új TResponse típust csomagoló generikus Response objektumpéldányt kapunk, amelynek RequestId-ja (lásd a Response generikus típus leírását lejjebb) az eredeti üzenet (request) Id-jával egyezik meg. Ez a property (kvázi gyártó függvény) elősegíti, hogy a feldolgozó oldal implementációjában megalkossuk az EventHub Core üzenetpárosítása szerint garantáltan a kapott kéréshez tartozó válasz üzenetet.

##### Response generikus típus (`Response<T>`)
Adott kéréshez tartozó válasz tárolására szolgáló üzenetek generikus wrapper típusa. A publikus részeinek  definiciója az EventHub Core-ban:
```csharp
/// <summary>
/// Válasz üzenet egy Eventhub üzenetre
/// </summary>
/// <typeparam name="T"></typeparam>
public class Response<T>
    where T : new()
{
    /// <summary>
    /// Maga az üzenet
    /// </summary>
    public T ResponseContent { get; set; }

    /// <summary>
    /// Sikeres a válasz, vagy hiba lépett fel? 
    /// </summary>
    public bool Ok => Exception == null;

    /// <summary>
    /// Hiba esetén kell tartalmazni egy Exceprtion-t, vagy egy Exception leszármazottat
    /// Ha nincs hiba akkor kötelezően null
    /// </summary>
    public Exception Exception { get; set; } = null;

    /// <summary>
    /// Üzenet azonosító (a request-tel párosítja)
    /// </summary>
    public string RequestId { get; set; }
}
```
* **T type parameter**: A T típus paraméter mondja meg, hogy milyen típust csomagol ténylegesen a Response.
* **ResponseContent property**: A tényleges választ tartalmazó T típusú objektum példány. Megengedett, hogy ne legyen kitöltve (null érték), amennyiben a válaszban egy hibát (Exception) jelzünk vissza.
* **Ok property**: Az Ok property azt mondja meg, hogy a kapott válasz üzenet egy hibaüzenet, vagy egy normál válasz üzenet-e? Amennyiben az Exception property nincs kitöltve (null) a Response objektum példányban, úgy az Ok property értéke true, ha az Exception nem null, akkor az Ok property értéke false.
* **Exception property**: Ha a feldolgozó oldal egy hibát szeretne a hívó oldalnak visszaküldeni a normál válasz helyett, akkor az Exception property segítségével teheti ezt meg. Az Exception property Exception típusú, így abba bármilyen Exception leszármazót elhelyezhető összhangban a .Net hibakezelési konvencióival.
* **RequestId property**: A kérés azonosítója, amihez ez a válasz tartozik. Szinkron hívások esetén az EventHub Core automatikusan kezeli. Aszinkron implementációk esetén is kihasználható, hogy az üzenetpárosítással kapcsolatban kihasználjuk a Core built-in szolgáltatásait, megkönnyítve ezáltal az egyedi aszinkron implementációk üzenetpárosításának megvalósítását.

Bár a Response típust közvetlenül is példányosíthatjuk a (a szérializáció miatt ezt nem lehet tiltani) a valódi használat során nem példányosítjuk sosem közvetlenül, hanem a feldolgozó által fogadott Request objektum MyResponse propertyjén keresztül szerezzük be a visszaküldendő válasz objektum példányunkat az alábbi code snipettel összhangban:
```csharp
var response = request.MyResponse;
```
Ahol request a fogadott kérés objektum példánya. Ezt követően annak függvényében, hogy a feldolgozás hibát akar visszajelezni, vagy egy tényleges választ visszaküldeni beállítjuk a megszerzett objektum Exception vagy ResponseContent property-jét az alábbi code snipet-hez hasonlóan:
```csharp
var response = request.MyResponse;
try
{
    // response feldolgozása hibakezeléssel
    ...
    var responseContent = new TRespnse()
    {
        PropertyA = 1,
        PropertyB = "BpropertyValue",
    };
    response.ResponseContent = responseContent;
}
catch(Exception e)
{
    response.Exception = e;
}
return response;
```

#### Csatorna inicializáció

Az **EventHubCore** stattikus típus tartalmaz egy **InitielizeChannel** metódust, segítségével egy adott csatorna példányt inicializálni lehet abban az EventHub végpontban, ahol a hívás történik.
Bár az EventHub nem igényel csatorna inicializációt, de bizonyos esetekben ennek használata indokolt lehet.
* Nem akarjuk, hogy az első híváés késleltetést szenvedjen a működési logikában, azáltal, hogy a csatorna infrastruktúra inicializációja ráterhelődik.
* Erősen túlterhelt párhuzamos környezetben a csatornainicializáció kiemelése a indokolt lehet, az inicializáció garantált és hibamentes megtörténtéhez. Ilyen például aRedis pub/sub csatorna megvalósítás.

Az inicializációt **InitielizeChannel** mindig szinkron végezzük, garantáljuk, hoyga a hívás soha ne kerüljön ThreadPool-ba!

Használati mintája:
```csharp
EventHubCore.InitielizeChannel<RedisPubSubChannel>("sampleservice");
```

#### Üzenetek küldése

Az üzenetek küldésére háromféle mód létezik az EventHub alatt. Egy szinkron, és két aszinkron küldési mechanizmus.

A szinkron küldést **Call**-nak hívjuk, és nem csak minden esetben választ vár a fogadó oldaltól, de minden esetben meg is várja a válasz beérkezését vagy timeoutot ad, ha mindez nem történik meg egy definiált határidőn belül.

Az egyik aszinkron küldést **Send**-nek hívjuk az EventHub-on és a hívási helyen semmilyen módon nem kaphat választ a fogadó oldaltól. Értesítést kap (Exception) arról, ha az üzenet elküldése infrastrukturális értelemben sikertelen. (Jellemzően nem működik az átviteli infrastruktúra.)

A másik aszinkron küldést **SendAsync**-nek hívjuk az EventHub-on és a hívási hely semmilyen közvetlen módszerrel nem kaphat választ sem a fogadó oldaltól, sem az esetleges infrastruktúra (átviteli) hibákról. Előnye viszont ennek a módszernek a rendkívüli átbocsátási képessége, és ezzel együtt a minimális hívó oldali blokkolása (hívási költsége). Pár Millió üzenet/másodperc átbocsátási képességről, és ennek megfelelően 1/pár millió másodperc hívási költségről beszélünk. (Az EventHub teljesítmény adatait egy későbbi fejezet tárgyalja.) Például logolásnál lehet értelme a használatának, ahol jellemzően nem szeretnénk, hogy a log infrastruktúra hibája leállítsa az üzleti folyamatokat.

##### Call
A szinkron hívást (**Call**), akkor használjuk, ha a küldő oldal választ vár a feldolgozó oldaltól, az ilyen hívási módot az EventHub-on Call-nak, vagy szinkron hívásnak hívjuk. Fő jellemzői: 
* A szinkron hívásoknak mindig van visszatérő típusa!
* A szinkron hívások mindig blokkolják a hívási helyet, amíg a válasz vissza nem érkezik, vagy le nem telik a definiált timeout.
Az EventHub statikus típusban a Call szignatúrája az alábbi:
```csharp
public static TResponse Call<TChannel, TRequest, TResponse>(string channelId, TRequest request, TimeSpan? timeOut = null)
    where TRequest : new()
    where TResponse : new()
    where TChannel : BaseChannel, new()
```

Mint látható ez a hívás egy három típusparaméterrel (type parameter) rendelkező generikus metódus, mely két kötelező és egy opcionális normál metódus paraméterrel is rendelkezik. A paraméterek magyarázata:
* **TChannel**: A csatornát meghatározó típus paraméter a generikusban. Értéke egy létező, és a kódbázison jelenlévő csatorna implementációs típus kell legyen, például **RedisPubSubChannel**. (**BaseChannel** leszármazott. A választható csatorna implementációkat az előző fejezet sorolja fel. De az EventHub Core felett tetszőleges egyedi csatorna implementációk alakíthatóak ki a Csatorna fejlesztést leíró későbbi fejezetben leírtak szerint.)
* **TRequest**: Ez a típus paraméter meghatározza a generikusban, hogy milyen típusú kérést küldünk a hívással, egyben ilyen lesz a második metódus paraméterben megadott (a tulajdonképpen elküldésre kerülő) request objektumpéldány típusa.
* **TResponse**: Ez a típus paraméter határozza meg a generikusban, hogy az elküldött kérésre (request) milyen típusú választ várunk.
* **channelId**: A channelId string paraméter definiálja azt a csatorna azonosítót, melyre az üzenetet küldjük. Mivel a csatorna nevek (id) egy egyezményt reprezentálnak (kinek küldjük) ezeket célszerű nem stringként megadni minden hívási helyen, hanem valamilyen módon kiemelni pl. konfigurációba.
* **request**: Maga az elküldött üzenet (kérés). TRequest típus egy példánya, amely a kérést reprezentálja.
* **timeOut**: Opcionális TimeSpan paraméter, amely a hívás timeout-ját definiálja, ha nem adjuk meg akkor a használt csatornán definiált timeout jut érvényre. A timoeoutok egyedi definiálhatóságát az indokolja, hogy mind az EventHub keret, mind a használt átviteli csatornák maximális normál késleltetése akár több nagyságrenddel kisebb lehet egy adott kérés tényleges feldolgozási időszükségletének. Így bizonyos hosszabb válaszidőket produkáló kérések esetén testre szabható a maximális válasz idő.

A **Call** a válasz beérkezéséig blokkolja a hívási hely végrehajtását (mind bármelyik normál szinkron metódus).
Amennyiben a fogadó oldal válasza nem érkezik meg a timeout-on belül a hívó oldalra, akkor a végrehajtási száll blokkolása megszűnik, és egy **FatalEventHubException** megy vissza a hívás helyére.
Példa a call használatára:
```csharp
SampleResult result = EventHubCore.Call<RedisPubSubChannel, SampleRequest, SampleResult>(
                        "sampleservice", 
                        new SampleRequest() { SampleRequestData = 7 },
                        new TimeSpan(0, 0, 5));
```
Mint látható a felhasználási oldalon:
* Nincs szükség semmilyen infrastruktúra inicializációra egyszerűen meghívjuk a Call-t ahol arra szükség van, olyan módon, mintha egy tetszőleges szinkron metódus hívást végeznénk.
* Az EventHub Core gondoskodik róla, hogy a küldött kérés továbbításra kerüljön a sampleservice névvel azonosított csatorna felé, és megkezdődjön a válaszra való várakozás.
* *(Az EventHub Core a belsejében a küldött SampleRequest típust becsomagolja egy Request<SampleRequest, SampleResponse> generikus típusba, majd serializálja azt és gondoskodik a csatornát specifikáló (TChannel) típus paraméter (a példában: RedisPubSubChannel) szerinti csatorna infrastruktúrára továbbításáról. Ezt követően megkezdi a válasz csatornán való megjelenésére való várakozást. A válasz minden esetben egy szérilaizált `Response<SampleResponse>` típus lesz. Ha a csatornán megjelenik egy olyan `Response<Sampleresponse>` objektumpéldány, amelyre igaz, hogy RequestId propertyje egyenlő az elküldött Request objektum Core által generált ID-jával, akkor a keret megvizsgálja, hogy a kapott `Response<SampleResponse>` objektum példány Ok property-je true értéket ad-e. Ha igen, akkor a kapott `Response<SampleResponse>` objektum példány ConcrateResponse tartalmát visszaadja. Amennyiben az Ok property értéke false, akkor igaz, hogy az Exception != null, és ekkor a keret kiváltja a válaszban érkezett Exception-t. Ha a válasz nem érkezik meg a példában definiált 5 másodpercen belül akkor egy FatalEventHubException váltodik ki (timeout). Ezen túl különféle Exception típusok jöhetnek még a használt csatorna implementációból, az átviteli infrastruktúra esetleges hibái miatt.)*
* Bármilyen fellépő hiba esetén (beleértve, hogy a feldolgozó oldal egy feldolgozási hibát akar visszajelezni egy érvényes válasz objektum visszaküldése helyett) Exception váltodik ki a hívási helyen.
* Amennyiben semmilyen hiba nem lép fel, és a feldolgozó oldal egy érvényes válasz objektumot küld, akkor a keret gondoskodik róla, hogy a Call hívás ezzel térjen vissza a hívási helyre.

Fenti információk alapján a **Call** használati mintája valami ilyesmi:
```csharp
try
{
    // Kérés objektum összeállítása:
    var request = new SampleRequest()
    {
        request.PpopertyA = 7,
        request.PropertyB = false,
    };
    ...
    var result = EventHubCore.Call<RedisPubSubChannel, SampleRequest, SampleResult>(
                        // A csatorna azonosítót egy valós implementációban 
                        //  valamilyen programlogika, vagy konfiguráció adja.
                        // NE HASZNÁLJUNK KÓDBA ÍRT STRINGEKET!!!
                        _myConfigutóration.ChannelId, 
                        // a kérést reprezentáló konkrét objektum
                        request,
                        // Timout, ha nem szükséges a csatorna alapbeállításától eltérni, elhagyható
                        new TimeSpan(0, 0, 5));
    // A válasz feldolgozása
    ...
}
catch(Exception e)
{
    // Hibakezelés implementációja
    ...
}

```

> **Soha ne dobjuk a Call hívást threadpoolba (pl. Task használatával)! Amennyiben mégis szükség lenne ilyenre, akkor TaskFactory-t kell használni, és kötelezően megadandó a TaskCreationOptions paramétert LongRunning-ra kell állítani! Ellenkező esetben az EventHubCore nem tudja garantálni, hogy az adott csatorna működése a Call hívások tekintetében hibamenetes lesz!**

##### Send
A **Send** olyan üzenetek elküldésére szolgál, amelyek feldolgozásának eredményéről nem várunk választ. Vagy legalábbis nem a küldés helyén várjuk azt (aszinkron logikák). A Send hívása a hívó kód oldalán szinkron történik, a hívó hely az üzenet kommunikációs csatornába továbbításának idejére blokkolódik, és ennek megfelelően hibajelzés az infrastruktúra működőképességéről van csak. Ha az üzenet kommunikációs csatornába küldése sikeres, akkor sem annak a fogadó oldali kivételéről, sem feldolgozásáról semmilyen módon nem jut vissza a hívó helyre információ.
Ennek megfelelően a **Send** void visszatérési típussal rendelkezik. Mivel az EventHub keret támogatja a kereten kívül megvalósított aszinkron üzenetközvetítő logikák megvalósítását, és maga is egy ilyen megvalósítással dolgozik a szinkron hívásai (Call) tekintetében, ezért a Send metódusa egy erősen túlterhelt metódus az EventHubCore static típusban.
Mindegyik változatára igazak az alábbiak:
* A hívás kizárólag csak közvetítő csatorna infrastruktúrával kapcsolatos hibák esetén ad Exception-öket. (Ezek a használt csatorna implementációtól függenek.)
* A hívási hely az üzenetnek a közvetítő csatornába juttatásának idejére blokkolódik. Ha ez sikeresen megtörténik a Send hívás visszatér, definíciójának megfelelően visszatérési érték nélkül (void).
* Önmagában az EventHub Core semmilyen módon nem garantálja aszinkron implementációkra a hívási flow globális tartását. Tehát egy előbb elküldött kérésre később érkezhet meg a válasz, mint egy ezt követően elküldöttre. Maga a küldési sorrend a csatorna implementáció tulajdonságainak függvényében lesz flowtartó, vagy sem.

Fentieknek megfelelően az összes **Send** változat használati esete megfelel az alábbi code snippetben használt elveknek:
```csharp
try
{
    var message = new SampleMessage();
    EventHubCore.Send<RedisPubSubChannel, SampleMessage>(
                            // A csatorna azonosítót egy valós implementációban 
                            //  valamilyen programlogika, vagy konfiguráció adja.
                            // NE HASZNÁLJUNK KÓDBA ÍRT STRINGEKET!!!
                            _myConfigutóration.ChannelId, 
                            message);
}
catch(Exception e)
{
    // Hibakezelés implementációja (csak átviteli hibák)
    ...
}
```

###### 1. Send változat: tetszőleges üzenet aszinkron elküldéséhez
```csharp
/// <summary>
/// Aszinkron üzenetküldés!
///     Olyan üzenetek elküldésére használandó, 
///     amelyre a küldő oldal semmilyen módon nem vár választ. 
/// A küldés során keletkező Exception-ök (Pl. közvetítő infrastruktúra hibái) 
///	felmennek a hívási helyre. 
/// </summary>
/// <typeparam name="TChannel">Csatorna típusa</typeparam>
/// <typeparam name="TMessage">Üzenet típusa</typeparam>
/// <param name="channelId">Csatorna azonosító</param>
/// <param name="message">üzenet</param>
public static void Send<TChannel, TMessage>(string channelId, TMessage message)
    where TMessage : new()
    where TChannel : BaseChannel, new()
```
Ez a Send metódus változat 2 type paraméterrel rendelkező generikus metódus, és tipikusan arra használjunk, hogy valamilyen üzenetet elküldjünk egy fogadó oldal számára egy aszinkron küldési implementációban:
* **TChannel type parameter**: A csatornát meghatározó típus paraméter a generikusban. Értékének egy létező, és a kódbázison jelenlévő csatorna implementációs típusnak kell lennie (BaseChannel leszármazott), például **RedisPubSubChannel**.
* **TMessage** type parameter: A ténylegesen küldött üzenet típusát határozza meg.
* **channelId paraméter**: A channelId string paraméter definiálja azt a csatorna azonosítót, melyre az üzenetet küldjük. Mivel a csatorna nevek (id) egy egyezményt reprezentálnak (kinek küldjük) ezeket célszerű nem stringként megadni minden hívási helyen, hanem valamilyen módon kiemelni pl. konfigurációba.
* **message paraméter**: Az elküldött üzenetet reprezentáló TMessage típusú objektum példány, a tulajdonképpeni üzenet.
 
Használati minta ezen Send változathoz:
```csharp
try
{
    var message = new SampleMessage();
    EventHubCore.Send<RedisPubSubChannel, SampleMessage>(
                        // A csatorna azonosítót egy valós implementációban 
                        //  valamilyen programlogika, vagy konfiguráció adja.
                        // NE HASZNÁLJUNK KÓDBA ÍRT STRINGEKET!!!
                        _myConfigutóration.ChannelId, 
                        message);
}
catch(Exception e)
{
    // Hibakezelés implementációja (csak átviteli hibák)
    ...
}
```

###### 2. Send változat: Az EventHube Core builtin wrapper típusainak szolgáltatásaival működő aszinkron request/response logikák request (kérés) oldali üzeneteinek elküldéséhez
```csharp
/// <summary>
/// Aszinkron Request üzenetküldés: 
///         Kérés oldal -> Elküld egy Request-et 
/// A küldés során keletkező exception-ök (Pl. közvetítő infrastruktúra hibái) 
///	felmennek a hívási helyre.
/// </summary>
/// <typeparam name="TChannel">Csatorna implementáció típusa</typeparam>
/// <typeparam name="TRequest">Kérés típusa</typeparam>
/// <typeparam name="TResponse">Kérés típusa</typeparam>        
/// <param name="channelId">Csatorna azonosító, ahová küldi</param>
/// <param name="request">Kérés üzenet</param>
public static void Send<TChannel, TRequest, TResponse>(
                                string channelId, Request<TRequest, TResponse> request)
    where TRequest : new()
    where TResponse : new()
    where TChannel : BaseChannel, new()
```
Ez a Send metódus változat 3 type paraméterrel rendelkező generikus metódus, és tipikusan arra használjunk, hogy egy kérést (request) továbbítsunk a fogadó oldal számára egy aszinkron küldési implementációban. (Érdekességként: maga az EventHub Core is ezt használja egy szinkron hívás (Call) küldő végponti oldalán a szinkron hívások megvalósítási kódjában.) A paraméterek magyarázata:
* **TChannel type parameter**: A csatornát meghatározó típus paraméter a generikusban. Értékének egy létező, és a kódbázison jelenlévő csatorna implementációs típusnak kell lennie (BaseChannel leszármazott), például **RedisPubSubChannel**.
* **TRequest** type parameter: Az elküldött kérés tényleges típusát határozza meg. (Lásd még a `Request<TRequest, TResponse>` generikus wrapper típusnál leírtakat!)
* **TRespons**: Az elküldött kérésre ilyen típusú objektumot várunk válaszul (Response). (Lásd még a `Request<TRequest, TResponse>` generikus wrapper típusnál leírtakat!)
* **channelId paraméter**: A channelId string paraméter definiálja azt a csatorna azonosítót, melyre az üzenetet küldjük. Mivel a csatorna nevek (id) egy egyezményt reprezentálnak (kinek küldjük) ezeket célszerű nem stringként megadni minden hívási helyen, hanem valamilyen módon kiemelni pl. konfigurációba.
* request paraméter: Maga a tényleges kérést reprezentáló, `Request<TRequest, TResponse>` típusú generikus objektum példány.

Használati minta ezen Send változathoz:
```csharp
try
{
    var request = new SampleRequest()
    {
        // Property init
        ...
    };
    var requestWrapper = new Request<SampleRequest, SampleResponse>()
    {
        RequestContent = request,
    };
    EventHubCore.Send<RedisPubSubChannel, SampleRequest, SampleResponse>(
                        // A csatorna azonosítót egy valós implementációban 
                        //  valamilyen programlogika, vagy konfiguráció adja.
                        // NE HASZNÁLJUNK KÓDBA ÍRT STRINGEKET!!!
                        _myConfigutóration.ChannelId, 
                        requestWrapper,
                        );
}
catch(Exception e)
{
    // Hibakezelés implementációja (csak átviteli hibák)
    ...
}
```

###### 3. Send változat: Az EventHube Core builtin wrapper típusainak szolgáltatásaival működő aszinkron request/response logikák response (válasz) oldali üzeneteinek elküldéséhez
```csharp
/// <summary>
/// Aszinkron Response üzenetküldés: 
///     Válasz oldal -> Elküld egy Response-t 
/// A küldés során keletkező Exception-ök (Pl. közvetítő infrastruktúra hibái) 
///	felmennek a hívási helyre 
/// </summary>
/// <typeparam name="TChannel">Csatorna implementáció típusa</typeparam>
/// <typeparam name="TResponse">Kérés típusa</typeparam>        
/// <param name="channelId">Csatorna azonosító, ahová küldi</param>
/// <param name="response">Kérés üzenet</param>
public static void Send<TChannel, TResponse>(
                string channelId, Response<TResponse> response)
    where TResponse : new()
    where TChannel : BaseChannel, new()
```
Ez a Send metódus változat 2 type paraméterrel rendelkező generikus metódus, és tipikusan arra használjunk, hogy egy választ (response) továbbítsunk a fogadó oldal számára egy aszinkron küldési implementációban. (Érdekességként: maga az EventHub Core is ezt használja egy szinkron hívás (Call) fogadó végponti oldalán, hogy visszajutassa a kérést (request) kezelő handler-ből a szinkron hívás hívó oldalára az arra adott választ (respopnse).) A paraméterek magyarázata:
* **TChannel type parameter**: A csatornát meghatározó típus paraméter a generikusban. Értékének egy létező, és a kódbázison jelenlévő csatorna implementációs típusnak kell lennie (BaseChannel leszármazott), például **RedisPubSubChannel**.
* **TRespons**: Egy kérésre (request) adott, válasz típusa (Response). (Lásd még a `Request<TRequest, TResponse>` és a `Response<TResponse>` generikus wrapper típusoknál leírtakat!)
* **channelId paraméter**: A channelId string paraméter definiálja azt a csatorna azonosítót, melyre az üzenetet küldjük. Mivel a csatorna nevek (id) egy egyezményt reprezentálnak (kinek küldjük) ezeket célszerű nem stringként megadni minden hívási helyen, hanem valamilyen módon kiemelni pl. konfigurációba.
* response paraméter: Maga a tényleges választ reprezentáló, `Response<TResponse>` típusú generikus objektum példány.

Használati minta ezen Send változathoz:
```csharp
try
{
    var response = new SampleResponse()
    {
        // Property init
        ...
    };
    var responseWrapper = new Response<SampleResponse>()
    {
        ResponseContent = response,
    };
    EventHubCore.Send<RedisPubSubChannel, SampleResponse>(
                        // A csatorna azonosítót egy valós implementációban 
                        //  valamilyen programlogika, vagy konfiguráció adja.
                        // NE HASZNÁLJUNK KÓDBA ÍRT STRINGEKET!!!
                        _myConfigutóration.ChannelId, 
                        responseWrapper,
                        );
}
catch(Exception e)
{
    // Hibakezelés implementációja (csak átviteli hibák)
    ...
}
```

##### SendAsync
Előfordulhat, hogy olyan aszinkron hívásra van szükség üzenetek elküldésére, amelyekkel szemben a nagy átbocsátó képesség és a kis küldő oldali késleltetés a lényeges szempontok, a küldés és a feldolgozás tényleges sikeressége pedig lényegtelen a hívó oldalán. (Ilyen lehet például egy logoló megoldás kialakítása. Vagy egy online, működést monitorozó megoldás. Bármi ahol nem akarjuk hogy egy kiegészítő/kísérő funkció (infrastrukturális) működésképtelensége kihatással legyen az eredeti üzleti implementáció működési biztonságára.)
Az Eventhub keret erre a célra a **SendAsync** metódusát biztosítja. Ennek lényege, hogy a hívási helyen a hívást azonnal egy háttér száll várósorra dobja (Task), majd visszatér. A tényleges küldés az adott Task végrehajtásra kerülésekor történik. Természetesen a valódi aszinkron háttérfeldolgozás sajátosságainak megfelelően a működést az alábbiak jellemzik:
* A sorrendtartást semmi nem garantálja, tehát hiába hívtunk előbb egy SendAsync üzenetküldést, attól egy később hívott tényleges elküldése már a küldő oldalon is előbb bekövetkezhet.
* A hívó blokkolása azonnal megszűnik, amint a Task a queue-ba került.
* Ha bármilyen Exception történik a tényleges küldés (task végrehajtása) során, akkor arról a hívó semmilyen módon nem értesül a küldést végző háttérszál futása egyszerűen megszakad.

A **SendAsync** szignatúrája:
```csharp
/// <summary>
/// Teljesen aszinkron üzenetküldés, 
///     amely során az átviteli hibák is elnyelődnek!!! 
///     Cserébe nagyon magas párhuzamos teljesítményre képes! 
///     És a hívás oldali költsége elhanyagolható.
///     Olyan üzenetek elküldésére használandó, 
///     amelyre a küldő oldal semmilyen módon nem vár választ.
///     Beleértve azt, is, hogy nem akar róla tudni, 
///     hogy maga  a küldés sikeres-e.
///     Pl.: logolásra, monitoring kliensek működtetésére használható fel jól.
///     A küldés során keletkező Exception-ök 
///     (Pl. közvetítő infrastruktúra hibái) 
///     mindig elvesznek a hívó szemszögéből. 
/// </summary>
/// <typeparam name="TChannel">Csatorna típusa</typeparam>
/// <typeparam name="TMessage">Üzenet típusa</typeparam>
/// <param name="channelId">Csatorna azonosító</param>
/// <param name="message">üzenet</param>
public static void SendAsync<TChannel, TMessage>(string channelId, TMessage message)
    where TMessage : new()
    where TChannel : BaseChannel, new()
```
Ahol:
* **TChannel type parameter**: A csatornát meghatározó típus paraméter a generikusban. Értékének egy létező, és a kódbázison jelenlévő csatorna implementációs típusnak kell lennie (BaseChannel leszármazott), például **RedisPubSubChannel**.
* **TMessage** type parameter: A ténylegesen küldött üzenet típusát határozza meg.
* **channelId paraméter**: A channelId string paraméter definiálja azt a csatorna azonosítót, melyre az üzenetet küldjük. Mivel a csatorna nevek (id) egy egyezményt reprezentálnak (kinek küldjük) ezeket célszerű nem stringként megadni minden hívási helyen, hanem valamilyen módon kiemelni pl. konfigurációba.
* **message paraméter**: Az elküldött üzenetet reprezentáló TMessage típusú objektum példány, a tuljadonképpeni üzenet.

Használati minta a SendAsync hívásához:
```csharp
    var message = new SampleMessage()
    {
        // property init
        ...
    };
    EventHubCore.SendAsync<RedisPubSubChannel, SampleMessage>(
                        // A csatorna azonosítót egy valós implementációban 
                        //  valmilyen programlogika, vagy konfiguráció adja.
                        // NE HASZNÁLJUNK KÓDBA ÍRT STRINGEKET!!!
                        _myConfigutóration.ChannelId, 
                        message);
    // Soha nem keletkezhet a hívási helyre feljutó Exception a SendAsync hívásában!
```

#### Üzenetek fogadása (feldolgozó oldal)
Ahhoz hogy az EventHub-ra küldött üzeneteket feldolgozzuk regisztrálni kell az EventHub csatorna feldolgozást végző végponti oldalán egy feldolgozó rutint az adott üzenettípushoz ez lesz az adott üzenet feldolgozását végző **kezelő** (**Handler**).

Üzenet feldolgozás csak handler regisztrációt követően lehetséges a handler regisztrációt megelőzően csatornába küldött üzenetek elvesznek, melyekről csak a szinkron hívások (**Call**) esetében lesz információnk (hibajelzésünk) a keret szintjén a küldő oldalán (timeout). (Ez a viselkedés átviteli csatorna függő. A jelenleg létező csatorna implementációkra igaz a fenti megállapítás. Maga a keret alkalmas rá, hogy a későbbiekben olyan üzenettartó csatornaimplementációk szülessenek, amelyek garantált üzenettovábbítást biztosítanak, és a fogadó oldali végpont hiányában tárolják az üzeneteket a feldolgozó végpont megjelenéséig. Természetesen ilyesminek csak visszajelzés nélküli aszinkron üzenet feldolgozó logikák esetén van értelme.)

A handler regisztrációt az **EventHubCore** statikus típus **RegisterHandler** generikus metódusán át lehetséges. Mivel az EventHub keret - a korábban tárgyaltak szerint - builtin támogatást nyújt aszinkron request/response logikák kialakítására, illetve a szinkron hívások is erre épülnek az EventHub-on, ezért a handler regisztrációknál különböző metódus szignatúrák regisztrációjára van szükség, annak függvényében, hogy mi a szándékunk az adott feldolgozással. Eszerint az alábbi fő handler típusokat különböztetjük meg:
1. **Szinkron üzenet-feldolgozó handler**: Ezek a handlerek a küldő oldalról Call hívással elküldött request-et fogadnak, amely a `Request<TRequest, TResponse>` generikus wrapperben érkezik (a keret által biztosítottan, automatikusan végzett csomagolással) és adott típusú választ adnak rájuk mindig a `Response<TResponse>` generikus wrapperrel visszatérve. A válasz továbbítását és az üzenetpárosítást a hívó oldalán teljes egészében az EventHub keret végzi a háttérben, maga a handler implementációban azonban lesz pár követelmény, a helyes működéshez.
2. **Aszinkron üzenetfeldolgozó handler**: Egy tetszőleges típust fogad, a regisztrált metódusnak nincs visszatérési értéke (`void`). Tipikusan válasz nélküli üzenet-feldolgozó logikák esetén használunk ilyeneket, vagy olyan aszinkron request/response  implementációkban, melyben nem használjuk ki az EventHub keret üzenetpárosító szolgáltatásait (a korábban megismert request/response generikus üzenet-wrappereken keresztül), hanem egyedi megoldást alakítunk ki a küldő és a fogadó oldalon összhangban implementálva annak működését.
3. **Aszinkron request (kérés) feldolgozó handler**: Egy `Request<TRequest, Trespons>` kérést fogad, olyan aszinkron request/response hívási logikákban, melyek implementációjában kihasználjuk az EventHub Core built-in request/response párosítási szolgáltatásait. A metódusnak nincs közvetlen visszatérési érétke (void), így ha a feldolgozás maga is küldi az aszinkron választ (Response) a hívó oldalra, akkor erről nem az EventHub keret gondoskodik a háttérben, hanem a feldolgozó rutin implementációjában kerül meghívásra egy `Send<Response<Tresponse>>` hívás.
4. **Aszinkron response (válasz) feldolgozó handler**: Tipikusan akkor használjuk, ha az EventHub keret üzenetpárosító szolgáltatásaira építve aszinkron request/response logikát alakítottunk ki. Ezek a handlerek nem a szolgáltatás fogadó, hanem a kliens oldalán kerülnek regisztrálásra, és fogadják a szolgáltatás aszinkron válasz üzeneteit (response) az elküldött kérésekre (request). Ezeknek a handler metódusoknak sosincs visszatérési értékük (`void`), és paraméterük minden esetben egy `Response<TResponse>` típus.

Fentieknek megfelelően a **RegisterHandler** egy túlterhelt statikus metódus az EventHubCore static class-ban, amelynek a fenti négy változata létezik. Mindegyiket megnézzük részleteiben. 

##### Szinkron üzenet-feldolgozó handlerek
Ezen handlereknek a szignatúrája minden esetben az alábbinak felel meg:
```csharp
public Response<TResponse> HandlerMethodeName(Request<TRequest, TResponse> request)
```
Tehát:
* A metódus visszatérési értéke minden esetben kötelezően a `Response<TResponse>` generikus wrapper osztály az EventHub-ból. Ahol a `TResponse` a wrapper által csomagolt tényleges válasz üzenettípus.
* A metódusnak egyetlen paramétere van, amely kötelezően a `Request<TRequest, TResponse>` wrapper osztály az EventHub-ból. Ahol a `TRequest` a ténylegesen csomagolt kérés típus, a `TResponse` pedig az erre a típusra adandó válasz típusa. A paramétert mindig request-nek nevezzük el!
* A paraméterben használt `Request<TRequest, TResponse>` generikusban definiált `TResponse` típus megegyezik a metódus visszatérési értékében szereplő `Response<TResponse>` generikusban szereplő `TResponse` típussal.
* A `HandlerMethodeName` a tényleges üzenetkezelő metódusunk neve. Használjunk - a **DDD-elv**eknek megfelelően - az üzleti domain által vezettet fogalmakkal operáló beszédes neveket. A metódusok elnevezési konvencióinak megfelelően igékkkel nevezzük el őket, és minden esetben kerüljük a rövidítések használatát!
* Az ilyen kezelő handlereket az alábbi **RegisterHandler** változattal regisztráljuk:
```csharp
/// <summary>
/// Registrál a megadott csatorna fölött egy üzenetkezelőt, amelyik egy adott típusú kérést kezel, 
///     és egy adott típusú (ehhez a kéréshez tartozó) válaszszal tér vissza.
/// Az ilyen üzenetkezelők a Call hívások fogadó oldalán kerülnek meghívásra!
/// </summary>
/// <typeparam name="TChannel">Csatorna típusa</typeparam>
/// <typeparam name="TRequest">Kérés típusa</typeparam>
/// <typeparam name="TResponse">Válasz típusa</typeparam>
/// <param name="channelId">Csatorna azonosító</param>
/// <param name="handler">Üzenet kezelő --> Tesponse/TResponse/ X(Request/TRequest, TResponse/) </param>
public static void RegisterHandler<TChannel, TRequest, TResponse>(
    string channelId, Func<Request<TRequest, TResponse>, Response<TResponse>> handler)
        where TRequest : new()
        where TResponse : new()
        where TChannel : BaseChannel, new()
```
Amennyiben a regisztrálandó metódus szignatúrája nem felel meg a fenti szabályoknak úgy nem lesz regisztrálható ezzel a **RegisterHandler** túlterheléssel. (Compile time error!)

Alapvető használati minta ilyen handler regisztrációjára:
```csharp
try
{
    EventHubCore.RegisterHandler<RedisPubSubChannel, TRequest, TRespons>(
                        // A csatorna azonosítót egy valós implementációban 
                        //  valamilyen programlogika, vagy konfiguráció adja.
                        // NE HASZNÁLJUNK KÓDBA ÍRT STRINGEKET!!!
                        _myConfigutóration.ChannelId, 
                        HandlerMethodeName);
}
catch(Exception e)
{
    // regisztráció során fellépő hiba kezelése
    ...
}
```
A handler implementációjában mindig használjuk ki az EventHub Core `Request<TRequest, TResponse>` és `Response<TResponse>` generikus wrapper típusokon át biztosított üzenetpárosító szolgáltatásait, hogy a keret automatikus szinkron üzenetpárosításának működését garantáljuk. Ennek a lényegi elemei:
* Az érkező `Request<TRequest, TResponse>` paraméterben a konkrét (csomagolt) üzenetet a **RequestContent** property tartalmazza.
* A metódus által visszaadott `Response<TResponse>` objektum pélfdányt mindig a fogadott `Request<TRequest, TResponse>` paraméter **MyResponse** property-jén át szerezzük be! (Bár a megszerzett `Response<TResponse>` objektumpéldányon elérhető a **RequestId** property setter-je, soha ne változtassuk meg szinkron kezelő handler-ekben annak értékét, mert ezzel elrontjuk az EventHub Core üzenetpárosító mechanizmusát, és a hívó oldalak timeout-ra futnak,  a válaszaink sosem kerülnek feldolgozásra.)
* A megszerzett `Response<TResponse>` objektum példány **ResponseContent** property-jében helyezzük el a tényleges üzenetet (`TResponse` típus).
* Amennyiben hiba lépne fel, akkor a hibához tartozóan hozzunk létre egy **Exception**, vagy Exception leszármazott objektumpéldányt és helyezzük el a megszerzett `Response<TResponse>` objektum példány **Exception** property-jében. Ugyanígy járjunk el a handlerben keletkező idegen Exception-ökkel is. A handler kódja legyen hibavédett, és catch ágon mindig tegyük be az elkapott hibákat a request objektum Exception property-jébe, és hiba esetén is térjünk vissza normál módon a függvényből. Ekkor az EventHub Core gondoskodik róla, hogy a hívó oldalán "dóbodjon" ez az Exception. Ellenkező esetben a Core nem továbbítja a választ és a hívó oldal pedig egy timeout-ot kap csak, érdemi hibavisszajelzés nélkül.
* Mindig a fenti mechanizmusra bízzuk a feldolgozási hibák kezelését és ne alakítsunk ki ettől eltérő módszereket az EventHub Core-ra épített szinkron hívási logikákban!
* A handler kódjának szál biztosnak kell lennie, mert az EventHub Core a teljesítmény érdekében aszinkron hívja a beeső üzeneteket feldolgozó handlereket! Minden esetlegesen szükséges erőforrás megosztásról és ezzel kapcsolatos szinkronizációról a handler implementációnak kell gondoskodnia! Tehát csak olyan programozó lesz képes garantáltan helyesen működő EventHub kommunikációt implementálni, aki tisztában van a szálvédett programozás alapvető elveivel és C#/.Net eszközeivel!

Fentiek alapján a szinkron (Call) handler implementációk alapvető szerkezetét adó code snippet így néz ki:
```csharp
public Response<SampleResponse> HandlerMethodeName(Request<SampleRequest, SampleResponse> request)
{
    // A kéréshez tartozó Response objektum beszerzése 
    //  (Response<SampleResponse> generikus wrapper típus):
    var response = request.MyResponse;
    try
    {
        // Kérés objektumpéldány beszerzése (SampleRequest típus) a 
        //  Request<SampleRequest, SampleResponse> generikus wrapper 
        //  RequestContent propertyjéből:
        var requestContent = request.RequestContent;
        // kérés feldolgozása:        
        ...
        // Feldolgozási hibajelzéseknél exception dobás Pl.:
        if (requestContent.Divider < 0)
        {
            throw new Exception("Division by zero!");
        }
        // Válaszban küldendő objektum kreálása (SampleResponse típus):
        var responseContent = new SampleResponse()
        {
            //property init:
            ...
        }
        // válasz elhelyezése a Response<SampleResponse> generikus wrapper
        //  ResponseContent propertyjében:
        response.RespnseContent = responseContent;
    }
    catch(Exception e)
    {
        // Exception elhelyezése a Response<SampleResponse> generikus wrapper 
        //  Exception propertyjében:
        response.Exception = e;
    }
    // Visszatérés a Request<SampleRequest, SampleResponse> MyResponse proprtyjéből beszerzett,
    //  és Exception és ResponseContent property-jeiben megfelelően beállított 
    //  Response<SampleResponse> generikus wrapper példánnyal: 
    return response;
}
```
> Természetesen semmi nem szorít rá, hogy a tényleges üzenet feldolgozás, vagy a válasz kreálása kizárólag a Handler metódus kódjában legyen leprogramozva egy spagetti kóddal. És remélhetőleg nem is jut senkinek eszébe ilyesmi… Mindig alkossunk a S.O.L.I.D-nak és más fejlett OOP szemléleteknek megfelelő kódot, ha már a világ legfejlettebb OOP nyelvén programozunk... 

##### Aszinkron üzenet feldolgozó handlerek regisztrációja
Ezen handlereknek a szignatúrája minden esetben az alábbinak felel meg:
```csharp
public void HandlerMethodeName(TMessage message)
```
Tehát:
* A metódus visszatérési értéke minden esetben kötelezően `void`.
* A metódusnak egyetlen paramétere van, amely tetszőleges típusú (TMessage), de a szerializáció miatt kötelezően van egy paraméter nélküli konstruktora. A paramétert mindig message-nek nevezük el!
* A `HandlerMethodeName` a tényleges üzenetkezelő metódusunk neve. Használjunk a **DDD-elv**eknek megfelelően az üzleti domain által vezettet fogalmakkal operáló beszédes neveket. A metódusok elnevezési konvencióinak megfelelően igékkkel nevezzük el őket, és minden esetben kerüljük a rövidítések használatát!

Az ilyen kezelő handlereket az alábbi **RegisterHandler** változattal regisztráljuk:
```csharp
/// <summary>
/// Regisztrál egy olyan üzenetketzelőt, 
///     amelynek nincs vissaztérési értéke és egy adott típusú beérkező üzenetet kezel
/// Aszinkron, (feltehetően visszajelzés nélküli) üzenetfeldolgozás implementálásához
/// </summary>
/// <typeparam name="TChannel">Csatorna típusa, emely felett az üzenetkezelő működik</typeparam>
/// <typeparam name="TMessage">Üzenet típusa, emylet ez a kezelő dolgoz fel</typeparam>
/// <param name="channelId">Csatorna azonosító: ezen a csatornán érkező üzeneteket dolgozza fel a kezelő</param>
/// <param name="handler">Metódus referencia, ez a metódus végzi a tényleges feldolgozást --> void X(TMessage)</param>
/// <param name="overrideHandler">Segítségével előítrható, hogy regisztrálja felül a végpontban (alkalmazástérben) meglévő kezelőt</param>                
public static void RegisterHandler<TChannel, TMessage>(
    string channelId, Action<TMessage> handler, bool overrideHandler = true)
        where TMessage : new()
        where TChannel : BaseChannel, new()
```
Amennyiben a regisztrálandó metódus szignatúrája nem felel meg a fenti szabályoknak úgy nem lesz regisztrálható a **RegisterHandler** ezen túlterhelésável. (Compile time error!)

Alapvető használati minta ilyen handler regisztrációjára:
```csharp
try
{
    EventHubCore.RegisterHandler<RedisPubSubChannel, SampleMessage>(
                        // A csatorna azonosítót egy valós implementációban 
                        //  valamilyen programlogika, vagy konfiguráció adja.
                        // NE HASZNÁLJUNK KÓDBA ÍRT STRINGEKET!!!
                        _myConfigutóration.ChannelId, 
                        HandlerMethodeName);
}
catch(Exception e)
{
    // regisztráció során fellépő hiba kezelése
    ...
}
```
A handler implementációja meglehetősen nagy szabadságfokkal rendelkezünk:
* Az érkező `TMessage` típusú paraméter (message) az érkező és feldolgozandó üzenet.
* Mivel ez egy aszinkron üzenet feldolgozó, így a hibakezelés részletei nincsenek kihatással az EventHub Core működésére (a továbbítás (kezelő handler aszinkron meghívását) követően a keret nem tud róla mi történik a kezelés során.). Így ebben is teljes az implementáló szabadsága.
* A handler kódjának szál biztosnak kell lennie, mert az EventHub Core aszinkron hívja a beeső üzeneteket feldolgozó handlereket! Minden esetlegesen szükséges erőforrás megosztásról és ezzel kapcsolatos szinkronizációról a handler implementációnak kell gondoskodnia!

Fentiek alapján az aszinkron (`Send<TChannel, TMessage>`) handler implementációk alapvető szerkezetért adó code snippet így néz ki:
```csharp
public void HandlerMethodeName(SampleMessage message)
{
    // Kérés feldolgozása:        
    ...
    // Az ilyen handlerek vagy visszajelzés nélküli üzenet feldolgozó architektúra részei. 
    //  Ez esetben a kód csak az üzenet feldolgozásra szorítkozik.
    // Vagy egy olyan aszinkron request/response architektúra részei, 
    //  mely egy teljesen saját megvalósítással párosítják az üzeneteket. 
    //  Ebben az esetben természetesen tartani kell az ezzel kapcsolatos szabályokat, 
    //  és a handlernek (vagy a feldolgozó infrastruktúra egyéb részének) 
    //  egy ponton gondoskodnia kell a megfelelő válasz visszaküldéséről, 
    //  amelyhez az EventHub Send metódusát használja fel:
    EventHubCore.Send<RedisPuBSubChannel, SampleResponseMessage>(
                        // A csatorna azonosítót egy valós implementációban 
                        //  valamilyen programlogika, vagy konfiguráció adja.
                        // NE HASZNÁLJUNK KÓDBA ÍRT STRINGEKET!!!
                        _myConfigutóration.ChannelId, 
                        sampleResponseMessageInstance
    );
}
```
> Természetesen semmi nem szorít rá, hogy a tényleges üzenet feldolgozás, vagy a válasz kreálása kizárólag a Handler metódus kódjában legyen leprogramozva egy spagetti kóddal. És remélhetőleg nem is jut senkinek eszébe ilyesmi… Mindig alkossunk a S.O.L.I.D-nak és más fejlett OOP szemléleteknek megfelelő kódot, ha már a világ legfejlettebb OOP nyelvén programozunk...  

##### Aszinkron request (kérés) feldolgozó handlerek regisztrációja
Ezen handlereknek a szignatúrája minden esetben az alábbinak felel meg:
```csharp
public void HandlerMethodeName(Request<TRequest, TResponse> request)
```
Tehát:
* A metódus nem rendelkezik visszatérési értékkel (`void`).
* A metódusnak egyetlen paramétere van, amely kötelezően a `Request<TRequest, TResponse>` wrapper osztály az EventHub-ból. Ahol a `TRequest` a ténylegesen csomagolt kérés típus, a `TResponse` pedig az erre a típusra adandó válasz típusa. A paramétert mindig request-nek nevezzük el!
* A `HandlerMethodeName` a tényleges üzenetkezelő metódusunk neve. Használjunk - a **DDD-elv**eknek megfelelően - az üzleti domain által vezettet fogalmakkal operáló beszédes neveket. A metódusok elnevezési konvencióinak megfelelően igékkkel nevezzük el őket, és minden esetben kerüljük a rövidítések használatát!
* Az ilyen kezelő handlereket az alábbi **RegisterHandler** változattal regisztráljuk:
```csharp
/// <summary>
/// Registrál a megadott csatorna fölött egy Requesteket fogadó üzenetkezelőt, 
/// melynek nincs visszatérési értéke 
/// Aszinkron Request/Response implementációkhoz a kérések fogadó oldalán
/// </summary>
/// <typeparam name="TChannel">Csatorna típus implementáció</typeparam>
/// <typeparam name="TRequest">Kérés típusa</typeparam>
/// <typeparam name="TResponse">Válasz típusa</typeparam>
/// <param name="channelId">Csatorna azonosító</param>
/// <param name="handler">üzenetkezelő --> void X(TRequest)</param>
/// <param name="overrideHandler">Segítségével előítrható, hogy regisztrálja felül a végpontban (alkalmazástérben) meglévő kezelőt</param>
public static void RegisterHandler<TChannel, TRequest, TResponse>(
    string channelId, Action<Request<TRequest, TResponse>> handler, 
    bool overrideHandler = true)
        where TRequest : new()
        where TResponse : new()
        where TChannel : BaseChannel, new()
```
Amennyiben a regisztrálandó metódus szignatúrája nem felel meg a fenti szabályoknak úgy nem lesz regisztrálható a **RegisterHandler** ezen túlterhelésével. (Compile time error!)

Alapvető használati minta ilyen handler regisztrációjára:
```csharp
try
{
    EventHubCore.RegisterHandler<RedisPubSubChannel, TRequest, TResponse>(
                        // A csatorna azonosítót egy valós implementációban 
                        //  valamilyen programlogika, vagy konfiguráció adja.
                        // NE HASZNÁLJUNK KÓDBA ÍRT STRINGEKET!!!
                        _myConfigutóration.ChannelId, 
                        HandlerMethodeName);
}
catch(Exception e)
{
    // regisztráció során fellépő hiba kezelése
    ...
}
```
Az ilyen handlerek implementációjában mindig használjuk ki az EventHub Core `Request<TRequest, TResponse>` és `Response<TResponse>` generikus wrapper típusokon át biztosított üzenetpárosító és hibakezelő szolgáltatásait, hogy ennek elvei és működési módja lehessen az egyezmény a hívó és  a feldolgozó oldal kommunikációjára vonatkozóan. (Egyébként nincs értelme ezeket a wrapper osztályokat használnunk!) Ennek a lényegi elemei:
* Az érkező `Request<TRequest, TResponse>` paraméterben a konkrét (csomagolt) üzenetet a **RequestContent** property tartalmazza.
* A metódus által visszaadott `Response<TResponse>` objektum példányt mindig a fogadott `Request<TRequest, TResponse>` paraméter **MyResponse** property-jén át szerezzük be! (Bár a megszerzett `Response<TResponse>` objektumpéldányon elérhető a **RequestId** property setter-je, soha ne változtassuk meg szinkron kezelő handler-ekben annak értékét.)
* A megszerzett `Response<TResponse>` objektum példány **ResponseContent** property-jében helyezzük el a tényleges üzenetet (`TResponse` típus).
* Amennyiben hiba lépne fel, akkor a hibához tartozóan hozzunk létre egy **Exception**, vagy Exception leszármazott objektumpéldányt és helyezzük el a megszerzett `Response<TResponse>` objektum példány **Exception** property-jében. Ugyanígy járjunk el a handler-ben keletkező idegen Exception-ökkel is. A handler kódja legyen hibavédett, és catch ágon mindig tegyük be az elkapott hibákat a request objektum Exception property-jébe, és hiba esetén is küldjük vissza a hívónak a választ.
* A handler kódjának szál biztosnak kell lennie, mert az EventHub Core a teljesítmény érdekében aszinkron hívja a beeső üzeneteket feldolgozó handlereket! Minden esetlegesen szükséges erőforrás megosztásról és ezzel kapcsolatos szinkronizációról a handler implementációnak kell gondoskodnia!
* A válasz (Response) visszaküldéséről mindig a hendler (vagy a feldolgozó infrastruktúra egyéb részének) kódjában kell gondoskodnunk az EventHub keret `Send<TChannel, TResponse>(string channelId, Response<TResponse> response)` metódusának hívásával.

Fentiek alapján az aszinkron kérés (`Request<TRequest, TResponse>`) feldolgozó handlerek implementációjának alapvető szerkezetért adó code snippet így néz ki:
```csharp
public void HandlerMethodeName(Request<SampleRequest, SampleResponse> request)
{
    // A kéréshez tartozó Response objektum beszerzése 
    //  (Response<SampleResponse> generikus wrapper típus):
    var response = request.MyResponse;
    try
    {
        // Kérés objektumpéldány beszerzése (SampleRequest típus) a 
        //  Request<SampleRequest, SampleResponse> generikus wrapper 
        //  RequestContent propertyjéből:
        var requestContent = request.RequestContent;
        // kérés feldolgozása:        
        ...
        // Feldolgozási hibajelzéseknél Exception dobás Pl.:
        if (requestContent.Divider < 0)
        {
            throw new Exception("Division by zero!");
        }
        // Válaszban küldendő objektum kreálása (SampleResponse típus):
        var responseContent = new SampleResponse()
        {
            //property init:
            ...
        }
        // válasz elhelyezése a Response<SampleResponse> generikus wrapper
        //  ResponseContent propertyjében:
        response.RespnseContent = responseContent;
    }
    catch(Exception e)
    {
        // Exception elhelyezése a Response<SampleResponse> generikus wrapper 
        //  Exception propertyjében:
        response.Exception = e;
    }
    // A válasz visszaküldése a Request<SampleRequest, SampleResponse> 
    //  MyResponse proprtyjéből beszerzett,
    //  és Exception és ResponseContent property-jeiben megfelelően beállított 
    //  Response<SampleResponse> generikus wrapper példány aszinkron elküldésével: 
    try
    {
        EventHubCore.Send<RedisPubSubChannel, SampleResponse>(
                        // A csatorna azonosítót egy valós implementációban 
                        //  valamilyen programlogika, vagy konfiguráció adja.
                        // NE HASZNÁLJUNK KÓDBA ÍRT STRINGEKET!!!
                        _myConfigutóration.ChannelId, 
                        response
        );
    }
    catch(Exception e)
    {
        // Esetleges válaszküldési hibák kezelése
    }
}
```
Aszinkron logikák valós implementációja igen bonyolult lehet. Fenti snippet nem tartalmaz erre vonatkozó elemeket!
> Természetesen semmi nem szorít rá, hogy a tényleges üzenet feldolgozás, vagy a válasz kreálása kizárólag a Handler metódus kódjában legyen leprogramozva egy spagetti kóddal. És remélhetőleg nem is jut senkinek eszébe ilyesmi… Mindig alkossunk a S.O.L.I.D-nak és más fejlett OOP szemléleteknek megfelelő kódot, ha már a világ legfejlettebb OOP nyelvén programozunk...  

##### Aszinkron response (válasz) feldolgozó handlerek regisztrációja
Ezen handlereknek a szignatúrája minden esetben az alábbinak felel meg:
```csharp
public void HandlerMethodeName(Response<TResponse> response)
```
Tehát:
* A metódus nem rendelkezik visszatérési értékkel (`void`).
* A metódusnak egyetlen paramétere van, amely kötelezően a `Response<TResponse>` wrapper osztály az EventHub-ból. A paramétert mindig response-nak nevezzük el!
* A `HandlerMethodeName` a tényleges üzenetkezelő metódusunk neve. Használjunk - a **DDD-elv**eknek megfelelően - az üzleti domain által vezettet fogalmakkal operáló beszédes neveket. A metódusok elnevezési konvencióinak megfelelően igékkkel nevezzük el őket, és minden esetben kerüljük a rövidítések használatát!
* Az ilyen kezelő handlereket az alábbi **RegisterHandler** változattal regisztráljuk:
```csharp
/// <summary>
/// Regisztrál egy kifejezetten response üzeneteket kezelő handlert 
///     (nincs visszatérési értéke)
/// Aszinkron Request/Response implementációkhoz a kéréseket (request) küldő oldalán, 
///     a válaszok (response) fogadásának céljából
/// </summary>
/// <typeparam name="TChannel">Csatorna típus</typeparam>
/// <typeparam name="TResponse">Válasz típusa</typeparam>
/// <param name="channelId">Csatorna azonosító</param>
/// <param name="handler">Üzenet kezelő --> void X(Response/TResponse/) </param>
/// <param name="overrideHandler">Segítségével előítrható, hogy regisztrálja felül a végpontban (alkalmazástérben) meglévő kezelőt</param>
public static void RegisterHandler<TChannel, TResponse>(
    string channelId, Action<Response<TResponse>> handler, 
    bool overrideHandler = true)
        where TResponse : new()
        where TChannel : BaseChannel, new()
```
Amennyiben a regisztrálandó metódus szignatúrája nem felel meg a fenti szignatúrára vonatkozó szabályoknak úgy nem lesz regisztrálható a **RegisterHandler** ezen túlterhelésével. (Compile time error!)

Alapvető használati minta ilyen handler regisztrációjára:
```csharp
try
{
    EventHubCore.RegisterHandler<RedisPubSubChannel, TResponse>(
                        // A csatorna azonosítót egy valós implementációban 
                        //  valamilyen programlogika, vagy konfiguráció adja.
                        // NE HASZNÁLJUNK KÓDBA ÍRT STRINGEKET!!!
                        _myConfigutóration.ChannelId, 
                        HandlerMethodeName);
}
catch(Exception e)
{
    // regisztráció során fellépő hiba kezelése
    ...
}
```
Az ilyen handlerek implementációjában mindig használjuk ki az EventHub Core `Request<TRequest, TResponse>` és `Response<TResponse>` generikus wrapper típusokon át biztosított üzenetpárosító és hibakezelő szolgáltatásait, hogy ennek elvei és működési módja lehessen az egyezmény a hívó és  a feldolgozó oldal kommunikációjára vonatkozóan. (Egyébként nincs értelme ezeket a wrapper osztályokat használnunk és ezek fogadására felkészített kezelőket regisztrálnunk!) Ennek a lényegi elemei:
* Az érkező `Response<TResponse>` paraméterben a konkrét (csomagolt) üzenetet a **ResponseContent** property tartalmazza.
* Az érkező `Response<TResponse>` paraméterben a **RequestId** tartalmazza annak a kérésnek (request) az azonosítóját, amelyhez a fogadott response tartozik.
* Az érkező `Response<TResponse>` paraméterben az **Ok** property true értéket ad, ha a válasz egy hiba nélküli valódi Response objektum. Az **Ok** property false értékű, ha a válasz egy hiba jelzés.
* Ekkor az **Exception** property-je tartalmaz egy Exception-t, vagy egy Exception leszármazottat. 
* A handler kódjának szál biztosnak kell lennie, mert az EventHub Core a teljesítmény érdekében aszinkron hívja a beeső üzeneteket feldolgozó handlereket! Minden esetlegesen szükséges erőforrás megosztásról és ezzel kapcsolatos szinkronizációról a handler implementációnak kell gondoskodnia!
* A válasz (Response) feldolgozó hendler-ek semmilyen választ nem küldenek vissza a másik oldalra!

Fentiek alapján az aszinkron válasz (`Response<TResponse>`) feldolgozó handler implementációjának alapvető szerkezetét adó code snippet így néz ki:
```csharp
public void HandlerMethodeName(Response<SampleResponse> response)
{
    try
    {
        // ehhez a kéréshez tartozik a feldolgozott üzenet:
        var requestId = response.RequestId;
        // Hibaellenőrzés
        if (response.Ok)
        {
            // Nincs hiba:
            // Válasz feldolgozása:
            // Tényleges válasz objektum kinyerése a wrapperből:
            var responseContent = response.ResponseContent;
            // Tényleges feldolgozás, és request/response párosítás, 
            //  válasz az eredeti kérőhöz juttatása, 
            //  a kialakított aszinkron működéi logikának megfelelően 
            //  stb.:
            ...
        }
        else
        {
            // A fogadott válasz (response) tulajdonképpen egy hibajelzés
            // Az Excpetion property tartalmazza a tényleges hibaüzenetet
            var errorMessage = response.Exception.Message;
            // Hibainformációk kinyerése, és request/response párosítás, 
            //  hibainformációnak az eredeti  kérőhöz juttatása, 
            //  a kialakított aszinkron működéi logikának megfelelően 
            //  stb.:
            ...
        }
    }
    catch(Exception e)
    {
        // Válasz feldolgozási hibák kezelésem, ha szükséges
        ...
    }
}
```
Mivel az EbventHub üzenetközvetítése rendkívül gyors, ezért aszinkron request/response logikák implementálásában mindig előbb a response üzenetet fogadó handlert regisztráljuk a request-et küldő oldalon, és csak ezután küldjük el az EventHubon a kerést. Fenti Code Snippet nem foglalkozik ezzel a kérdéskörrel, de az aszinkron logikák implementációja ezen túl is igen bonyolult lehet. Fenti snippet nem tartalmaz erre vonatkozó elemeket!
> Természetesen semmi nem szorít rá, hogy a tényleges üzenet feldolgozás, vagy a válasz kreálása kizárólag a Handler metódus kódjában legyen leprogramozva egy spagetti kóddal. És remélhetőleg nem is jut senkinek eszébe ilyesmi… Mindig alkossunk a S.O.L.I.D-nak és más fejlett OOP szemléleteknek megfelelő kódot, ha már a világ legfejlettebb OOP nyelvén programozunk... 

#### Típus-vezérelt működés
Most, hogy már egyaránt ismerjük az EventHub üzenetek küldésének módját, és az üzeneteket fogadó/feldolgozó handlerek regisztrációjának módját, kitérhetünk rá, hogy mit is jelent tulajdonképpen, hogy az EventHub Core típusvezérelt üzenetközvetítést valósít meg.

Egy adott csatornán közlekedő üzenetet minden egyes az adott csatornára bármilyen Handler regisztrációval feliratkozott EventHub végpont (EndPoint) megkap. Ha az adott végponton talál olyan üzenetkezelőt, amelyik regisztrálva van az érkezett üzenet típushoz, akkor a kapott üzenettel meghívja ezt a kezelőt (handler). De mit is jelent az, hogy "adott üzenet típus"? Az alábbi szabályok érvényesülnek az üzenet/handler párosításban: 
1. Ha a fogadott üzenet egy `Request<Trequest, TResponse>`, melyhez `Response<TResponse>` visszatérési érték tartozik, akkor az EventHub Core egy olyan handler regisztrációt keres a végpontban, amely megfelel a `public Response<TResponse> HandlerMethodeName(Request<TRequest, TResponse> request)` szignatúrának. Ha talál ilyet, akkor ezt hívja meg szinkron módon és megvárja a hívás visszatérését, a visszakapott `Response<TResponse>` objektumpéldányt pedig automatikusan továbbítja a kérést küldő oldalra.
2. Ha a fogadott üzenet egy `Request<Trequest, TResponse>`, melyhez `void` visszatérési érték tartozik, akkor az EventHub Core egy olyan handler regisztrációt keres a végpontban, amely megfelel a `public void HandlerMethodeName(Request<TRequest, TResponse> request)` szignatúrának. Ha talál ilyet, akkor ezt hívja meg aszinkron módon. 
3. Ha a fogadott üzenet egy `Respons<TResponse>`, melyhez `void` visszatérési érték tartozik, akkor az EventHub Core egy olyan handler regisztrációt keres a végpontban, amely megfelel a `public void HandlerMethodeName(Response<TResponse> response)` szignatúrának. Ha talál ilyet, akkor ezt hívja meg aszinkron módon.
4. Ha a fogadott üzenet egy tetszőleges (paraméter nélküli konstruktorral rendelkező) típus, de nem `Request<TRequest, TResopponse>` és nem is `Respons<TResponse>`, melyhez `void` visszatérési érték tartozik, akkor az EventHub Core egy olyan handler regisztrációt keres a végpontban, amely megfelel a `public void HandlerMethodeName(TMessage message)` szignatúrának. Ha talál ilyet, akkor ezt hívja meg aszinkron módon.

Mint látható, az üzenet típusához hozzátartozik az az információ is, hogy ad-e, és ebben az esetben milyen konkrét visszatérési típussal (result) rendelkező választ ad a kezelő a kérésre. Amikor az EventHub Core kapcsán üzenet típusról beszélünk, akkor abba minden esetben beleértjük ezt az információt is!

Ha a keret nem talál semelyik fenti egyezésnek megfelelő handler regisztrácót, akkor nem foglalkozik a fogadott üzenettel. Ez nem hiba, egyszerűen az EventHub Core lazán csatolt, hálós üzenet infrastruktúrájához tartozó üzenet-broadcastingra épített feldolgozás sajátossága. Mivel a végpontok közvetlenül nem tudnak arról, hogy a teljes üzenet-közvetítő infrastruktúrán milyen végpontok, milyen számban, és milyen handler-regisztrációkkal vesznek részt, ezért az sem hiba, ha egy adott üzenetet senki nem fogad. Ezek az üzenetek elvesznek az EventHub-on, amelyről szinkron hívásoknál egy timeout formájában értesül a hívó oldal. (Az EventHub Core fölött elképzelhető olyan átviteli csatorna implementáció, amely egy olyan átviteli csatornára épül, amely a fel nem dolgozott üzenetek eltárolására is szolgáltatást nyújt. (Pl. MSMQ, Klafka) Erre építve elképzelhető üzenettartás megvalósítása és késleltetett feldolgozás is. De jelenleg nem áll rendelkezésre ilyen csatorna implementáció.) Aszinkron hívások esetén azonban a keret szintjén semmilyen visszajelző mechanizmus nem létezik, legfeljebb az aszinkron logika EventHub Core feletti implementációja detektálhatja a válasz üzenet elmaradásának tényét.

Az EventHub-on mindig alakítsunk ki, valódi üzenettípusokat, és használjuk ki a típusosságot és a típusvezéreltséget! Ennek megfelelően saját típusokat használjunk, és ne .Net alaptípusokra regisztráljunk üzenetkezelőket!

#### Handler konkurencia, üzenet túlterhelés, körkörös végtelen hívások
Az előző fejelzetben leírtakból következően az EventHubon létezik a **Handler konkurencia** fogalma. Ilyen akkor áll elő, amikor több végpont is feliratkozik ugyanannak az üzenetnek a fogadására. Az EvenetHub ilyen esetben minden végpontban meghívja regisztrált feldolgozó handlert. A sorrendiség teljesen véletlenszerű, az infrastruktúra elhelyezkedésétől, az átviteli csatornák sebességétől függ, semmilyen sorrendiség, vagy priorítás nem garantálható, a kezelők a különböző végpontokban a keret szintjén nem tudnak egymásról. Szinkron hívás esetén sem létezik semmilyen garancia arra, hogy melyik feldolgozás válasza ér előbb vissza a küldő oldalra.

Nem lehetséges ugyanakkor egy adott végpontban "túlterhelni" egy üzenetet abban az értelemben, hogy ugyanarra az üzenet típusra több kezelőt is bejegyzünk. Ennek az oka, hogy a **RegisterHandler** hívások mindegyik verziója úgy működik, hogy ha az adott típusú kezelőre már van egy regisztráció, akkor az újabb regisztráció felülírja azt, és a korábbi kezelő regisztráció nem lesz érvényben többé. Függetlenül attól, hogy a regisztráció ugyanazt a metódust regisztrálja, vagy egy másikat. Emiatt egy EventHub végponton egy adott típusra maximum egy kezelő létezhet.

Legyen A metódusunk, melynek egyetlen ismert viselkedéseleme, hogy minden esetben meghívja B metódust, melynek egyetlen ismert viselkedés eleme, hogy minden esetben meghívja A metódust. A modern programozási környezetet az ilyen hívási logikák kialakítását hibának tekintik és általában beépített védelemmel rendelkeznek a kialakult végtelen hívási flow legalább futás idejű megszüntetésére (StackOverflow Excpetion). Az EventHub nem tartalmaz védelmet a körkörös végtelen hívási hurkok kialakítására nézve. Legyünk tehát körültekintőek, mikor aszinkron logikákat építünk!

#### Hibakezelés
Amennyiben az EventHub alatti üzenetközvetítő csatorna nem működik, akkor erről a küldés oldal értesülhet:
* Call (szinkron küldés) esetén minden esetben egy Exception megy fel a Call hívásának helyére. Az Exception tényleges típusa nagymértékben függ a hívott csatorna implementációjától és a hiba jelegétől.
* Send (aszinkron küldés) esetén minden esetben egy Exception megy fel a Send hívásának helyére. Az Exception tényleges típusa nagymértékben függ a hívott csatorna implementációjától és a hiba jellegétől.
* SendAsynch (ténylegesen aszinkron módon az EventHub csatornára dobott üzenet) hívással való küldés esetén a hívó hely semmilyen módon nem értesül az átviteli csatorna hibáiról sem!

Amennyiben az infrastruktúra hiba megszűnik, azt ezt követő következő üzenetküldés már sikeres lesz. 

Fogadás oldalon, az EventHub végpont nem értesül közvetlen az átviteli infrastruktúra hibáról (hacsak nem küld maga is üzenetet). Természetesen a hiba eredményeként nem kap üzeneteket. Az infrastruktúra hiba megszűnte után a végpont újra megkapja az EventHub-on éppen érkező üzeneteket.

A küldő infrastruktúra végpontokban való helyreállításáért az EventHub Core-t felhasználó alkalmazás kódjának semmit nem kell tennie azt az EventHub Core alatt működő csatornaimplementáció a háttérben önműködően és automatikusan garantálja azt!

Amikor egy végpont handler feliratkozást végez, akkor legfeljebb abban az esetben értesül egy esetleges fennálló átviteli infrastruktúra hibáról, ha maga az átviteli csatornát jelentő infrastruktúra is ekkor jön létre. Egy már létrejött csatornára a további handler feliratkozások megtörténnek éppen fennálló infrastruktúra hiba esetén is. Ezek a handlerek az infrastruktúra hiba megszűnte után ténylegesen működni kezdenek.

**A hibakezelés részletei nagyban függenek a használt átviteli csatorna implementációjától. Ezért az ezzel kapcsolatos részletek az adott csatorna dokumentációjában találhatóak.**

#### Erőforrás kezelés
Az EventHub megoldás infrastruktúra inicializáció mentes kialakítása azt jelenti, hogy a Core maga gondoskodik róla, hogy az átviteli infrastruktúra rendelkezésre álljon. A felhasználás elől elrejtve fenntartja és optimalizálja annak kezelését.
Ez, és a handler regisztrációk felülíró működési elve biztosítja, hogy az EvenetHub erőforrásigényének mindig van egy végtelen alatti konkrét korlátja. (Elméleti végtelenbe skálázódási képesség…) A static megvalósítás, és az erőforrás megosztás miatt alapvetően nincs szükség az EventHub erőforrásokkal való gazdálkodásra a felhasználás oldalán.

Elképzelhetők azonban olyan helyzetek, amikor szükségünk lehet arra hogy adott végponton megszüntessük egy adott üzenet kezelését, vagy egy adott üzenetcsatorna figyelését, a végpont alkalmazásterének megszüntetésétől elválasztott módon. Ezekre az esetekre rendelkezésre áll a kezelők kiregisztrálására és komplett üzenet csatornák eldobására is egy API.
##### Handlerek eldobása (kiregisztrálása)
A RegisterHandler 4 változatának megfelelő párként, rendelkezésre áll az **EventHubCore.DropHandler** generikus túlterhelt metódus 4 változata. Ezek segítségével bármelyik üzenettípusra vonatkozó kezelő regisztrációt megszüntethetjük. Az adott üzenettípust a végpont többé nem fogja feldolgozni az adott csatornán.

A **DropHandler** 4 változata:
1. Ha egy `public Response<TResponse> HandlerMethodeName(Request<TRequest, TResponse> request)` szignatúrának megfelelő üzenetkezelőt kívánunk megszüntetni, akkor a DropHandler alábbi túlterhelését használjuk: 
```csharp
/// <summary>
/// Kiregisztrálja (eldobja) a megadott csatornán az üzenettípushoz regisztrált handlert
/// </summary>
/// <typeparam name="TChannel">Csatorna típusa</typeparam>
/// <typeparam name="TRequest">Kérés típusa</typeparam>
/// <typeparam name="TResponse">Válasz típusa</typeparam>
/// <param name="channelId">csatorna azonosító</param>
/// <param name="handler">üzenet kezelő metódus, amit kiregisztrál</param>
public static void DropHandler<TChannel, TRequest, TResponse>(
            string channelId, Func<Request<TRequest, TResponse>, Response<TResponse>> handler)
    where TRequest : new()
    where TResponse : new()
    where TChannel : BaseChannel, new()
```
2. Ha egy `public void HandlerMethodeName(Request<TRequest, TResponse> request)` szignatúrának megfelelő üzenetkezelőt kívánunk megszüntetni, akkor a DropHandler alábbi túlterhelését használjuk:
```csharp
/// <summary>
/// Kiregisztrálja (eldobja) a megadott csatornán az üzenettípushoz regisztrált handlert
/// </summary>
/// <typeparam name="TChannel">Csatorna típusa</typeparam>
/// <typeparam name="TMessage">Üzenet típusa</typeparam>
/// <param name="channelId">csatorna azonosító</param>
/// <param name="handler">üzenet kezelő metodus, amit kiregisztrál</param>
public static void DropHandler<TChannel, TRequest, TResponse>(
                    string channelId, Action<Request<TRequest, TResponse>> handler)
    where TRequest : new()
    where TResponse : new()
    where TChannel : BaseChannel, new()
```
3. Ha egy `public void HandlerMethodeName(Response<TResponse> response)` szignatúrának megfelelő üzenetkezelőt kívánunk megszüntetni, akkor a DropHandler alábbi túlterhelését használjuk:
```csharp
/// <summary>
/// Kiregisztrálja (eldobja) a megadott csatornán az üzenettípushoz regisztrált handlert
/// </summary>
/// <typeparam name="TChannel">Csatorna típusa</typeparam>
/// <typeparam name="TResponse">Üzenet típusa</typeparam>
/// <param name="channelId">csatorna azonosító</param>
/// <param name="handler">üzenet kezelő metodus, amit kiregisztrál</param>
public static void DropHandler<TChannel, TResponse>(
                    string channelId, Action<Response<TResponse>> handler)
    where TResponse : new()
    where TChannel : BaseChannel, new()
```
4. Ha egy `public void HandlerMethodeName(TMessage message)` szignatúrának megfelelő üzenetkezelőt kívánunk megszüntetni, akkor a DropHandler alábbi túlterhelését használjuk:
```csharp
/// <summary>
/// Kiregisztrálja (eldobja) a megadott csatornán az üzenettípushoz regisztrált handlert
/// </summary>
/// <typeparam name="TChannel">Csatorna típusa</typeparam>
/// <typeparam name="TMessage">Üzenet típusa</typeparam>
/// <param name="channelId">csatorna azonosító (erről a csatornáról távolítja el a kezelőt)</param>
/// <param name="handler">az eltávolítandő kezelő</param>
public static void DropHandler<TChannel, TMessage>(string channelId, Action<TMessage> handler)
    where TMessage : new()
    where TChannel : BaseChannel, new()
```
Az eldobott kezelő regisztrációnak megfelelő üzenet típusokat nem kezeli többé az adott EventHub végpont, amíg újabb kezelőt nem regisztrálunk hozzájuk.
##### Csatornák eldobása
Ha egy teljes üzenetcsatornát szüntetnénk meg egy EventHub végpontban, akkor erre az alábbi EventHubCore statikus metódussal van lehetőségünk:
```csharp
/// <summary>
/// Az összes handlert kiregisztrálja + megszünteti a csatornát
/// </summary>
/// <param name="channelId">Csatorna, amit megszüntetünk.</param>
public static void DropChannel<TChannel>(string channelId)
    where TChannel : BaseChannel, new()
```
Ahol:
* **TChannel típus paraméter**: a megszüntetendő csatorna típusa.
* **channelId string paraméter**: a megszüntetendő csatorna azonosítója.

A hívás eldobja az összes adott csatornán érvényes kezelő regisztrációját az adott EvenThub végpontban (vagyis alkalmazástérben). Így a fogadó oldalt tekintve semmilyen üzenetet nem dolgoz fel többé a végpont, amelyik ezen a csatornán érkezik. Amíg újra nem regisztrálunk a csatornára legalább egy üzenetkezelő handlert. 

A küldést illetően egy csatorna eldobást követően bármikor újra üzenetet küldhetünk a végponton az adott csatornára, de a csatorna ismételt regisztrációs költsége is terhelni fogja ezt az eldobást követő első küldést. (Mintha legelőször használnánk az adott végpontban a csatornát.)

### Tippek/trükkök, Best practice
* Törekedjünk rá, hogy az EventHub kommunikációhoz mindig erre a célra dedikált üzenet típusokat használjunk.
* Ezeket a típusokat egy valós implementációban két, vagy több független oldalon is ismernünk kell. Azért, hogy ezek terjesztése jól megoldott legyen, szervezzük őket önálló .Net Assembly-be (class library), melyek lehetőleg nem tartalmaznak mást. Ezen dll-ek elnevezésében használjuk az EventHubContract utótagot. (Pl.: PrintingService.EventHubContract.dll)
* Az EventHub Core Newtonsoft JSON-t használ az üzenetek szérializációjára, és deszérializációjára. Ezért a **[JsonIgnore]** és a további NewtonSoft JSON szérializációt befolyásoló attribútumok használhatóak az üzenet osztályokon.
* Az EventHub Core típusfelismerése támogatja a .Net egymásba ágyazott osztályait (Nested class), ezért a különféle üzeneteket organizálhatjuk egy class hierarchiában definiálva őket, vagy csak egyszerűen egy Contract class alá szervezve, elősegítve ezzel a felhasználási helyen való használatukat (kódkiegészítés, stb.).
* Egy adott szolgáltatás képességeit reprezentáló üzenetosztályokat (Contract) mindig szervezzük egy önálló .Net Assembly (Class library) alá. Ennek a verziózására használjuk a `AssemblyInformationalVersion` attribútumot, és mindig a <a href="https://semver.org/" target="_blank">Semantic Versioning</a> elveit használjuk a verziózásra a Contractra önmagában, mint API-ra tekintve!

### Működési paraméterek
Az EventHub Core egyetlen paraméter konfiguráció szintű beállítására van lehetőség:
#### ThrowEventHubCoreExceptions paraméter
Azt mondja meg, hogy az EventHub keret dobjon-e kivételeket nem fatális hibák esetén is a hívó oldalra, vagy nyelje le ezeket az információkat kivételek dobása nélkül. Fatális hibának nevezünk minden az üzenet tényleges elküldését megakadályozó hibát. Nem fatális hibák azok, melyek az EventHub végpont működőképességére nincsenek kihatással. Ilyen pl., ha olyan handlert próbálunk eldobni, amelyikre nincs is regisztráció. A működést megakadályozó alapvető infrastruktúra hibákat mindig továbbdobja a keret egy Exception formájában a hívó felé.

A beállítást kétféleképpen adhatjuk meg:
#### AppSettings kulcsként
Ezt a paramétert megadhatjuk a hostoló alkalmazás app.configjában egy `Vrh.EventHub.Core:ThrowEventHubCoreExceptions` nevű application settings kulcs alatt, amelynek az értéke boolean típusú (true/false). Amennyiben definiáljuk e helyen, akkor nem veszi figyelembe, hogy mit állítunk be az egyedi XML konfigurációban.
Az alábbi példa szerint:
```xml
<appSettings>      
  <add key="Vrh.EventHub.Core:ThrowEventHubCoreExceptions" value="true" />
</appSettings>
```
#### XML fájlban megadott beállításként
Ha a beállítást nem definiáljuk a hostoló alkalmazás alkalmazás kulcsai közt annak application config fájljában, akkor egy VRH konfigurációs XML-ben elhelyezve is megadhatjuk azt. Az alábbi példa mutatja a konfigurációt adó XML szerkezetét:
```xml
<?xml version="1.0" encoding="utf-8" ?> 
<Vrh.EventHub.Core>
  <ThrowEventHubCoreExceptions>true</ThrowEventHubCoreExceptions>
</Vrh.EventHub.Core>
```
A fenti tartalommal az XML fájlt a hostoló alkalmazás könyvtárában kell elhelyezni a `Vrh.EventHub.Core.Config.xml` néven.

Ha a fenti alapértelmezéstől el kívánunk térni, akkor beállítást tartalmazó XML fájlt és a **ThrowEventHubCoreExceptions** XML tag abban elfoglalt helyét definiálhatjuk a szokásos leírással a hostoló alkalmazás app configjában elhelyezett application settings kulccsal:
```xml
<appSettings>      
  <add key="Vrh.EventHub.Core:Config" value="@EventHub.Core.Config.xml/Vrh.EventHub.Core" />
</appSettings>
```
Ahol:
* **@** prefix segítségével írjuk alő, hogy a megadott fájlnév relatív útvonal definíció a hostoló alkalmazás munkakönyvtárához képest. Ha abszolút útvonalakkal definiálunk, hagyjuk el a @ jelet!
* A fájl útvonalakban a **\\** karaktert használjuk elválasztóként.
* A fájl definicó után egy **/** után lehetőségünk van definiálni azt az XML-ben lévő útvonalat, ahol a konfiguráció található. Az egyes XML tageket a **/** jel választja el egymástól.

### Használati példák
A használati példához nézzük az alábbi esetet: van egy AddMachine-nak elnevezett kis programunk, ami azt tudja, hogy egész számokat kap egy listában, és azokat összeadja, majd visszatér az eredménnyel. Valami ilyesmi:
```csharp
private int AddAll(List<int> forAdding)
{
    int result = 0;
    foreach (var number in forAdding)
    {
        result += number;
    }
    return result;
}
```
#### Szinkron szolgáltatás képesség publikálása, és használata
Tegyük fel, hogy ezt a rendkívüli képességet publikálni akarjuk, mint EvenetHub szolgáltatást. Ha megértettük az EventHub típusosságra, és főképp típusvezéreltségre vonatkozó elveit, akkor világos számunkra, hogy az EventHub szolgáltatásunk nem kaphat egyszerűen egy `List<int>` típust és nem térhet vissza egy `int` típussal, hanem létre kell hoznunk egy olyan üzenetosztályt, ami kifejezi, hogy ez az adott szolgáltatás egyetlen fenti képességéhez tartozó üzenetünk, és mi az arra adott válasz. (Különben a következő olyan szolgáltatás elemnél, amely alapvetően `List<int>` jellegű adatokkal dolgozik, és `int`-et ad vissza nagy bajban lennénk...)

Gondoljuk végig mit akarunk! Egy számokból álló listát küldeni, és a küldött számok összegeként előálló eredményt visszakapni. Előbbi a requestünk, utóbbi a response rá. A két üzenetet fogjuk össze egy Contract osztályba! Valami ilyesmi lesz:
```csharp
public class AddMachineContract
{
    public class AddThis
    {
        public List<int> Numbers { get; set; }
    }
    
    public class Result
    {
        public int Amount { get; set; }
    }
}
```
A fenti kód egy külön Class Library projektbe kerül, hogy egy önálló .Net Assembly-vé (dll) forduljon, és könnyen terjeszthető legyen a szolgáltatást használni kívánó kliensek számára!

Tegyük fel, hogy egy szinkron hívást (Call) akarunk megvalósítani egy elképzelt AddMachine kliensben. Mivel figyelmesen olvastuk az EventHub dokumentációját, így tudjuk, hogy a szinkron üzenet feldolgozáshoz egy olyan handlerre lesz szükségünk, amelyik `public Response<TResponse> DDD_MethodeName(Request<TRequest, Tresponse>)` szignatúrával rendelkezik, ahol TRequest = **AddMachineContract.AddThis**, és TResponse = **AddMachineContract.Result**. Valami ilyesmi lesz:
```csharp
public Response<AddMachineContract.Result> AddForSyncUse(
            Request<AddMachineContract.AddThis, AddMachineContract.Result> addingRequest)
{
    // A request-hez tartozó response pár beszerzése
    var myResponse = addingRequest.MyResponse;
    try
    {
        int result = AddAll(addingRequest.RequestContent.Numbers);
        // Visszatérési típus kreálása, és beállítása
        myResponse.ResponseContent = new AddMachineContract.Result { Amount = result };
    }
    catch(Exception e)
    {
        // Hibaág, hibakezelés
        myResponse.Exception = e;
    }
    return myResponse;
}
```
Ahhoz hogy ez egy EventHubon publikált szolgáltatássá váljon nem kell mást tenni, mint regisztrálni (Pl. az AddMachine construktorában) egy adott nevű csatornára a szolgáltatás képességet egy EventHub handler regisztráció formájában, amelyik a fenti `Request<AddThis, Result>` kéréshez bejegyez egy szinkron (tehát `Response<Result>` típust visszaadó) kezelő metódust (handler). Mégpedig a fent látható AddForSyncUse-t. Tehát:
```csharp
EventHubCore.RegisterHandler<RedisPubSubChannel, 
                            AddMachineContract.AddThis, 
                            AddMachineContract.Result>("addmachine", AddForSyncUse);
```
És ezzel a szolgáltatás oldalon publikáltuk is az EventHub-ra a képességet egy addmachine azonosítójú  csatornán. (Az egyszerű láthatóságért a példában ezt az azonosítót itt helytelenül a kódba égetett stringként adtuk meg, amit természetesen sosem követnénk el egy valós implementációban!)

Mit kell tenni a kliens oldalon való használathoz?
Egyszerűen meghívjuk a megfelelő üzenettel a szolgáltatást, hivatkozva a csatorna azonosítóra mint címre:
```csharp
public void TestSyncCall()
{
    var addThis = new AddMachineContract.AddThis 
                        { Numbers = new List<int>(new int[] { 1, 2, 3 }) };
    try
    {
        var result = EventHubCore.Call<RedisPubSubChannel, 
                                        AddMachineContract.AddThis, 
                                        AddMachineContract.Result>("addmachine", addThis);
        // válasz feldolgozása Pl. kííratás
        ...
        Console.WriteLine(result.Amount);
    }
    catch(Exception e)
    {
        // Hiba kezelés
    }
}
```
Természetesen a klinsnek ismerni kell: 
* A Contract (szolgáltatás szerződés) részét képező fenti típusokat (AddThis, Result), ahhoz hogy meg tudja hívni a szolgáltatást. (Erre mondtuk, hogy önálló dll-ként célszerű terjeszteni.)
* Magát az EventHubot is ismernie kell, és azt az EventHub Channel-t, amelyiken az üzenetközvetítés zajlik. Ezt a megfelelő Nuget csomagok használatával érjük el. A példánkhoz az Vrh.EventHub.Core és a Vrh.EventHub.Protocols.RedisPubSub Nuget csomagokra van szükség, és a Vrh.EventHub.Core és a Vrh.EventHub.Protocols.RedisPubSub névterek using-jával érhetjük el amire szükségünk van belőle (EventHubeCore static típus és a RedisPubSubChannel típus).
* Arra a csatorna azonosítóra is szükség van a kliens oldalon, amelyiken az elérni kívánt szolgáltatás/képesség elérhető. (Ez az amit az egyszerúség kedvéért mindkét oldalon beégettünk egy stringként a példában: addmachine). Vegyük észre az EventHub hívásokkal kapcsolatban, hogy a szolgáltatáscím az egy URI-nak felel meg. A generikusban meghatározott típus a protokoll kijelölés, a használt channelId pedig a tényleges szolgáltatás cím!  

#### Aszinkron, visszajelzés nélküli szolgáltatás képesség publikálása, és használata
Tegyük, fel, hogy Aszinkron is használni akarjuk az összeadás képességét, egy visszajelzés nélküli aszinkron szolgáltatásként.
Ekkor létre kell hoznunk egy ennek megfelelő metódust, amit mint EventHub kezelőt tudunk majd regisztrálni a szolgáltatás oldalon. A metódus nem ad vissza semmit (`void`) és közvetlenül AddMachineContract.AddThis típust fogad. Ilyesmi:
```csharp
public void ReceiveAddFullAsync(AddMachineContract.AddThis forAdding)
{
    int result = AddAll(forAdding.Numbers);
    // Feldolgozás: Pl az összes kapott szám hozzáadása valami központi counterhez
}
```
Bejegyezzük, mint EventHub handlert (publikálás):
```csharp
EventHubCore.RegisterHandler<RedisPubSubChannel, AddMachineContract.AddThis>(
                                                            "addmachine", ReceiveAddFullAsync);
```
Innentől kezdve az EventHub infrastruktúrán létezik az a képesség, hogy aszinkron módon fogadjon a végpont egy AddMachineContract.AddThis típusú üzenetet az addmachine azonosítójú RedisPubSubChannel típusú átviteli csatornán. Az egyezmény részleteinek ismeretében egy kliens meghívhatja bármikor ezt a szolgáltatást:
```csharp
public void TestAsyncSend()
{
    var addThis = new AddMachineContract.AddThis 
                        { Numbers = new List<int>(new int[] { 1, 2, 3 }) };
    EventHubCore.Send<RedisPubSubChannel, AddMachineContract.AddThis>("addmachine", addThis);
}
```
Vagy, mivel nincs visszajelzés teljesen aszinkron módon is hívhatja a szolgáltatást:
```csharp
EventHubCore.SendAsync<RedisPubSubChannel, AddMachineContract.AddThis>("addmachine", addThis);
```
A két hívási mód közt javarészt az dönthet, hogy kívánunk-e a kliens oldalán értesülni az infrastruktúra átviteli hibáiról, vagy sem. (Cserében a nagyobb teljesítményért, és a kisebb küldési költségekért, azaz a hívóoldali blokkolási idők minimalizálásáért.)
#### Aszinkron request/response szolgáltatás struktúrák kialakítása az EventHub beépített szolgáltatáskészletére építve
Tegyük fel, hogy összeadási képességünket valamiért aszinkron elvű request/response párra is elérhetővé akarjuk tenni, és ehhez az EventHub Core beépített szolgáltatásait felhasználjuk a `Request<TRequest, TResponse>` és a `Response<Tresponse>` generikus wrapperek használatával. Ekkor a szolgáltatás oldalán regisztrálnunk kell egy olyan metódust, amelyik `void` visszatéréssel rendelkezik és egy `Request<AddMachineContract.AddThis, AddMachineContract.Result>` paramétere van. Egy ilyen metódusra lesz szükségünk:
```csharp
public void AddForAsyncUse(
            Request<AddMachineContract.AddThis, AddMachineContract.Result> addingRequest)
{
    // A request-hez tartozó response pár beszerzése
    var myResponse = addingRequest.MyResponse;
    try
    {
        int result = AddAll(addingRequest.RequestContent.Numbers);
        // Visszatérési típus kreálása, és beállítása
        myResponse.ResponseContent = new AddMachineContract.Result { Amount = result };
    }
    catch(Exception e)
    {
        // Hibaág, hibakezelés
        myResponse.Exception = e;
    }
    // Válasz aszinkron visszaküldése a hívónak az EventHub Core Send metódusával
    EventHubCore.Send<RedisPubSubChannel, 
                        Response<AddMachineContract.Result>>("addmachine", myResponse);
}
```
Ezt a handlert a szolgáltatás oldalán bejegyezzük, hogy fogadja az aszinkron módon hozzá intézett kéréseket, amelyekben megkapja a kéréseket reprezentáló `Request<AddMachineContract.AddThis, AddMachineContract.Result>` üzeneteket:
```csharp
EventHubCore.RegisterHandler<RedisPubSubChannel, 
                        Request<AddMachineContract.AddThis, AddMachineContract.Result>>(
                            "addmachine", AddForAsyncUse);
```

A szolgáltatást használónak a kérésének az elküldése előtt be kell jegyeznie egy handlert, amivel a kérésére kapott választ fogadni fogja (fordított sorrend esetén nincs garancia arra, hogy a fogadás megtörténik):
```csharp
EventHubCore.RegisterHandler<RedisPubSubChannel, AddMachineContract.Result>(
            "addmachine", ReceiveAddMachineAsyncResult);
```
Az Aszinkron Response-ok fogadására a kérést küldő oldalán bejegyezendő handler szignatúrájának meg kell felelnie annak, hogy `void` visszatérési értékekkel rendelkezik és egy `Response<TResponse>` paraméterrel. Esetünkben ez lesz a bejegyzett ReceiveAddMachineAsyncResult metódus:
```csharp
public void ReceiveAddMachineAsyncResult(Response<AddMachineContract.Result> response)
{
    Program.Write($"Client side: Receive async response (Response<AddMachineContract.Result>) for    {response.RequestId} request, with this data: {response.ResponseContent.Amount}");
}
```
Amikor a kliens elküld egy kérést, akkor az EventHub Core majd ezt a metódust hívja vissza a válasszal az eredeti hívási hely végrehajtása pedig nem vár erre, hanem a küldés után azonnal folytatódik.
```csharp
var request = new Request<AddMachineContract.AddThis, AddMachineContract.Result>();
request.RequestContent = new AddMachineContract.AddThis { Numbers = _numbers };
EventHubCore.Send<RedisPubSubChannel, Request<AddMachineContract.AddThis, AddMachineContract.Result>>
    ("addmachine", request);
...
```
Természetesen a fenti példa igen leegyszerűsített, csak az EventHub kommunikációval kapcsolatban mutatja az aszinkron request/response megvalósítás lényegét. Egy valódi aszinkron request/response logika jóval bonyolultabb. Például a request küldését megakasztjuk egy semaforral, a handlerünk (ellenőrizve, hogy a fogadott üzenet a küldőhöz tartozik-e), gondoskodik a válasz elérhetővé tételéről a hívó számára, és reseteli a semafort. Vagy esetleg tárolásra kerülnek a kérések mellé a hozzá tartozó válaszok és pl. egy független prezentációs réteg ezekre építve dolgozik. De számtalan megoldás létezhet, melyeket - ha az aszinkron logika kialakítása valóban indokolt - úgyis meghatároz az aszinkron megvalósítási indok természete. 

Aszinkron logikát pedig soha ne alakítsunk ki "kivagyiságból". Ha ezen a szinten vagyunk programozásból (és nem a <a href="https://en.wikipedia.org/wiki/KISS_principle" target="_blank">**KISS**</a> tartásának szintjén), akkor úgyis harmatos lesz az implementációnk, csak gyengék vagyunk még az elkövetett hibák megértéséhez…
### Teljesítmény információk
Az EventHub teljesítménye nagymértékben függ a használt átviteli csatorna képességeitől. Maga  a Core jelentős átviteli teljesítményre és sebességre alkalmas. Ennek demonstrálására a konkrét RedisPubSub csatornamegvalósítás és egy teljesítmény mérésére készült példaimplementáció teljesítményéről osztunk meg információkat.

Az EventHub Core és a Redis is párhuzamos feldolgozású, a párhuzamos feldolgozási teljesítmény irányába természetes módon skálázódni képes megoldások. Ezért a mért teljesítmény adatok nagyban függenek attól, hogy az infrastruktúra elemei ugyanazon a gépen futnak-e vagy sem, és a futtató gépnek milyen párhuzamos feldolgozási képessége és teljesítménye van. Illetve aktuálisan használja-e a feldolgozási/számítási kapacitásait valami más is. A legjobb teljesítményt a Redis kiszolgáló, a szolgáltatás végpont és a szolgáltatást használó (kliens) végpont külön gépre telepítésével érhetjük el, ha ez indokolt. (*3 != 1 + 1 + 1*)

A tényleges teljesítmény azonban nagyban függ a tényleges implementáció erőforrásigényeitől, a megfelelő elvű és minőségű programozástól. Ezért az itt lévő adatok tájékoztató jellegűek, amit egy hibás handler implementáció teljes mértékben tönkretehet (a fogadó oldalon, illetve szinkron teljesítményt tekintve a hívási helyet is blokkolhatja timeoutig).

A tesztkörnyezet, amiben a mérés készült: 
* Mind a szolgáltatás végpont, mind a kliens végpont, mind maga az átvitelt biztosító Redis kiszolgáló ugyanazon a gépen dolgozik.
* A gép egy i7-es 4 fizikai maggal és magonként két párhuzamos feldolgozási threaddel. (Ez egy manapság (2018Q1) általános felső-közép kategóriás processzor teljesítmény.)
* Szinkron hívások esetén az EventHub válaszidejének jelentős része lehet a kapott üzenet feldolgozása a szolgáltatás végponton az üzenetet fogadó handlertől. Mivel a mérés az átviteli képesség demonstrációjára szolgál, ezért az implementációnk igen rövid idejű metódust (két egész szám összeadása) végez csak el.

Az eredményeink:
* Szinkron hívásokból (Call) hozzávetőleg **700 szinkron hívás/másodperc** az átbocsátási képesség. Egy szinkron hívás költsége nagyjából **1,4 ms**. (De ez az érték egy valós implementációban nagyban romolhat a feldolgozó oldal tényleges feldolgozásának a teljesítményének a függvényében.)
* Aszinkron hívás (Send, a küldő oldal nem vár választ a hívás helyén) hozzávetőleg **7200 hívás/másodperc** a hívó oldali átbocsátó képesség. Egy aszinkron küldés időköltsége így hozzávetőleg **0,139 ms**.
* Valódi aszinkron hívással (SendAsync) rendkívüli küldési teljesítmény érhető el. De a teljes párhuzamos feldolgozásra való skálázódás miatt, ez függ legjobban az éppen rendelkezésre álló párhuzamos számítási kapacitásoktól. Így a mérési eredmények rendkívül nagy szórást mutatnak. Ezen a módon ugyanakkor **néhány millió üzenet/másodperc** a küldő oldali átbocsátóképesség. Ami a küldő oldalon rendkívül alacsony **1/néhány millió ms** költséget jelent.
* A fogadó oldali feldolgozó képesség **8000 üzenet/másodperc** körül van. De ez nagyban függhet a fogadó handler tényleges implementációjától. Ez az adat egy szinkronizált, de kevés és kis költségű műveletet végző handler mellett érvényes. (Teljesen aszinkron elvű szinkronizált erőforrásokon nem osztozó handler implementációkkal a párhuzamos teljesítmény irányába skálázódásnak köszönhetően, ez a teljesítmény akár több nagyságrenddel is nőhet.)

Mivel az EventHub inicializáció mentes infrastruktúra, ezért az első hívás költsége több nagyságrendel nagyobb, mint további hívásoké. Ez az úgynevezett warmup time (amikor az EventHub végpont megkreálja a továbbiakban újrahasznosított infrastruktúrát) hozzávetőleg **360 ms** körül van.

Handler bejegyzési költségek üzenetfogadó végpontban:
* Az első handler bejegyzési ideje a RedisPubSub csatorna felett: **~260 ms**
* A további handlerek bejegyzési költsége ugynazon csatornára: **<<1 ms**

## Csatorna fejlesztési dokumentáció
**Egyelőre nincs meg a dokumentáció ezen része!!!** TODO: Megírandó!

<hr></hr>

## Version History:
### 1.2.0 (2019.09.25)
Compatibility API changes:
- Lehetőség van rá előírni, hogy a handler register figyelembe vegye a regisztrált handlert is, és nem írja felül, ha nem egyezik egy korábban regisztráltal. Ennek segítségével lehet ugyanabban az alkalmazás térben többszörös handlereket üzemeltetni ugyanarra a contractra. (Nincs rá további built-in támogatás ezért szinkron call-oknál nincs értelme. Ezért a lehetőség a szinkron call regisztrációknál továbbra sem érhető el!!!)

Patches:
- Ha több handler van regisztrrálva egy végponton, akkor az EventHub.Core gondoskodik róla, hogy az érkező üzenetekre az összes regisztrált kezelőt végighívja.
### 1.1.5 (2019.09.12)
Patches:
* EntryAssemblyFixer beépítése
### 1.1.4 (2019.05.21)
Patches:
* Channel register lock-ok pótlása
* Lognál a Handler nullvédelme a handlerregistereknél
### 1.1.3 (2019.05.21)
Patches:
* Upgrade Nugets
### 1.1.2 (2018.07.23)
Patches:
* Annak a javítása, hogy a DropHandler nem működik Call típusú eseménykezelők kiregisztrálásához

### v1.1.1 (2018.07.04)
Patches:
* Call (szinkron) hívások egy aszinkron metódusba terelve, hogy a SemaphoreSlim async waitje legyen használható (await)
* Refactoring result object beszerzés
* AutoReset Event kicserélve SemaphoreSlim-re a RegisteredCallWait objektumokban, hogy taskpool-ban is megfelelően működjön
* Methode Invoke nem taskalapú a handlerhívásokban

### v1.1.0 (2018.05.28)
Compatibility API changes:
* InitielizeChannel API hívás bevezetése

### v1.0.0 (2018.04.06)
* Initial release with documentation

### v1.0.0-pre-release (2018.03.26)
* Initial pr version without documentation

### v1.0.0-pre-alpha (2017.12.04)
* Prototype version
