 # Vrh.EventHub.Protocols.RedisPubSub
 
 ![](http://nuget.vonalkod.hu/content/projectavatars/eventhubredispubsub.png)
 
Ez a leírás a komponens **2.0.7** kiadásáig bezáróan naprakész.
Igényelt minimális framework verzió: **4.5**
Teljes funkcionalitás és hatékonyság kihasználásához szükséges legalacsonyabb framework verzió: **4.5**

**A komponens arra szolgál, hogy egységes és egyszerű megvalósítása legyen .Net alkalmazás modulok közti kommunikációnak. Attól függetlenül, hogy azok  közös alakalmazástérben működnek, vagy sem. Ez a komponens <a href="https://redis.io/" target="_blank">Redis</a> infrastruktúra felett, a Rudis / valósít meg egy olyan átviteli csatornát, amelyik a <a href="https://redis.io/topics/pubsub" target="_blank">Redis Pub/Sub szolgáltatásra</a> épül.**

Az alábbiak igazek rá:
* Rendkivül gyors, a végpontok TCP kapcsolatára épített átvitel.
* Online, üzenetperzisztánciát nem biztosító, a Redis Pub/Sub feliratkozás/publikálás elveivelk egyező működésű átviteli csatorna EventHub végpontok közt.

**Használatához a <a href="http://nuget.vonalkod.hu/packages/Vrh.EventHub.Protocols.RedisPubSub" target="_blank">Vrh.EventHub.Protocols.RedisPubSub nuget csomag</a> telepítendő. A felhasználás helyén a `Vrh.EventHub.Protocols.RedisPubSub` névétér includolandó. A csatorna az EventHub.Core generikusokban használandó típusneve: `RedisPubSubChannel`**

```csharp
using Vrh.EventHub.Core;
using Vrh.EventHub.Protocols.RedisPubSub;

...

{
    ...
    EventHubCore.RegisterHandler<RedisPubSubChannel, SampleMessage>("ch1", SampleMessageProcessor);
    ...
    EventHubCore.Send<RedisPubSubChannel, TwoNumber>("ch1", new TwoNumber() { One = 1, Two = 2 });
    ...
    // Stb.
}
```
## Konfigurációs lehetőségek
A Csatornán két beállítást tudunk definició szintjén konfigurálni:
1. **A csatornán alapértelmezésként használt timeout-ot**. Ez adja meg, hogy a szinkron EventHub hívások (Call) meddig várjanak a válaszra ha a RedisPubSub csatornát használják, és a Call-ban nem kerül definiálásra  timeout értéke.
2. **Azt a Redis connection aliast**, amelyik megadja, hogy melyik Redis kiszolgálót kell az EventHub végpontnak mint kommunikácioós infrastrúktárát használnia az üzenettovábbításra. Ez egy **Vrh.Redis.ConnectionStore** szerinti érvényes alias név. (Ennek részleteit lásd a <a href="http://nuget.vonalkod.hu/packages/Vrh.Redis.ConnectionStore/" target="_blank">Vrh.Redis.ConnectionStore Nuget</a> csomag <a href="http://gitlab.vonalkod.hu:443/vrh/Vrh.Redis.ConnectionStore/blob/developer/ReadMe.md" target="_blank">dokumentációjában</a>.) 

A beállítások megadásának kért módja van, vagy a hostoló alaklamzás application configjába kerülnek szabvány app settings kulcsként. Vagy ha itt nem adjuk meg őket akkor szabvány Vrh konfigurációs XML-ben is elhelyezhetőek.

### Applicattion setting kulcsok
#### Vrh.EventHub.RedisPubSub:ChannelTimeout kulcs
Definiálja a csatornán alapérttelmezésként használt timeout érétkét.
```xml
<appSettings>      
  <add key="Vrh.EventHub.RedisPubSub:ChannelTimeout"
        value="00:00:00.1" />
</appSettings>
```
Ahol az érték egy érvényes TimeSpan string lehet HH:MM:SS.ZZZ formában, ahol:
* **HH** órát
* **MM** percet
* **SS** másodpercet
* **ZZZ** törtmásodpercet (a helyiértéknek megfelelő rész)

definiál. (Tehát fenti érték 1 tizedmásodperces timeout-ot jelent.)

#### Vrh.EventHub.RedisPubSub:RedisConnection
Definiélja az EventHub végpont által használt Redis Connection aliast, mint **Vrh.Redis.ConnectionStore** szerinti absztrakciót.
```xml
<appSettings>      
  <add key="Vrh.EventHub.RedisPubSub:RedisConnection"
        value="EventhubRedisInfrastructure" />
</appSettings>
```
Ahol az érétk egy tetszőlsegs string lehet.

### VRH Konfigurációs XML
Ha a beállításokat nem app settings kulcsként adjuk meg, akkor lehetőségünk van VRH konfigurációs XML-ben elhelyezni. Az XML releváns szerkezeti elemeit az alábbi példa szemlélteti:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<Vrh.EventHub.RedisPubSub>
  <RedisConnection>redis</RedisConnection>
  <ChannelTimeout>00:00:00.1</ChannelTimeout>
</Vrh.EventHub.RedisPubSub>
```

Ahol a megadható értékek a fenti app settings kulcsaknál leírtakkal ekvivalensek. Alapértelmezésben a fenti tartalommal az XML fájlt a hostoló alkalmazás könyvtárában kell elhelyezni a `Vrh.EventHub.RedisPubSub.Config.xml` néven.

Ha a fenti alapértelmezéstől el kívánunk térni, akkor beállítást tartalmazó XML fájlt és a beállítások XML tagjeit tartalmazó abban elfoglalt helyét definiálhatjuk a szokásos leírással a hostoló alkalmazás app configjában elhelyezett application settings kulccsal:
```xml
<appSettings>      
  <add key="Vrh.EventHub.RedisPubSub:Config" value="@Vrh.EventHub.RedisPubSub.Config.xml/Vrh.EventHub.RedisPubSub" />
</appSettings>
```
Ahol:
* **@** prefix segítségével írjuk alő, hogy a megadott fájlnév relatív útvonal definíció a hostoló alkalmazás munkakönyvtárához képest. Ha abszolút útvonalakkal definiálunk, hagyjuk el a @ jelet!
* A fájl útvonalakban a **\\** karaktert használjuk elválasztóként.
* A fájl definicó után egy **/** után lehetőségünk van definiálni azt az XML-ben lévő útvonalat, ahol a konfiguráció található. Az egyes XML tageket a **/** jel választja el egymástól.

<hr></hr>

## Version History:
### 2.0.7 (2019.09.25)
Patches:
1. Upgrade EventHub.Core to v1.2.0
### 2.0.6 (2019.09.12)
Patches:
1. Upgrade EventHub.Core to v1.1.5 (függőség kijavítása)
### 2.0.5 (2019.09.12)
Patches:
1. Upgrade EventHub.Core to v1.1.5
### 2.0.4 (2019.05.21)
Patches:
1. Upgrade EventHub.Core to v1.1.4
### 2.0.3 (2019.05.21)
Patches:
1. Upgrade Nugets
### 2.0.2 (2018.07.23)
Patches:
1. Core patch (1.1.2) életbe léptetése (csommag függőség az EventHub.Core 1.1.2-re emelve)
### 2.0.1 (2018.07.04)
Patches:
1. Redis küldés és fogadás áttéve önáló szállra (taskpool környezetben való jobb teljesítményért)
### 2.0.0 (2018.05.28)
Incopatibility API Changes:
1. Vrh.Redis.ConnectionStore remove (revert to static lazy base ConnectionMultiplexer share sample)
2. Configuration XML: RedisConnectionAlias XML tag rename to RedisConnection
### 1.0.0 (2018.04.06)
* Initial release with documentation
## 1.0.0-prerelease (2018.03.26)
* Initial prerelease without documentation
## 1.0.0-pre-alpha (2017.12.04)
* Prototype version