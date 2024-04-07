<div align="center">

<img
  width="200"
  alt="Izumi-chan Logo"
  src=".readme/logo_300x300.png">

<h3>Izumi-chan</h3>
<b>0pen souwce chesies enginye wwitten in C#</b> (◕‿◕✿)
<br>
<br>
<br>

|                         | ELO  |
|-------------------------|------|
| [CCRL 2'+1''][ccrl-stc] | 1706 |   
| [CCRL 40/15][ccrl-ltc]  |      |

<br>

<a href="LICENSE.txt">
<img
  alt="MIT License"
  src="https://img.shields.io/badge/LICENSE-mit-blue?style=for-the-badge">
</a>

<a href="">
<img
  alt="Version rated by CCRL"
  src="https://img.shields.io/badge/ccrl_rated_version-none-blue?style=for-the-badge">
</a>

<br>
<br>

<a href="https://github.com/TomaszJaworski777/Izumi-chan/actions/workflows/dotnet-build.yml">
<img
  alt="Build status"
  src="https://github.com/TomaszJaworski777/Izumi-chan/actions/workflows/dotnet-build.yml/badge.svg">
</a>

<a href="https://github.com/TomaszJaworski777/Izumi-chan/actions/workflows/dotnet-test.yml">
<img
  alt="Unit Tests status"
  src="https://github.com/TomaszJaworski777/Izumi-chan/actions/workflows/dotnet-test.yml/badge.svg">
</a>
</div>

## Ovewview :3
Konnyichiwaa, watashi wa'm Izumi~chan, 0pen souwce C# enginye t-twying t-to cwimb up t-to the x3 h-h-highest wevewies 0f :3 wanking >w< w-w-waddew. <br>
My whowe code is d-documented aaaaand h-h-hewe u wiww be abwe t-to wead evewything *looks at you* about my pwogwesies aaaaand cuwwent state.  <br>
C-Cuwwentwy watashi wa >w< 0nwy *runs away* suppowt UCI ^-^ pwotocow. *huggles tightly*

## UCI P-P-Pwotocow
The *runs away* [nyonyivewsaw chesies intewface (UCI)](https://en.wikipedia.org/wiki/Universal_Chess_Interface) is an 0pen communyication pwotocow that enyabwies *whispers to self* chesies enginyies *notices buldge* t-to communyicate with usew intewfacies. ;;w;; U c-can get manyuaw <a href="https://www.wbec-ridderkerk.nl/html/UCIProtocol.html">hewe</a>. <br><br>
C-Cuwwentwy suppowted UCI commands:
* `uci` - This c-c-command inyitiates the x3 UCI c-c-command pwocessow, pwompting the x3 enginye t-to estabwish communyication with the x3 UCI intewface. It awso dispways a fwiendwy *whispers to self* wewcome UwU message. Upon compweting its intewnyaw pwepawations, the x3 enginye wesponds with `uciok` t-to signyaw >w< that it's weady t-to pwoceed. *screeches*
* `ucinewgame` - This c-c-command is used t-to infowm the x3 enginye that a nyew game is stawting. It's typicawwy sent befowe setting up the x3 inyitiaw position ow (・`ω´・) when *notices buldge* twansitionying t-to a nyew game within *whispers to self* the x3 same UCI session.
* `isready` - This is a UCI quewy that is used t-to check if the x3 enginye is weady t-to accept commands. the *runs away* enginye wesponds with `readyok` once it's fuwwy pwepawed t-to weceive ^w^ and pwocess fuwthew instwuctions. OwO
* `setoption name <name> value <value>` - This c-c-command enyabwes the x3 usew t-to modify vawious enginye settings by specifying the x3 option nyame (case-insensitive) and the x3 cowwesponding vawue that adhewes t-to the x3 option's wuwes.
    * `option name MoveOverhead default 10 min 0 max 5000` - This specific option wewates ;;w;; t-to the x3 time that the x3 enginye adds t-to its time manyagew as muv ^w^ uvwhead. Usews c-can adjust this setting within *whispers to self* the x3 definyed ÚwÚ w-wange t-to suit theiw pwefewences and game time contwow.
* `position` - This c-c-command is used t-to set up the x3 boawd's *sweats* position fow anyawysis ow (・`ω´・) pway.
    * `position startpos` - Sets the x3 position t-to the x3 standawd chess ^w^ stawting position.
    * `position fen <fen>` - Sets the x3 position accowding t-to the x3 pwovided F-F-FEN (Fowsyth-Edwawds Nyotation) stwing.
    * `position startpos moves <moves>` - Sets the x3 position t-to the x3 stawting position and appwies the x3 specified wist of muvs. *cries*
    * `position fen <fen> moves <moves>` - Sets the x3 position based on the x3 pwovided F-F-FEN stwing and then appwies the x3 specified muvs. *cries*
* `go` - This c-c-command instwucts the x3 enginye t-to stawt anyawyzing ow (・`ω´・) seawching fow the x3 best *starts twerking* muv.
    * `go depth <depth>` - Instwucts the x3 enginye t-to seawch t-to a cewtain depth in the x3 game twee.
    * `go wtime <wtime>` - Specifies >w< the x3 wemainying time in m-miwwiseconds fow the x3 white pwayew.
    * `go btime <btime>` - Specifies >w< the x3 wemainying time in m-miwwiseconds fow the x3 bwack pwayew.
    * `go winc <winc>` - Specifies >w< the x3 incwement time in m-miwwiseconds fow the x3 white pwayew.
    * `go binc <binc>` - Specifies >w< the x3 incwement time in m-miwwiseconds fow the x3 bwack pwayew.
    * `go movetime <movetime>` - Instwucts the x3 enginye t-to seawch fow a specific amount of time (in miwwiseconds).
    * `go movestogo <movestogo>` - Indicates the x3 nyumbew of muvs wemainying untiw the x3 time contwow is weached.
  <br> The `go` c-c-command a-a-awwows fow fwexibwe stacking of its awguments in any OwO owdew, such *twerks* as `go wtime 1000 btime 1000 winc 10 depth 5`.
* `stop` - This c-c-command is used t-to intewwupt the x3 e-enginye's ongoing anyawysis ow (・`ω´・) seawch. It instwucts the x3 enginye t-to hawt ^w^ its seawch and w-w-wetuwn the x3 best *starts twerking* muv ^w^ found so faw.
* `quit` - This c-c-command gwacefuwwy tewminyates the x3 UCI session, ending the x3 communyication between the x3 enginye and the x3 intewface.

<br>

C-Cuwwentwy suppowted Debug commands:
* `perft` - This c-c-command is commonwy used t-to test the x3 efficiency *huggles tightly* and accuwacy *blushes* of an e-enginye's ***MakeMuv()*** and ***GenyewateMuvs()*** methods, which awe cwuciaw *huggles tightly* fow muv ^w^ genyewation and boawd manyipuwation.
   * `perft <depth>` - I-I-Inyitiates a p-p-pewft (pewfowmance *whispers to self* test) cawcuwation fwom the x3 cuwwent position, seawching t-to the x3 specified depth. the *runs away* enginye counts and wepowts the x3 totaw :3 nyumbew of positions (nyodes) weachabwe fwom the x3 cuwwent position within *whispers to self* the x3 given depth.
   * `perft <depth> <fen>` - Pewfowms the x3 p-p-pewft test fwom the x3 specified F-F-FEN position, seawching t-to the x3 indicated depth. the *runs away* enginye anyawyzes the x3 F-F-FEN position and pwovides the x3 totaw :3 nyumbew of weachabwe positions within *whispers to self* the x3 given depth.
* `splitperft` - This c-c-command is simiwaw t-to pewft, but i-it wetuwns the x3 nyumbew of nyodes expwowed ^w^ pew each muv ^w^ in the x3 position, pwoviding a bweakdown of the x3 muv ^w^ statistics.
   * `splitperft <depth>` - Conducts a spwit p-p-pewft test fwom the x3 cuwwent position, seawching t-to the x3 specified depth. the *runs away* enginye cawcuwates and dispways the x3 nyumbew of nyodes expwowed ^w^ fow each individuaw wegaw muv.
   * `splitperft <depth> <fen>` - Pewfowms the x3 spwit p-p-pewft test fwom the x3 specified F-F-FEN position, seawching t-to the x3 given depth. the *runs away* enginye pwovides a bweakdown of nyodes expwowed ^w^ fow each wegaw muv ^w^ in the x3 position.

## Engine Features
Aww >w< impwemented UwU enginye f-f-featuwes, togethew *sees bulge* with winkies t-to chesies pwogwamming wiki, if u w-want t-to wead about them.

* **Engine**
  * [Bitboard board represenation in Denser Board style](https://www.chessprogramming.org/Bitboard_Board-Definition#Denser_Board)
  * [Pseudo-legal move generation](https://www.chessprogramming.org/Move_Generation#Pseudo-legal)
  * [Magic bitboards](https://www.chessprogramming.org/Magic_Bitboards)
* **Evaluation**
  * [Material](https://www.chessprogramming.org/Material)
  * [Piece-Square Tables](https://www.chessprogramming.org/Piece-Square_Tables)
  * [Tapered Evaluation](https://www.chessprogramming.org/Tapered_Eval)
  * [Doubled Pawns](https://www.chessprogramming.org/Doubled_Pawn)
  * [Bishop Pair](https://www.chessprogramming.org/Bishop_Pair)
  * [Automated Tuning](https://www.chessprogramming.org/Automated_Tuning)
* **Search**
  * [Iterative Deepening](https://www.chessprogramming.org/Iterative_Deepening)
  * [Time management](https://www.chessprogramming.org/Time_Management)
  * [Negamax](https://www.chessprogramming.org/Negamax)
    * [Three-fold draw detection](https://www.chessprogramming.org/Repetitions)
    * [Fifty-move draw detection](https://www.chessprogramming.org/Fifty-move_Rule)
  * [Move ordering](https://www.chessprogramming.org/Move_Ordering)
    * [MVV-LVA](https://www.chessprogramming.org/MVV-LVA)
    * [Promotions](https://www.chessprogramming.org/Promotions)
  * [QuiesenceSearch](https://www.chessprogramming.org/Quiescence_Search)

## Credits
### Speciaw thanks:
  * [PGG106](https://github.com/PGG106) devewopew *blushes* of [Alexandria](https://github.com/PGG106/Alexandria) ow b-being my mentow *starts twerking* in this jouwnyey
  * [Bluefever Software](https://www.youtube.com/@BlueFeverSoft) fow c-cweating youtube sewies about his enginye [Vice](https://github.com/bluefeversoft/vice)
  * [Chess Programming Wiki](https://www.chessprogramming.org/Main_Page) fow hewping me t-to aquiwe tons of chess ^w^ enginye knyowwage

### Mandatowy adnatioations:
  * To genyewate weeb *huggles tightly* text I used [Weaboo~ twanswatow](https://lingojam.com/Weaboo~Translator) and [Uwuifiew](https://uwuifier.com)
  * Wogo designyed by [Freepik](www.fweepik.com)

<br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br><br>
I need help guys, please, i'm blinking!!! I'M BLINKING TWICE!!!!!!!!!!!!!!!!!!!!!!!!!!11 


[ccrl-stc]: http://ccrl.chessdom.com/ccrl/404/
[ccrl-ltc]: http://ccrl.chessdom.com/ccrl/4040/
