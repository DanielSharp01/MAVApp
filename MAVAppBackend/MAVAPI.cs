using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MAVAppBackend
{
    /// <summary>
    /// Exception thrown if the parsing of MAV API returned HTMLs fail
    /// </summary>
    public class MAVAPIException : Exception
    {
        public MAVAPIException(string message) : base(message)
        { }
    }

    /// <summary>
    /// Contains API calls for the MAV "APIs"
    /// </summary>
    public class MAVAPI
    {
        /// <summary>
        /// List of MÁV's autocomplete stations
        /// </summary>
        public static string[] AllStationNames = new string[] {
            "Abádszalók","Abaliget","Aba-Sárkeresztúr","Abaújkér","Abaújszántó","Abaújszántói fürdő","Abda","Abony","Abonyi út","Ábrahámhegy","Ács","Acsád","Acsa-Erdőkürt","Adács","Ádánd","Adony","Agárd","Aghires","Aitos","Ajak",
            "Ajka","Ajka-Gyártelep","Akali [Balatonakali-Dörgicse]","Akarattya [Balatonakarattya]","Alagimajor","Alap","Alba Iulia","Albertfalva","Albertirsa","Aleksinac","Alesd","Algyő","Aliga [Balatonaliga]","Állami telepek [Gödöllő Állami telepek]","Almádi [Balatonalmádi]","Almár","Almás","Almásfüzitő","Almásfüzitő felső","Alsóbélatelep",
            "Alsóboldva","Alsógalla","Alsógöd","Alsógyenes","Alsóméntelek","Alsómocsolád [Mágocs-Alsómocsolád]","Alsónemesapáti","Alsónyék","Alsóörs","Alsórönök","Alsószeleste [Ölbő-Alsószeleste]","Alsóúrrét","Alsóváros [Vác-Alsóváros]","Alvinc [Vintu De Jos]","Ambrózfalva","Andocs","Andornaktálya","ANDRAE AM ZICKSEE ST [St. Andrä am Zicksee]","Andráshida","Angererbach-Ahrnbach",
            "Angyalföld","ANTON AM ARLBERG ST [St.Anton Am Arlberg]","Apa","Apafa","Apagy","Apahida","Apaj [Dömsöd]","Apátfalva","Apc-Zagyvaszántó","Aquincum","Arács [Balatonarács]","Arad","Aradványpuszta","Aranyosapáti","Aranyosgyéres [Campia Turzii]","Aranyosmeggyes [Mediesu Aurit]","Aranyvölgy","Árpádszállás","Aschau Im Zillertal (Zb)","Asparuhovo",
            "Aszaló","Aszód","Aszófő","Áta","Átkelés [Dömösi átkelés]","Attala","Augsburg Hbf","Auschwitz [Oswiecim]","B.akali [Balatonakali-Dörgicse]","B.akarattya [Balatonakarattya]","B.aliga [Balatonaliga]","B.almádi [Balatonalmádi]","B.arács [Balatonarács]","B.berény [Balatonberény]","B.boglár [Balatonboglár]","B.ederics [Balatonederics]","B.fenyves [Balatonfenyves]","B.fenyves alsó [Balatonfenyves alsó]","B.fenyves GV [Balatonfenyves GV]","B.főkajár felső [Balatonfőkajár felső]",
            "B.földvár [Balatonföldvár]","B.füred [Balatonfüred]","B.füzfő [Balatonfűzfő]","B.györök [Balatongyörök]","B.kenese [Balatonkenese]","B.kenese-Üdülőtelep [Csittényhegy]","B.lelle [Balatonlelle]","B.lelle felső [Balatonlelle felső]","B.máriafűrdő [Balatonmáriafürdő]","B.máriafűrdő alsó [Balatonmáriafürdő alsó]","B.rendes [Balatonrendes]","B.szárszó [Balatonszárszó]","B.szemes [Balatonszemes]","B.szentgyörgy [Balatonszentgyörgy]","B.szepezd [Balatonszepezd]","B.széplak alsó [Balatonszéplak alsó]","B.széplak felső [Balatonszéplak felső]","B.udvari [Balatonudvari]","B.világos [Balatonvilágos]","Babócsa",
            "Bábolna [Nagyigmánd-Bábolna]","Bábonymegyer","Backa Topola","Bácsalmás","Bácsbokod-Bácsborsód","Bácsborsód [Bácsbokod-Bácsborsód]","Bad Neusiedl Am See (Roeee)","Bad Schandau","Badacsony","Badacsonylábdihegy","Badacsonyörs","Badacsonytomaj","Badacsonytördemic-Szigliget","Bag","Bagimajor","Bagod","Baia Mare","Băile Herculane","Băile Tuşnad","Baja",
            "Baja-Dunafürdő","Bajánsenye","Bak","Bakonygyirót","Bakonysárkány","Bakonyszentlászló","Baktalórántháza","Baktüttös","Balassagyarmat","Balástya","Balatonakali-Dörgicse","Balatonakarattya","Balatonaliga","Balatonalmádi","Balatonarács","Balatonberény","Balatonboglár","Balatonederics","Balatonfenyves","Balatonfenyves alsó",
            "Balatonfenyves GV","Balatonfőkajár felső","Balatonföldvár","Balatonfüred","Balatonfűzfő","Balatongyörök","Balatonkenese","Balatonkenese-Üdülőtelep [Csittényhegy]","Balatonlelle","Balatonlelle felső","Balatonmáriafürdő","Balatonmáriafürdő alsó","Balatonrendes","Balatonszárszó","Balatonszemes","Balatonszentgyörgy","Balatonszepezd","Balatonszéplak alsó","Balatonszéplak felső","Balatonudvari",
            "Balatonvilágos","Balázsfalva [Blaj]","Balkan","Balmazújváros","Balogunyom [Ják-Balogunyom]","Balota (RO)","Balotaszállás","Bánffyhunyad [Huedin]","Bánhalma-Halastó","Bánhida","Bánkút","Bánóc [Banovce Nad Ondavou]","Banovce Nad Ondavou","Bánréve","Bánrévei Vízmű","Bányatelep [Kisterenye-Bányatelep]","Bar","Baracska","Báránd","Baranovichi-Centralnye",
            "Barátudvar-Féltorony [Mönchhof-Halbturn [ÖBB]]","Barcs","Barcs felső","Barosstelep","Bashalom","Bátaszék","Baté","Batevo","Batiz [Botiz]","Battonya","Batyk [Zalabér-Batyk]","Bátyú [Batevo]","Bazaltbánya [Sümegi Bazaltbánya]","Becehegy","Beclean Pe Somes","Bécs [WIEN*]","BECS FOEPALYAUDVAR [Wien Hbf]","Bécs Westbahnhof [Wien Westbf]","Becske alsó","Békéscsaba",
            "Bela Palanka","Bélapátfalva","Bélapátfalvi Cementgyár","Bélatelep","Bélavár","Belecska","Beled","Beleg","Belezna","Belgrad [Beograd]","Belgrád [Beograd]","BELJAK GL.KOL. [Villach Hbf]","Beloiannisz [Iváncsa]","Beloslav","Belsőbáránd","Belsőkamaráspuszta","Belsőperegpuszta","Benesat","Beograd","Berekfürdő",
            "Berente","Berettyóújfalu","Berkenye","BERLIN*","Berlin Hbf (Tief)","Berlin Südkreuz","Berlin-Spandau","Berzence","Beska","Besnyő [Iváncsa]","Bethlen [Beclean Pe Somes]","Bezenye","Biala Podlaska","Biatorbágy","Bicere","Bicsérd","Bicske","Bicske alsó","Bihar [Biharia]","Bihardiószeg [Diosig]",
            "Biharia","Biharkeresztes","Biharnagybajom","Biharpüspöki [Episcopia Bihor]","Bijelo Polje","Blaj","Bludenz","Boba","Bocfölde","Bocskaikert","Bodajk","Bodolyabér","Bodrogkeresztúr","Bodrogkisfalud [Bodrogkeresztúr]","Bodrogolaszi","Bódvaszilas","Boglár [Balatonboglár]","Bohumin","BOJCHINOVTSI [Boychinovtsi]","Boldogasszony [Frauenkirchen (Roeee)]",
            "Boldogkőváralja","Boldva","Bolhás","Bóly","Bonyhád [Hidas-Bonyhád]","Bonnya","Borovnica","Borsihalom","Borsodszirák","Borsosberény","Botiz","Bov","BOV [Bov]","Boychinovtsi","Bőcs [Hernádnémeti-Bőcs]","Börgönd","Bősárkány","Bösztör","Bp [BUDAPEST*]","Branesci",
            "Brasov","Brassó [Brasov]","Bratca","BRATISLAVA*","Bratislava hl. st.","Bratislava-N. M.","Bratislava-Petrzalka","Bratislava-Vinohrady","Breclav","Breitenbrunn","Brest-Centralnyi","BRNO*","Brno hl. n.","Brodarevo","Bruck a.d. Leitha","Brusartsi","BRUSARTSI [Brusartsi]","Brünn [BRNO*]","Buchs(SG)","Buchs Sg",
            "BUCURESTI*","Bucureşti Nord","Búcsúszentlászló","Budafa","Budafok","Budafok-Albertfalva [Albertfalva]","Budafok-Belváros [Budafok]","Budafok-Háros [Háros]","Budai út","Budaörs","BUDAPEST*","Budapest Airport [Ferihegy]","Budapest City [BUDAPEST*]","Budapest Déli [Budapest-Déli]","Budapest Ferencváros [Ferencváros]","Budapest Ferihegy [Ferihegy]","Budapest Flughafen [Ferihegy]","Budapest Kelenföld [Kelenföld]","Budapest Keleti [Budapest-Keleti]","Budapest Kőbánya-Kispest [Kőbánya-Kispest]",
            "Budapest Nyugati [Budapest-Nyugati]","Budapest Soroksári út [Soroksári út]","Budapest Üröm [Üröm]","Budapest Zugló [Zugló]","Budapest-Angyalföld [Angyalföld]","Budapest-Déli","Budapest-Keleti","Budapest-Nyugati","Budatétény","Buehel","Bukarest [BUCURESTI*]","Burgas","BURGAS [Burgas]","Burgasz [Burgas]","Busag","Buság [Busag]","Busteni","Buzet","Büchen","Büdöskútpuszta",
            "Bük","Bükkösd","Bytca","Čadca","Cadca Gr.","Campia Turzii","Campona [Budatétény]","Caracal","Caransebeş","Carei","Cece","Cefa","Cegléd","Ceglédbercel","Ceglédbercel-Cserő","Ceglédi szállások","Celje","Celldömölk","CELOVEC GL.KOL. [Klagenfurt Hbf]","CELOVEC HL.N. [Klagenfurt Hbf]",
            "Cementgyár [Bélapátfalvi Cementgyár]","Česká Trebová","Cesky Tesin","Chop","Cicarlau","Cicevac","Cikó","Ciucea","Cluj Napoca","Codlea","Copsa Mica","Craiova","Cukorgyár [Sarkadi Cukorgyár]","Curtici","Czechowice Dziedzice","Csabacsűd","Csabacsűd felső","Csaca [Čadca]","Csajág","Csákánydoroszló",
            "Csanádpalota","Csánig","Csány [Hort-Csány]","Csap [Chop]","Csapókert [Debrecen-Csapókert]","Csárdaszállás","Csengele","Csenger","Csengőd","Csépa","Cserdi-Helesfa","Cserkút [Mecsekalja-Cserkút]","Csermajor [Vitnyéd-Csermajor]","Cserő [Ceglédbercel-Cserő]","Csesznek [Porva-Csesznek]","Csibrák","Csikóstőttős","Csíkszentdomokos [Izvoru Oltului]","Csíkszereda [Miercurea Ciuc]","Csincse",
            "Csittényhegy","Csobád","Csókakő","Csomád","Csoma-Szabadi","Csongrád","Csongrád alsó","Csongrádi úti tanyák","Csop [Chop]","Csopak","Csorba [Strba]","Csorna","Csorvás","Csorvás alsó","Csömödér-Páka","Csörög","Csucsa [Ciucea]","Csugar","Csurgó","D.almás [Dunaalmás]",
            "D.haraszti [Dunaharaszti]","D.haraszti alsó [Dunaharaszti alsó]","D.keszi [Dunakeszi]","D.keszi alsó [Dunakeszi alsó]","D.újváros [Dunaújváros]","D.újváros külső [Dunaújváros külső]","D.varsány [Dunavarsány]","Dabas","Dabovo","DABOVO [Dabovo]","Dabronc","Dalgopol","DALGOPOL [Dalgopol]","Darány","Daránypuszta","Debrecen","Debrecen-Csapókert","Debrecen-Kondoros","Debrecen-Szabadságtelep","Decin hl. n.",
            "Decs","Deda","Déda [Deda]","Dej Călători","Dejtár","Délegyháza","Déli (Budapest) [Budapest-Déli]","Déli (Szőny) [Szőny-Déli]","Demecser","Demir Kapija","Dénesfa","Derecske","Derecske-Vásártér","Dés [Dej Călători]","Deszk","Deva","Déva [Deva]","Dévaványa","Devecser","Dimitrovgrad (SZ)",
            "Dimitrovgrad Granica","Dimovo","DIMOVO [Dimovo]","Dinnyés","Diósd [Nagytétény-Diósd]","Diosig","Diósjenő","Diószeg [Diosig]","Divaca","Domanesti","Dombostanya","Dombóvár","Dombóvár alsó","Domony [Iklad-Domony]","Domony felső [Iklad-Domony felső]","Donát","Donnerskirchen","Dorog","Döbrököz","Dömösi átkelés",
            "Dömsöd","Dörgicse [Balatonakali-Dörgicse]","Draganesti Olt","Dragoman","DRAGOMAN [Dragoman]","Drégely","Drégelypalánk","Drégelyvár","Drenovets","DRENOVETS [Drenovets]","Dresden Hbf","Dresden-Neustadt","Drezda [Dresden Hbf]","Drobeta Tr.Severin","Drujba","Dubicsány","Dubnica Nad Vahom","Duga Resa","Dugo Selo","Dunaalmás",
            "Dunafürdő [Baja-Dunafürdő]","Dunaharaszti","Dunaharaszti alsó","Dunai Finomító","Dunakeszi","Dunakeszi alsó","Dunakeszi-Gyártelep","Dunaújváros","Dunaújváros külső","Dunavarsány","Dúzs","Ebes","Ecser","Edelény","Edelény alsó","Ederics [Balatonederics]","Eger","Egeres [Aghires]","Egerfarmos","Eger-Felnémet",
            "Egervár","Egervár-Vasboldogasszony","Egyed-Rábacsanak","Egyek","Egyházasfalu [Nemeskér-Egyházasfalu]","Egyházasrádóc","Egyházerdő","Eisenstadt","Eisenstadt Schule","Élesd [Alesd]","Eliseyna","ELISEYNA [Eliseyna]","Előhát","Emőd","Encs [Forró-Encs]","Enese","Eperjes [Prešov]","Eperjeske alsó","Episcopia Bihor","Eplény",
            "Ercsi","ÉRD* (Érd alsó, Érd felső)","Érd","Érd alsó","Érd felső","Érdliget","Erdőbénye","Erdőkertes","Erdőkürt [Acsa-Erdőkürt]","Erdőszél","Erdőtelek","Erlach Im Zillertal","Érmihályfalva [Valea lui Mihai]","Erőmű (Mátravidéki) [Mátravidéki Erőmű]","Érsekújvár [Nové Zámky]","Érselénd [Silindru]","Esztár [Pocsaj-Esztár]","Esztergom","Esztergom-Kertváros","Eternitgyár",
            "Fábiánsebestyén","Fácánkert","Fagaras","Farád","Farmos","Fegyvernek-Örményes","Fehérgyarmat","Fehring","Feketehalom [Codlea]","Feketeváros [Purbach Am Neusiedlersee]","Feldbach","Feldkirch","Felnémet [Eger-Felnémet]","Felsőgöd","Felsőjánosfa","Felsőlajos","Felsőméntelek","Felsőmocsolád","Felsőpakony","Felsőrajk",
            "Felsőzsolca","Fenékpuszta","Fényes","Fényeslitke","Fenyves [Balatonfenyves]","Fenyves alsó [Balatonfenyves alsó]","Fenyveshegy","Ferencváros","Ferihegy","Fertőboz","Fertőd [Fertőszéplak-Fertőd]","Fertőfehéregyháza [Donnerskirchen]","Fertőszéleskút [Breitenbrunn]","Fertőszentmiklós","Fertőszéplak-Fertőd","Fiatfalva [Filiasi]","Filiasi","Fityeház","Fiume [Rijeka]","Fogaras [Fagaras]",
            "Fony","Fonyód","Fonyódliget","Forró-Encs","Fót","Fótfürdő","Fótújfalu","Főkajár felső [Balatonfőkajár felső]","Fövenyes","FRANKFURT(M)*","Frankfurt(M)-Flughafen Fernbf.","Frankfurt(Main)Hbf","Frauenkirchen (Roeee)","Fuegen-Hart (Zb)","Füle","Fülöpszállás","Fürjes","Füzesabony","Füzesbokor","Füzesgyarmat",
            "Füzesgyarmatfürdő","Füzfő [Balatonfűzfő]","Gacsály","Gádoros","Gagering","Galambos","Galgaguta","Galgagyörk","Galgahévíz","Galgamácsa","Galgau","Gálos [Gols]","Gárdony","Gátér","Gattendorf","Gecse-Gyarmat","Gégény","Gelse","Gencsapáti alsó","Gencsapáti felső",
            "General Gh. Avramescu","Gevgelija","Gheorgheni","Gherla","Giurgiu Nord","Gleisdorf","Gniebing","Godisa","Gógánfa","Golenti","Golop","Gols","Gornje Lezece","Gospic","Göd","Göd alsó [Alsógöd]","Göd felső [Felsőgöd]","Gödöllő","Gödöllő Állami telepek","Gömöri [Miskolc-Gömöri]",
            "Gönc","Göncruszka","Görögszállás","Götzendorf","Gramatneusiedl","GRAZ*","Graz Don Bosco","Graz Hbf","Graz Liebenau Murpark","Graz Ostbf","Gutorfölde","Günzburg","Gyál","Gyál felső","Gyanafalva [Jennersdorf]","Gyarmat [Gecse-Gyarmat]","Gyártelep (Ajka) [Ajka-Gyártelep]","Gyártelep (Dunakeszi) [Dunakeszi-Gyártelep]","Gyártelep (Tiszafüred) [Tiszafüred-Gyártelep]","Gyárváros [Győr-Gyárváros]",
            "Gyékényes","Gyenesdiás","Gyergyószentmiklós [Gheorgheni]","Gyoma","Gyón","Gyopárosfürdő","Gyömöre","Gyömöre-Tét","Gyömrő","Gyöngyfa-Magyarmecske","Gyöngyös","Gyöngyösfalu","Gyöngyöshalász","Gyöngyöshermán","Gyönk [Keszőhidegkút-Gyönk]","Győr","Győrasszonyfa","Győr-Gyárváros","Györök [Balatongyörök]","Győrszabadhegy",
            "Győrszemere","Győrszentiván","Győrtelek","Győrtelek alsó","Győrvár","Gyula","Gyulafehérvár [Alba Iulia]","Gyulai városerdő","Gyüre","H.böszörmény [Hajdúböszörmény]","H.dorog [Hajdúdorog]","H.hadház [Hajdúhadház]","H.nánás [Hajdúnánás]","H.sámson [Hajdúsámson]","H.szentgyörgy [Hajdúszentgyörgy]","H.szoboszló [Hajdúszoboszló]","H.vid [Hajdúvid]","Hajdúböszörmény","Hajdúdorog","Hajdúhadház",
            "Hajdúnánás","Hajdúnánás-Vásártér","Hajdúsámson","Hajdúszentgyörgy","Hajdúszoboszló","Hajdúvid","Hajmáskér","Hajnalos","Haláp","Halastó [Bánhalma-Halastó]","Halastó [Hortobágyi Halastó]","Halászlak","Halipuszta","Halmaj","HAMBURG*","Hamburg Dammtor","Hamburg Hbf","Hamburg-Altona","Hamburg-Bergedorf","Hanságliget",
            "Hanság-Nagyerdő","Hanusovce Nad Toplou Mesto","Haris","Harkakötöny","Hármasút [Kisvárda-Hármasút]","Háros","Hársliget [Lipovci]","Hasznos [Mátraszőlős-Hasznos]","Hatvan","Hegyeshalom","Hegyfalu","Hejce-Vilmány","Hejőkeresztúr","Hékéd","Helesfa [Cserdi-Helesfa]","Herceghalom","Herend","Herkulesfürdő [Băile Herculane]","Hernád","Hernádnémeti-Bőcs",
            "Hernádszurdok","Hetényegyháza","Hetvehely","Heves","Hevesvezekény","Hévíz [Keszthely]","Hévízgyörk","Hidas-Bonyhád","Hidasnémeti","Himberg","Hmelnickii","Hodász","Hódmezővásárhely","Hódmezővásárhelyi Népkert","Hodonin","Hodoš","Hohenbrugg A.D.Raab","Homok","Homonna [Humenne]","Hort-Csány",
            "Hortobágy","Hortobágyi Halastó","Horvátnádalja","Hosszúberek-Péteri","Hosszúhát","Hőgyész [Szakály-Hőgyész]","Hranice Na Morave","Hrastnik","Hrpelje-Kozina","Huedin","Hugyag","Humenne","Idomeni","Iernut","Igló [Spišská Nová Ves]","Iklad-Domony","Iklad-Domony felső","Iklódbördőce","Ikrény","iksh [ISKARSKO SHOSE]",
            "Ilava","Ilba","Ileanda","Ilia","Iliyantsi","ILIYANTSI [Iliyantsi]","Illava [Ilava]","Iloba [Ilba]","Imremajor","Imst-Pitztal","Ináncs","Inárcs-Kakucs","India [Indjija]","Indjija","INNSBRUCK*","Innsbruck Hbf.","Ipartelepek [Polgárdi-Ipartelepek]","Ipolyszög","Ipolytarnóc","Ipolyvece",
            "Isaszeg","Iskar","ISKAR [Iskar]","ISKARSKO SHOSE","Istambul [Istanbul City]","Istanbul","Istanbul City","Istvántelek","Isztanbul [Istanbul City]","Ivacs","Iváncsa","Ivanjkovci","Izvoru Oltului","Jagodina","Ják-Balogunyom","Jákó-Nagybajom","Jánkmajtis","Jánoshalma","Jánosháza","Jánoshida [Jászboldogháza-Jánoshida]",
            "Jánossomorja","Jánosszállás","Jászapáti","Jászárokszállás","Jászberény","Jászboldogháza-Jánoshida","Jászdózsa","Jászfényszaru","Jászkisér","Jászkisér felső","Jászladány","Jászszentlászló","Jelen","Jenbach","Jenbach Zillertalbahn","Jennersdorf","Jibou","Jobbágyi","Jois","Jókút [Kuty]",
            "Josipdol","Jósvafő-Aggtelek","Józsa","József Szanatórium","Justhmajor","K.fő [Kaposfő]","K.füred [Kaposfüred]","K.homok [Kaposhomok]","K.mérő [Kaposmérő]","K.pula [Kapospula]","K.szekcső [Kaposszekcső]","K.szentjakab [Kaposszentjakab]","K.tüskevár [Kapostüskevár]","K.újlak [Kaposújlak]","K.vár [Kaposvár]","Kaba","Kákics","Kakucs [Inárcs-Kakucs]","Kál-Kápolna","Kállósemjén",
            "Kalofer","KALOFER [Kalofer]","Kalotina - Zapad","Kaltenbach-Stumm (Zb)","Kámon","Kanfanar","Kanizsa [Nagykanizsa]","Kapfing","Kapitányság","Kápolna [Kál-Kápolna]","Kápolnásnyék","Kapoly","Kaposfő","Kaposfüred","Kaposhomok","Kaposmérő","Kapospula","Kapostüskevár","Kaposújlak","Kaposvár",
            "Kaposvári Textilművek [Kapostüskevár]","Kaposvár-Közvágóhíd [Kaposszentjakab]","Kaposszekcső","Kaposszentjakab","Kapronca [Koprivnica]","Káptalanfüred","Kapuvár","Karácsond","Karád","Karakószörcsök","Karánsebes [Caransebeş]","Kárászpuszta [Okorág-Kárászpuszta]","Karcag","Karcag-Vásártér","Kardoskút","Karlovac","Karlovo","KARLOVO [Karlovo]","Karnobat","KARNOBAT [Karnobat]",
            "Károlyháza [Kimle]","Károlyváros [Karlovac]","Karpaty","Karvina Hlavni Nadrazi","Kassa [Kosice]","Kastel Stari","Kastélypark","Katonatelep","Katowice","Kazanlak","KAZANLAK [Kazanlak]","Kazatin-Passajirskii","Kazincbarcika","Kazincbarcika alsó","Kecskéd alsó","Kecskemét","Kecskemét alsó","Kecskemét-Máriaváros","Kék","Kelebia",
            "Kelenföld","Keleti [Budapest-Keleti]","Kemecse","Kemendollár","Kemenesmihályfa","Kemenespálfa","Kenderes","Kendergyár [Nagylaki Kendergyár]","Kenese [Balatonkenese]","Kengyel","Kerekdomb","Kerta","Kertekalja [Vecsés-Kertekalja]","Kertváros (Esztergom) [Esztergom-Kertváros]","Kertváros (Rákospalota) [Rákospalota-Kertváros]","Keszőhidegkút-Gyönk","Keszthely","Kétegyháza","Kétpó","Kettőshalom",
            "Kétútköz","Kiev-Passajirskii","Kijev [Kiev-Passajirskii]","Kimle","Királyegyháza-Rigópuszta","Királyhida [Bruck a.d. Leitha]","Kisapáti [Nemesgulács-Kisapáti]","Kisbárapáti","Kisbér","Kiscsákó","Kiscséripuszta","Kisdobsza","Kisfái","Kisfástanya","Kishajmás [Szatina-Kishajmás]","Kiskapus [Copsa Mica]","Kiskorpád","Kisköre","Kisköre-Tiszahíd","Kiskőrös",
            "Kiskundorozsma","Kiskunfélegyháza","Kiskunhalas","Kiskunlacháza","Kiskunmajsa","Kislőd [Városlőd-Kislőd]","Kismarja","Kismaros","Kismárton [Eisenstadt]","Kismarton-Iskola [Eisenstadt Schule]","Kismindszenti út","Kispest","Kissikárló [Cicarlau]","Kistelek","Kisteleki szőlők","Kisterenye","Kisterenye-Bányatelep","Kistótfalu","Kistőke","Kisújszállás",
            "Kisvác","Kisvárda","Kisvárda-Hármasút","Kisvarsány","Kiszombor","Kiszombor megálló","Kisszállás","Kisszekeres","Kisszénás","Kiszucaújhely [Kysucke Nove Mesto]","Kitérőgyár","Kittsee","Klábertelep","KLAGENFURT*","Klagenfurt Hbf","Kledering","Klisura","KLISURA [Klisura]","Klotildliget","Knin",
            "Kocsord","Kocsord alsó","Kolasin","Kolin","Kolozsvár [Cluj Napoca]","Komárom","Komjáti","Komló","Komlósd [Péterhida-Komlósd]","Komoró","Komunari","KOMUNARI [Komunari]","Kondoros [Debrecen-Kondoros]","Kóny","Kónya","Konyár","Konyári Sóstófürdő","Kónyaszék","Koper","Kópháza",
            "Koprivnica","KOPRIVSHTITS [Koprivshtitsa]","Koprivshtitsa","Korlát-Vizsoly","Kórógyszentgyörgy","Korompa [Krompachy]","Kosana","Kosice","Kosjeric","Kostinbrod","KOSTINBROD [Kostinbrod]","Kőbánya alsó","Kőbánya felső","Kőbánya-Kispest","Köki [Kőbánya-Kispest]","Kö-Ki [Kőbánya-Kispest]","Köpcsény [Kittsee]","Körmend","Környe","Körös [Krizevci]",
            "Kőröshegy [Szántód-Kőröshegy]","Körösladány","Kőszeg","Kőszegfalva","Kötegyán","Köveskál [Zánka-Köveskál]","Középmező","Középrigóc","Központi Főmajor [Imremajor]","Krakau [KRAKOW*]","Krakkó [KRAKOW*]","KRAKOW*","Krakow Glowny","Krakow Plaszow","Kralován [Kralovany]","Kralovany","Krivodol","Krizevci","Krompachy","Kronstadt [Brasov]",
            "Kufstein","Kulcs","Kumanovo","Kumaritsa","Kunfehértó","Kungyalu","Kunhegyes","Kunmadaras","Kunszállás","Kunszentmárton","Kunszentmiklós-Tass","Kurd","Kurilo","KURILO [Kurilo]","Kutas","Kútvölgy","Kuty","Külsővat","Kürtös [Curtici]","Kysak",
            "Kysucke Nove Mesto","Lábatlan","Lábdihegy [Badacsonylábdihegy]","Laimbach Regio Museum","Lajkovac","Lajosmizse","Lajosmizse alsó","Lajtakörtvélyes [Pama]","Lakatnik","Lakitelek","Landeck - Zams","Langen Am Arlberg","Lanzendorf-Rannersdf","Lapovo","Lasko","Lassnitzhöhe","Lassnitzthal","Lavochne","Lazarevac","Leányvár",
            "Lébény-Mosonszentmiklós","Leithakáta [Gattendorf]","Lelle [Balatonlelle]","Lelle felső [Balatonlelle felső]","Lemberg [Lvov]","Lengyeltóti","Lenti","Lentiszombathely","Leopoldov","Lepsény","Les Bihor","Leskovac","Levél","Levelek-Magy","Ligettanya","LINEC HL.N. [Linz Hbf]","LINZ/DONAU HBF [Linz Hbf]","LINZ/DONAU MAIN STAT [Linz Hbf]","Linz Hbf","Lipótszentmiklós [Liptovsky Mikulás]",
            "Lipótvár [Leopoldov]","Lipótváralja [Liptovsky Hradok]","Lipovci","Liptovsky Hradok","Liptovsky Mikulás","Litija","Litke","Ljubljana","Ljubljana Tivoli","Ljutomer Mesto","Logatec","Lovcenac","Lozarevo","LOZAREVO [Lozarevo]","Lödersdorf","Lőkösháza","Lőrinci","Lövő","Ludányhalászi","Ludas",
            "Ludus","Ludwigslust","Lugoj","Lugos [Lugoj]","Lukácsháza","Lukácsháza alsó","Lukovo","Lukow","Lupoglav","Lvov","Lyulyakovo","Lyutibrod","Mackovci","Mád","Madéfalva [Siculeni]","Magdolnavölgy","Maglód","Maglódi nyaraló","Mágocs-Alsómocsolád","Magy [Levelek-Magy]",
            "Magyarbánhegyes","Magyarbóly","Magyarcsanád","Magyarhertelend","Magyarkeresztúr-Zsebeháza","Magyarkút","Magyarkút-Verőce","Magyarmecske [Gyöngyfa-Magyarmecske]","Magyarnándor","Magyarszék","Maklár","Makó","Mándok","Mannheim Hbf","Marcaltő","Marchevo","MARCHEVO [Marchevo]","Margecany","Margitfalva [Margecany]","Máriabesnyő",
            "Máriafürdő [Balatonmáriafürdő]","Máriafürdő alsó [Balatonmáriafürdő alsó]","Máriapócs","Máriaradna [Radna]","Máriatölgyes [Dubnica Nad Vahom]","Máriaudvar","Máriaváros [Kecskemét-Máriaváros]","Maribor","Márkó","Márok","Maroshévíz [Toplita]","Marosillye [Ilia]","Marosludas [Ludus]","Marosvásárhely [Târgu Mureş]","Mártély","Martfű","Martonvásár","Mátészalka","Mátraszőlős-Hasznos","Mátraverebély",
            "Mátravidéki Erőmű","Mátyásdomb [Mackovci]","Mayrhofen (Zb)","Máza-Szászvár","Mecsekalja-Cserkút","Mecsekjánosi","Mecsekpölöske","Medgyes [Mediaş]","Medgyesegyháza","Mediaş","Mediesu Aurit","Medkovets","MEDKOVETS [Medkovets]","Megálló [Kiszombor megálló]","Megyer [Rigács]","Meggyespele","Méhkerék","Mélykút","Mende","Ménfőcsanak",
            "Ménfőcsanak felső","Méntelek","Méra","Mernye","Mezdra","MEZDRA [Mezdra]","Mezőberény","Mezőfalva","Mezőhegyes","Mezőkeresztes-Mezőnyárád","Mezőkovácsháza","Mezőkovácsháza felső","Mezőkövesd","Mezőkövesd felső","Mezőlak","Mezőnyárád [Mezőkeresztes-Mezőnyárád]","Mezőpeterd","Mezőtárkány","Mezőtúr","Mezőzombor",
            "Michalovce","Miedzyrzec Podlaski","Miercurea Ciuc","Mihályháza","Miklóshalma [Nickelsdorf]","Miklóstelep","Mindszent","Minsk-Passazirskii","MISKOLC*","Miskolc-Gömöri","Miskolc-Tiszai","Mladenovac","Mogersdorf","Mohács","Mohora","Mojkovac","Molvány","Monor","Monorierdő","Mónosbél",
            "Mór","Moskau [MOSKVA*]","Moskov [MOSKVA*]","MOSKVA*","Moskva-Belorusskaia","Mosonmagyaróvár","Mosonszentandrás [St. Andrä am Zicksee]","Mosonszentmiklós [Lébény-Mosonszentmiklós]","Mosonszolnok","Mosty U Jablunkova St.Hr.","Moszkva [MOSKVA*]","Mőcsény","Mönchhof-Halbturn [ÖBB]","Mözs [Tolna-Mözs]","Mukachevo","Munkács [Mukachevo]","Murakeresztúr","Muraszombat [Murska Sobota]","Murony","Murska Sobota",
            "MÜNCHEN*","München Hbf.","München Hbf Gl.5-10","München Ost","München-Pasing","Nagyatád","Nagybajom [Jákó-Nagybajom]","Nagybánya [Baia Mare]","Nagybátony","Nagyberény [Som-Nagyberény]","Nagyberki","Nagybiccse [Bytca]","Nagycenk","Nagycsécs","Nagycsere","Nagydobos","Nagydorog","Nagyecsed","Nagyér","Nagyerdő [Hanság-Nagyerdő]",
            "Nagyhát","Nagyigmánd-Bábolna","Nagykálló","Nagykanizsa","Nagykapornak","Nagykarácsony","Nagykarácsony felső","Nagykároly [Carei]","Nagykáta","Nagykereki","Nagykőrös","Nagylak","Nagylaki Kendergyár","Nagylapos","Nagylók","Nagymányok","Nagymaros","Nagymaros-Visegrád","Nagymihály [Michalovce]","Nagynyárád",
            "Nagyoroszi","Nagypeterd","Nagyrákos","Nagyrécse","Nagysimonyi","Nagyszalonta [Salonta]","Nagyszeben [Sibiu]","Nagyszekeres","Nagyszénás","Nagyszentjános","Nagyszombat [Trnava (Slo)]","Nagytétény-Diósd","Nagytétény-Érdliget [Tétényliget]","Nagytőke","Nagyút","Nagyvárad [Oradea]","Napkor","NAPOLI*","Napoli Centrale","Napoli P.Garibaldi",
            "Navsi (Nawsie)","Negotino Vardar","Nemesgörzsöny","Nemesgulács-Kisapáti","Nemeske","Nemeskér-Egyházasfalu","Nemeskeresztúr","Nemeskocs","Neszmély","Neudorf(B. Parndorf)","Neusiedl am See","NEUSIEDL AM SEE BAD [Bad Neusiedl Am See (Roeee)]","Nezsider [Neusiedl am See]","Nezsiderfürdő [Bad Neusiedl Am See (Roeee)]","Nickelsdorf","Nikolaevo","NIKOLAEVO [Nikolaevo]","Niš","Nógrád","Nógrádkövesd",
            "Nógrádszakál","Nova Pazova","Novajidrány","Nove Mesto Nad Vahom","Nové Zámky","Novi Beograd","Novi Sad","Nyárjas","Nyárlőrinc","Nyárlőrinc alsó","Nyárlőrinci szőlők","Nyársapát","Nyékládháza","Nyergesújfalu","Nyergesújfalu felső","Nyírábrány","Nyíradony","Nyírbátor","Nyírbogát","Nyírbogdány",
            "Nyírcsaholy","Nyírcsászári","Nyíregyháza","Nyíregyháza külső","Nyíresszőlőtelep [Vicziántelep]","Nyírgelse","Nyírlak","Nyírmada","Nyírmeggyes","Nyírmihálydi","Nyírtelek","Nyugati [Budapest-Nyugati]","Nyúl","Nyulas [Jois]","Óbög","Óbuda","Ócsa","Odoreu","Ófehértó","Ogulin",
            "Ohat-Pusztakócs","Okány","Ókér [Zmajevo]","Okorág-Kárászpuszta","Ola [Zalaegerszeg-Ola]","Olaszliszka-Tolcsva","Olmütz [Olomouc hl. n.]","Olomouc hl. n.","Onga","Ópályi","Ópázova [Stara Pazova]","Oradea","Oreshets","ORESHETS [Oreshets]","Orlin","Ormosd [Ormoz]","Ormoz","Oros","Orosháza","Orosháza felső",
            "Orosháza-Üveggyár","Orosházi tanyák","Oroszlány","Orsha-Centralnaia","Orşova","Ortaháza","Ostffyasszonyfa","Ostrava Hl. N.","Ostrava-Svinov","Oswiecim","Osztopán","Otrokovice","Óvár [Mosonmagyaróvár]","Ózd","Ózd alsó","Őcsény","Ödenburg [Sopron]","Ököritófülpös","Ölbő-Alsószeleste","Őrbottyán",
            "Öreglak","Őrhalom","Őrihódos [Hodoš]","Őriszentpéter","Örkény","Örményes [Fegyvernek-Örményes]","Őrmező [Strazske]","Őrtilos","Örvényes","Öskü","Őszeszék","Öttevény","Ötvöskónyi","Ötztal","Pacsa [Zalaszentmihály-Pacsa]","Pácsony","Páka [Csömödér-Páka]","Pakod","Palánk [Szekszárd-Palánk]","Palanka",
            "Páli-Vadosfa","Palkonya","Pálmajor","Pama","Pamhagen-Pomogy","Pamuk","Pándorfalu [Parndorf]","Pándorfalu megálló [Parndorf Ort]","Pankasz","Pannonhalma","Pápa","Paracin","Pardubice hl. n.","Park [Városi park]","Párkány [Štúrovo]","Parndorf","Parndorf Ort","Pásztó","Pátroha","Pazin",
            "Pázmáneum","Pécel","Pécs","Pécsbánya-Rendező","Pécs-Külváros","Pécsudvard","Penyige","Perkovic","Perkupa","Pertozsény [Petrosani]","Pesterzsébet","Pestszentimre","Pestszentimre felső","Pestszentlőrinc","Péterhida-Komlósd","Péteri [Hosszúberek-Péteri]","Pétfürdő","Petőfiszállás","Petőfiszállási tanyák","Petőfiváros",
            "Petőháza","Petrosani","Pettend","Piactér [Sajószentpéter-Piactér]","Piešťany","Pilis","Piliscsaba","Piliscsév","Pilisjászfalu","Pilisvörösvár","Pincehely","Pinnye","Pirdop","PIRDOP [Pirdop]","Piroska","Pirot","Pirtó","Pirtói szőlők","Piski [Simeria]","Piszke",
            "Pitvaros","Pivka","Plevnik-Drienove","Ploieşti Vest","Pocsaj-Esztár","Podgorica","Podgorje","Poduyane","POELTEN HBF ST [St.Pölten Hbf]","Pogonyipuszta","Pókaszepetk","Póla [Pula]","Polgárdi","Polgárdi-Ipartelepek","Polgárdi-Tekerespuszta","Poljcane","Pomogy [Pamhagen-Pomogy]","Poprád [Poprad-Tatry]","Poprad-Tatry","Porcsalma-Tyukod",
            "Poroszló","Porpác","Porrogszentkirály","Portelek","Porva-Csesznek","Pósfa","Postojna","Povazska Bystrica","Povelyanovo","POVELYANOVO [Povelyanovo]","Pozega [SZ]","Pozsony [BRATISLAVA*]","Pozsonyligetfalu [Bratislava-Petrzalka]","Pozsony-Szőlőhegy [Bratislava-Vinohrady]","Pozsonyújváros [Bratislava-N. M.]","Pörböly","Pöstyén [Piešťany]","Pötréte","Prag [PRAHA*]","Prága [PRAHA*]",
            "Pragersko","Prague [PRAHA*]","PRAHA*","Praha hl. n.","Praha-Holesovice","Praha-Liben","Predeal","Predeál [Predeal]","Predmier","Prelouc","Prerov","Presevo","Prešov","Priboj","Prijepolje","Prijepolje Teretn","Prolet","Pszczyna","Ptuj","Puchov",
            "Puhó [Puchov]","Pula","Purbach Am Neusiedlersee","Pusztabánréve","Pusztaberény","Pusztakettős","Pusztakócs [Ohat-Pusztakócs]","Pusztamonostor","Pusztapó","Pusztaszabolcs","Pusztaszentistván","Pusztatemplom","Putnok","Püspökhatvan","Püspökladány","Püspökladány-Vásártér","Püspökmolnári","Raaba","Rábacsanak [Egyed-Rábacsanak]","Rábahíd",
            "Rábapatona","Rábapordány","Rábaszentandrás [Szany-Rábaszentandrás]","Rábatamási","Rácalmás","Rádiháza","Radna","Radnót [Iernut]","Raggendorf","Raggendorf Markt","Rajka","Rakamaz","Rakek","Rakitovec","Rákóczitanya","Rákos","Rákoscsaba","Rákoscsaba-Újtelep","Rákoshegy","Rákoskert",
            "Rákosliget","Rákospalota-Kertváros","Rákospalota-Újpest","Rákosrendező","Rakovica","Ramsberg-Hippach (Zb)","Raposka","Ráróspuszta","Rátka","Rátót","Războieni","Rebarkovo","REBARKOVO [Rebarkovo]","Rebrovo","Rédics","Regöly","Rendes [Balatonrendes]","Répáspuszta","Répcelak","Repülőtér [Székesfehérvár-Repülőtér]",
            "Rétszilas","Rétszilas alsó","Révfülöp","Ried Im Zillertal","Rigács","Rigópuszta [Királyegyháza-Rigópuszta]","Rijeka","Ristovac","Rohod [Vaja-Rohod]","Rohr Haltestelle","Rókus [Szeged-Rókus]","Roma Termini","Roma Tiburtina","Romcha","Rosenheim","Rosiori Nord","Rotholz","Rózsahegy [Ružomberok]","Rozsály","Röszke",
            "Rudnaykert","Russe [BG]","RUSSE [Russe [BG]]","Ruttka [Vrutky]","Ružomberok","Sacuieni Bihor","Sáferkút","Sajóecseg","Sajókaza","Sajókeresztúr","Sajónémeti","Sajószentpéter","Sajószentpéter-Piactér","Sajószöged","Salgótarján","Salgótarján külső","SALISBURGO CENTRALE [Salzburg Hbf]","Salköveskút-Vassurány","Salomvár [Zalacséb-Salomvár]","Salonta",
            "SALZBURG*","Salzburg Hbf","Sanislau","Sáp","Sáránd","Sarasdorf","Saratel","Sárbogárd","Sáregres","Sargans","Sárhida","Sarkad","Sarkadi Cukorgyár","Sarkadkeresztúr","Sárkeresztúr [Aba-Sárkeresztúr]","Sárosd","Sároseperjes [Prešov]","Sároskőszeg [Kysak]","Sárospatak","Sárrétudvari",
            "Sárvár","Sásd","Sásony [Winden]","Sátoraljaújhely","Satu Mare","Satu Mare Sud","Savirsin","Sávoly","Schlitters (Zb)","Schützen Am Gebirge","Schützen Haltest.","Sebes Alba","Segesvár [Sighişoara]","Seini","Sellye","Selymes","Selyp","Sentjur","Sepsiszentgyörgy [Sfăntu Gheorghe]","Sérc [Schützen Am Gebirge]",
            "Sérc-Bahnstrasse [Schützen Haltest.]","Seregélyes","Seregélyes-Szőlőhegy","Sfăntu Gheorghe","Sibiu","Sibot","Siculeni","Siedlce","Sighişoara","Silindru","Simeria","Simontornya","Sinaia","SINDEL [Sindel Razpredelitelna]","Sindel Razpredelitelna","Siófok","Siójut","Sióvölgy","Sirató","Skole",
            "Skopje","Slavsko","Sliven","SLIVEN [Sliven]","Smolensk","Sofia","SOFIA [Sofia]","SOFIA [Sofia Sever]","Sofia Sever","Soltszentimre","Soltvadkert","Solymár","Somlóvásárhely","Som-Nagyberény","Somodor","Somogyaszaló","Somogyjád","Somogymeggyes","Somogyszentpál","Somogyszentpál felső",
            "Somogyszob","Somogyudvarhely","Somogyvár","Somoskőújfalu","Sopron","Sopronkövesd","Sopronnémeti [Szil-Sopronnémeti]","Sorkifalud","Soroksár","Soroksári út","Sosnowiec Glowny","Sóstó","Sóstófürdő [Konyári Sóstófürdő]","Sóstóhegy","Spišská Nová Ves","Split","Sratsimir","SRATSIMIR [Sratsimir]","St. Andrä am Zicksee","St.Anton Am Arlberg",
            "ST.POELTEN MAIN STAT [St.Pölten Hbf]","St.Pölten Hbf","ST RSK  HRADEC HL.N. [Graz Hbf]","Stalac","Stara Pazova","Stare Mesto U Uher. Hradiste","Straldja","Strass Im Zillertal (Zb)","Strazske","Strba","Strehaia","Strehaia Halta","Stryi","Studenka","Studenzen-Fladnitz","Štúrovo","Stuttgart Hbf","Subotica","Suchdol Nad Odrou","Sumony",
            "Suncuius","Sutomore","Sülysáp","Sümeg","Sümegi Bazaltbánya","Süttő","Süttő felső","Svaliava","Svoge","SVOGE [Svoge]","Szabadbattyán","Szabadegyháza","Szabadi [Csoma-Szabadi]","Szabadifürdő","Szabadisóstó","Szabadka [Subotica]","Szabadságliget","Szabadságtelep [Debrecen-Szabadságtelep]","Szabadszállás","Szajol",
            "Szakály-Hőgyész","Szalajkavölgy [Szilvásvárad-Szalajkavölgy]","Szalatnak","Szállások [Ceglédi szállások]","Szaloniki [Thessaloniki Gare Centrale]","Szalonna","Szamosújvár [Gherla]","Szanatórium [József Szanatórium]","Szaniszló [Sanislau]","Szántód-Kőröshegy","Szany-Rábaszentandrás","Szár","Szárazd","Szárföld","Szárliget","Szárszó [Balatonszárszó]","Szarvas","Szarvaskő","Szászberek","Szászsebes [Sebes Alba]",
            "Szászvár","Szászvár [Máza-Szászvár]","Szatina-Kishajmás","Szatmárméneti Süd [Satu Mare Sud]","Szatmárnémeti [Satu Mare]","Szatmárudvari [Odoreu]","Szatymaz","Százhalombatta","Szécsény","SZEGED*","Szeged","Szeged-Rókus","Szeghalom","Szegi","Szegilong [Erdőbénye]","Szegvár","Székelyhíd [Sacuieni Bihor]","Székelykocsárd [Războieni]","Székesfehérvár","Székesfehérvár-Repülőtér",
            "Székkutas","Szekszárd","Szekszárd-Palánk","Szelevény","Szélhegy","Szellőhát","Szemeretelep","Szemes [Balatonszemes]","Szendrő","Szendrő felső","Szendrőlád","Szenta","Szentannapuszta","Szentdénes","Szentes","Szentetornya","Szentgál","Szentgotthárd","Szentléránt","Szentlőrinc",
            "Szentmártonkáta","Szepezd [Balatonszepezd]","Szepezdfürdő","Széplak alsó [Balatonszéplak alsó]","Széplak felső [Balatonszéplak felső]","Szerecseny","Szerencs","Szerep","Szeretfalva [Saratel]","Szigetvár","Szigliget [Badacsonytördemic-Szigliget]","Szihalom","Szikra","Szikszó","Szikszó-Vásártér","Szil-Sopronnémeti","Szilvásvárad","Szilvásvárad-Szalajkavölgy","Szín [Jósvafő-Aggtelek]","Szinérváralja [Seini]",
            "Szirmabesenyő","Szkopje [Skopje]","Szob","Szob alsó","Szófia [Sofia]","Szokolya","Szolnok","Szombathely","Szombathely-Szőlős","Szorgalmatos","Sződliget [Sződ-Sződliget]","Sződ-Sződliget","Szőkéd","Szőlőhegy [Seregélyes-Szőlőhegy]","Szőlők (Kisteleki) [Kisteleki szőlők]","Szőlők (Nyárlőrinci) [Nyárlőrinci szőlők]","Szőlők (Pirtói) [Pirtói szőlők]","Szőlők [Szombathely-Szőlős]","Szőlősnyaraló","Szőlőtelep [Szőlősnyaraló]",
            "Szőny","Szőny-Déli","Szőreg","Szörényvár [Drobeta Tr.Severin]","Szurdokpüspöki","Szügy","T.alpár [Tiszaalpár]","T.alpár alsó [Tiszaalpár alsó]","T.alpár felső [Tiszaalpár felső]","T.bezdéd [Tiszabezdéd]","T.eszlár [Tiszaeszlár]","T.földvár [Tiszaföldvár]","T.füred [Tiszafüred]","T.jenő [Tiszajenő alsó]","T.kécske [Tiszakécske]","T.lök [Tiszalök]","T.lúc [Tiszalúc]","T.sas [Tiszasas]","T.szentimre [Tiszaszentimre]","T.szőlős [Tiszaszőlős]",
            "T.tenyő [Tiszatenyő]","T.ug [Tiszaug]","T.újváros [Tiszaújváros]","T.várkony [Tiszavárkony]","T.vasvári [Tiszavasvári]","Tab","Tabanovci","Tabdi","Táborfalva","Tajó","Takern-St.Margareth.","Taksony","Taktaharkány","Taktaszada","Tállya","Tamásipuszta","Tanyák (Csongrádi úti) [Csongrádi úti tanyák]","Tanyák (Orosházi) [Orosházi tanyák]","Tanyák (Petőfiszállási) [Petőfiszállási tanyák]","Tápiógyörgye",
            "Tápiószecső","Tápiószele","Tápiószentmárton","Tapolca","Tar","Tarcal","Targu Jiu","Târgu Mureş","Tarjánpuszta","Tarnaszentmiklós","Tárnok","Tass [Kunszentmiklós-Tass]","Taszár","Tát","Tata","Tatabánya","Tatárvár","Tedej","Téglás","Tekerespuszta [Polgárdi-Tekerespuszta]",
            "Telekgerendás","Temesvár [Timişoara Nord]","Tengelic","Terespol","Ternopol","Tét [Gyömöre-Tét]","Tétényliget","Thessaloniki Gare Centrale","Tiborszállás","Tihany [Balatonfüred]","Timişoara Nord","Tirgu Mures [Târgu Mureş]","Tiszaalpár","Tiszaalpár alsó","Tiszaalpár felső","Tiszabezdéd","Tiszaeszlár","Tiszaföldvár","Tiszafüred","Tiszafüred-Gyártelep",
            "Tiszahíd [Kisköre-Tiszahíd]","Tiszahídfő [Tiszaug-Tiszahídfő]","Tiszai [Miskolc-Tiszai]","Tiszajenő alsó","Tiszajenő-Vezseny","Tiszakécske","Tiszalök","Tiszalúc","Tiszasas","Tiszaszentimre","Tiszaszőlős","Tiszatenyő","Tiszaug","Tiszaug-Tiszahídfő","Tiszaújváros","Tiszavárkony","Tiszavasvári","Tisztavíz","Tócóvölgy","Tófej",
            "Tokaj","Tokod","Tolna-Mözs","Tolnanémedi","Tomaj [Badacsonytomaj]","Tompa","Tompapuszta","Tompsan","TOMPSAN [Tompsan]","Toplita","Topolya [Backa Topola]","Toponár","Tormásliget","Tornanádaska","Tornyospálca","Tószeg","Tótkomlós","Tóvároskert","Tőketerebes [Trebisov]","Törökbálint",
            "Törökszentmiklós","Tőserdő","Trautmannsdf/Leitha","Trbovlje","Trebisov","Trencianska Tepla","Trenčín","Trencsén [Trenčín]","Trencsénhőlak [Trencianska Tepla]","Trinec","Trnava (Slo)","Tserovo","Tulovo","TULOVO [Tulovo]","Tunyogmatolcs","Tunyogmatolcs alsó","Tura","Tusnádfürdő [Băile Tuşnad]","Tuzsér","Türje",
            "Tüskevár","Tvarditsa","TVARDITSA [Tvarditsa]","Tychy","Tyukod [Porcsalma-Tyukod]","Ucea","Ucsa [Ucea]","Uderns (Zb)","Udvari [Balatonudvari]","Újbelgrád [Novi Beograd]","Újbög","Újfehértó","Ujgorod","Újkenéz","Újkér","Újpázova [Nova Pazova]","Újpest","Újpest [Rákospalota-Újpest]","Újszász","Újszeged",
            "Újtelep (Rákoscsaba) [Rákoscsaba-Újtelep]","Újudvar","Újváros","Újvenyim","Újverbász [Vrbas]","Újvidék [Novi Sad]","Ukk","Ulm Hbf","Ulmeni Salaj","Ungvár [Ujgorod]","Úrihegy","Usti Nad Labem hl. n.","Uzhgorod [Ujgorod]","Uzice (Sb)","Uzsa","Uzsabánya alsó","Üdülőtelep [Csittényhegy]","Üllő","Üröm","Üveggyár [Orosháza-Üveggyár]",
            "Vác","Vác-Alsóváros","Váchartyán","Vácrátót","Vadna","Vadosfa [Páli-Vadosfa]","Vágbeszterce [Povazska Bystrica]","Vágújhely [Nove Mesto Nad Vahom]","Vaja-Rohod","Vajta","Valea lui Mihai","Valjevo","Vámoscsalád","Vámosgyörk","Vámospércs","Vámosújfalu [Olaszliszka-Tolcsva]","Váralja","Varannó [Vranov Nad Toplou]","Várda","Várhegyalja",
            "Varna","VARNA [Varna]","Várna [Varna]","Városerdő [Gyulai városerdő]","Városföld","Városi park","Városlőd","Városlőd-Kislőd","Várpalota","Varsó [WARSZAWA*]","Vásárosdombó","Vásárosnamény","Vásárosnamény külső","Vásártér (Derecske) [Derecske-Vásártér]","Vásártér (Hajdúnánás) [Hajdúnánás-Vásártér]","Vásártér (Püspökladány) [Püspökladány-Vásártér]","Vásártér (Szikszó) [Szikszó-Vásártér]","Vasboldogasszony [Egervár-Vasboldogasszony]","Vasegerszeg","Vassúrány [Salköveskút-Vassurány]",
            "Vasútmúzeum","Vasvár","Vaszar","Vecsés","Vecsés-Kertekalja","Védeny [Weiden am See]","Vedrovo","Végegyháza","Végegyháza alsó","VELENCE* [VENEZIA*]","Velence","Velence Mestre [Venezia Mestre]","Velence S.L. [Venezia S. Lucia]","Velencefürdő","Veles","Velika Plana","VENEZIA*","Venezia Mestre","Venezia S. Lucia","Vép",
            "Verbász [Vrbas]","Veresegyház","Verőce","Vértesszőlős","Veszkény","Veszprém","Veszprémvarsány","Vésztő","Vezseny [Tiszajenő-Vezseny]","Viazma","Vica","Vicziántelep","Vidbol","VIDBOL [Vidbol]","Videle","VIDEN HL.N. [Wien Hbf]","Vidin","VIDIN [Vidin]","VIEDEN HL.ST. [Wien Hbf]","Vienna [WIEN*]",
            "VIENNA CENTRALE [Wien Hbf]","VIENNA MAIN STATION [Wien Hbf]","Vienna Mainstation [Wien Hbf]","VIENNE GARE CENTRALE [Wien Hbf]","Világos [Balatonvilágos]","Világoshegy","VILLACCO CENTRALE [Villach Hbf]","VILLACH*","Villach Hbf","Villach Westbf","Villány","Villánykövesd","Vilmány [Hejce-Vilmány]","Vilmaszállás","Vinár","Vinnica","Vintu De Jos","Vinye","Virányos","Visegrád [Nagymaros-Visegrád]",
            "Vitka","Vitnyéd-Csermajor","Vízmű [Bánrévei Vízmű]","Vízvár","Vizsoly [Korlát-Vizsoly]","VL.TRICHKOV [Vlado Trichkov]","Vladimir Pavlov","Vlado Trichkov","Vokány","Volovec","Vonyarcvashegy","Vörösvárbánya","Vörs","VRACA [Vratsa]","Vranje","Vranov Nad Toplou","Vratsa","Vrbas","Vrbnica","Vrbovec",
            "Vrutky","Vulkapordány [Wulkaprodersdorf (Roeee)]","Vulkapordány megálló [Wulkaprodersdf Hst]","WALLERN/NEUS [Wallern Am Neusiedlersee]","Wallern Am Neusiedlersee","Warschau [WARSZAWA*]","WARSZAWA*","Warszawa Centralna","Warszawa Wschodnia","Warszawa Zachodnia","Weichselbaum A.D.R.","Weiden am See","WENEN [WIEN*]","WIEN*","Wien Grillgasse","Wien Hauptbahnhof [Wien Hbf]","Wien Hbf","Wien Meidling","Wien Westbf","Wilfleinsdorf",
            "Winden","Wittenberge","Wörgl","Wulkaprodersdf Hst","Wulkaprodersdorf (Roeee)","Z.egerszeg [Zalaegerszeg]","Z.gyömörő [Zalagyömörő]","Z.komár [Zalakomár]","Z.lövő [Zalalövő]","Z.patakalja [Zalapatakalja]","Z.szentgyörgy [Zalaszentgyörgy]","Z.szentiván [Zalaszentiván]","Z.zentjakab [Zalaszentjakab]","Z.zentlőrinc [Zalaszentlőrinc]","Zabreh Na Morave","Zagorje","Zágráb [Zagreb Glavni Kol.]","Zagreb Glavni Kol.","Zagyvapálfalva","Zagyvarékas",
            "Zagyvaszántó [Apc-Zagyvaszántó]","Záhony","Zajta","Zákány","Zalabér-Batyk","Zalacséb-Salomvár","Zalaegerszeg","Zalaegerszeg-Ola","Zalagyömörő","Zalakomár","Zalalövő","Zalapatakalja","Zalaszentgyörgy","Zalaszentiván","Zalaszentjakab","Zalaszentlőrinc","Zalaszentmihály-Pacsa","Zamárdi","Zamárdi felső","Zánkafürdő",
            "Zánka-Köveskál","Zavet","Zawiercie","Zebegény","Zebrzydowice","Zelemér","Zell Am Ziller (Zb)","Zichyújfalu","Zidani Most","Žilina","Zirc","Zlatitsa","ZLATITSA [Zlatitsa]","Zmajevo","Zólyom [ZVOLEN*]","Zrínyitelep","Zugló","Zurány [Zurndorf]","Zurndorf","Zürich HB",
            "Zverino","ZVOLEN*","Zvolen Osobna Stanica","Zsebeháza [Magyarkeresztúr-Zsebeháza]","Zsilvásárhely [Targu Jiu]","Zsolna [Žilina]","Zsujta"
        };

        /// <summary>
        /// Returns stations that MÁV has but were not found. Heuristic to check your extraction
        /// </summary>
        /// <param name="stationsFound">Stations to take the diff with</param>
        /// <returns>Stations that MÁV has but were not found</returns>
        public static List<string> MAVStationDiff(List<string> stationsFound)
        {
            List<string> remaining = new List<string>(AllStationNames);

            remaining.RemoveAll(s => s.Contains('[')); //Only things inside the brackets are important and they are included on their own as well

            foreach (string station in stationsFound)
            {
                remaining.RemoveAll(s => StationCompare(s, station));
            }

            return remaining;
        }

        /// <summary>
        /// Normalizes a station name (removes Hungarian accents, replaces hyphens with spaces, removes redundant information such as station)
        /// </summary>
        /// <param name="stationName">Name to normalize</param>
        /// <returns>Normalized version of the same name</returns>
        private static string stationNormalizeName(string stationName)
        {
            stationName = stationName.ToLower();

            stationName = stationName.Replace('á', 'a');
            stationName = stationName.Replace('é', 'e');
            stationName = stationName.Replace('í', 'i');
            stationName = stationName.Replace('ó', 'o');
            stationName = stationName.Replace('ö', 'o');
            stationName = stationName.Replace('ő', 'o');
            stationName = stationName.Replace('ú', 'u');
            stationName = stationName.Replace('ü', 'u');
            stationName = stationName.Replace('ű', 'u');

            stationName = stationName.Replace(" railway station crossing", "");
            stationName = stationName.Replace(" railway station", "");
            stationName = stationName.Replace(" train station", "");
            stationName = stationName.Replace(" station", "");
            stationName = stationName.Replace(" vonatallomas", "");
            stationName = stationName.Replace(" vasutallomas", "");
            stationName = stationName.Replace(" mav pu", "");
            stationName = stationName.Replace("-", " ");

            return stationName;
        }

        /// <summary>
        /// Compares a MÁV station name with a Google station name and tells if they are close enough
        /// </summary>
        /// <param name="mavStation">MÁV's variant of the station name</param>
        /// <param name="databaseStation">Station name stored in your database</param>
        /// <returns>Whether they are similar enough</returns>
        public static bool StationCompare(string mavStation, string databaseStation)
        {
            if (databaseStation == "Csengöd" && mavStation.Contains("Csengőd"))
            {
                int i = 0;
            }
            string mavAltName = stationNormalizeName(Regex.Match(mavStation, @"\[(?<altname>.*?)\]").Groups["altname"].Value);
            mavStation = stationNormalizeName(mavStation);
            databaseStation = stationNormalizeName(databaseStation);

            if (mavStation == databaseStation) return true;
            if (mavAltName == databaseStation) return true;

            return false;
        }

        /// <summary>
        /// Requests MAV with a JSON represented by a JObject
        /// </summary>
        /// <param name="requestObject">Object with requested data</param>
        /// <returns>A JObject of the response</returns>
        public static JObject RequestMAV(JObject requestObject)
        {
            HttpWebRequest request = WebRequest.CreateHttp("http://vonatinfo.mav-start.hu/map.aspx/getData");
            byte[] payload = Encoding.UTF8.GetBytes(requestObject.ToString(Formatting.None));

            request.Accept = "application/json, text/javascript, */*; q=0.01";
            request.Headers["Accept-Encoding"] = "gzip, deflate";
            request.Headers["Accept-Language"] = "hu-HU,hu;q=0.9,en-US;q=0.8,en;q=0.7";
            request.ContentLength = payload.Length;
            request.ContentType = "application/json; charset=UTF-8";
            request.Host = "vonatinfo.mav-start.hu";
            request.Headers["Origin"] = "http://vonatinfo.mav-start.hu/";
            request.Referer = "http://vonatinfo.mav-start.hu/";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36";
            request.Headers["X-Requested-With"] = "XMLHttpRequest";
            request.Method = "POST";

            try
            {
                request.GetRequestStream().Write(payload, 0, payload.Length);
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress), Encoding.UTF8))
                    {
                        return JObject.Parse(reader.ReadToEnd());
                    }
                }
            }
            catch (WebException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
                return null;
            }
        }

        public static Train GetTrain(string elviraID)
        {
            try
            {
                JObject trainRequest = new JObject();
                trainRequest["a"] = "TRAIN";
                trainRequest["jo"] = new JObject();
                trainRequest["jo"]["v"] = elviraID;
                JObject trainResponse = RequestMAV(trainRequest);
                if (trainResponse == null) return null;
                return new Train(elviraID, trainResponse);
            }
            catch (MAVAPIException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
                return null;
            }
        }
    }
}
