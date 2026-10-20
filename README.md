# Gains Defender

![Cover di Gains Defender](Gains-Defender-cover.png)

**Un prototipo arcade di difesa in Unity: proteggi i gains eliminando con un clic o un tocco gli scheletri che convergono verso il centro.**

Gains Defender combina uno sfondo fotografico domestico, con contenitori di integratori su un tavolo, e nemici tridimensionali. L'obiettivo è immediato: intercettare i nemici prima che raggiungano il punto da difendere, accumulando punti finché si riesce a resistere. Un solo nemico arrivato a destinazione termina la partita.

Il progetto contiene una singola scena, quattro script di gameplay e quattro prefab principali. La struttura concentra il lavoro su input, generazione dei nemici, movimento, punteggio e interfaccia, mantenendo il ciclo di gioco facilmente leggibile e modificabile.

> Questo documento deriva dall'analisi di script, scena, prefab, modelli e impostazioni presenti nella cartella. Le ragioni tecniche illustrate sono interpretazioni delle scelte visibili nei file, non dichiarazioni dell'autore. Non sono state eseguite una build o una sessione di gioco. La cover è un'illustrazione promozionale generata con AI, non uno screenshot del prototipo.

## Indice

- [Esperienza di gioco](#esperienza-di-gioco)
- [Comandi e selezione dei nemici](#comandi-e-selezione-dei-nemici)
- [Spawn, movimento e bilanciamento](#spawn-movimento-e-bilanciamento)
- [Punteggio e interfaccia](#punteggio-e-interfaccia)
- [Direzione visiva e rendering](#direzione-visiva-e-rendering)
- [Architettura](#architettura)
- [Scelte progettuali e compromessi](#scelte-progettuali-e-compromessi)
- [Struttura della cartella](#struttura-della-cartella)
- [Tecnologie e dipendenze](#tecnologie-e-dipendenze)
- [Apertura, avvio e build](#apertura-avvio-e-build)
- [Parametri modificabili](#parametri-modificabili)
- [Stato attuale e limiti](#stato-attuale-e-limiti)
- [Possibili sviluppi](#possibili-sviluppi)
- [Asset, crediti e licenza](#asset-crediti-e-licenza)

## Esperienza di gioco

All'avvio, `GameController` prepara il menu mostrando titolo, pannello e pulsante **Start**. Le istruzioni e il punteggio sono inizialmente nascosti; lo spawner rimane inattivo.

Premendo Start:

1. La partita viene marcata come attiva e il punteggio torna a zero.
2. Titolo, pulsante e pannello del menu vengono nascosti.
3. Compaiono istruzioni e punteggio; le istruzioni scompaiono dopo 5 secondi.
4. Lo spawner programma il primo nemico dopo 2 secondi e continua a generarne uno ogni 2 secondi.
5. Ogni scheletro si muove in linea retta verso l'origine del mondo.
6. Un clic o un tocco valido elimina il nemico e assegna 10 punti.
7. Se un nemico arriva a meno di 0,1 unità dall'origine, viene attivato il game over.

Il game over interrompe le nuove generazioni, nasconde il contatore di partita e mostra il punteggio finale nel titolo, insieme al pulsante Start. Non sono presenti un traguardo di vittoria, livelli o ondate numerate: il ciclo è potenzialmente continuo, con difficoltà impostata da parametri costanti.

Il pulsante Start può richiamare una nuova sequenza, ma **il riavvio non ripulisce i nemici della partita precedente**. Per una ripartenza completamente pulita, nello stato attuale è opportuno interrompere e riattivare Play nell'Editor. Il dettaglio è descritto nella sezione sui limiti.

## Comandi e selezione dei nemici

| Dispositivo | Azione | Risultato |
| --- | --- | --- |
| Mouse | Clic sinistro sul nemico | Elimina il nemico colpito dal raycast |
| Touchscreen | Pressione iniziale del tocco principale sul nemico | Esegue la stessa selezione del mouse |
| Interfaccia | Pulsante Start | Avvia la sequenza di partita |

`InputController` usa direttamente `Mouse.current` e `Touchscreen.current` del nuovo **Unity Input System**. L'azione viene riconosciuta con `wasPressedThisFrame`: tenere premuto non produce un attacco continuo e il trascinamento non è una meccanica di gioco. Il codice gestisce il tocco principale, non una selezione multitouch indipendente.

La posizione sullo schermo viene convertita in un raggio con `Camera.ScreenPointToRay`. `Physics.Raycast` ricerca un collider entro **100 unità**, filtrando il layer **Enemy**. Nella scena la maschera vale `64`, corrispondente al layer 6.

Se il collider colpito possiede `EnemyController` sullo stesso GameObject, viene chiamato `OnDefeated()`: il controller prova ad aggiungere i punti e distrugge l'oggetto. Una pressione a vuoto non comporta penalità. Non sono implementati proiettili, salute del nemico, attacchi multipli o tempi di ricarica.

La camera viene recuperata in `Start()` tramite `Camera.main`: deve quindi esistere una camera attiva con tag `MainCamera`. Il riferimento serializzato viene sostituito da questa ricerca all'inizializzazione.

## Spawn, movimento e bilanciamento

### Generazione sul perimetro

`EnemySpawner` sceglie un angolo casuale fra 0 e 2π e calcola la posizione:

```text
x = cos(angolo) × raggio
y = 0
z = sin(angolo) × raggio
```

Con il raggio predefinito di **12 unità**, i nemici nascono sulla circonferenza nel piano XZ. L'intero perimetro può produrre minacce; non esistono corsie o punti di ingresso fissi. L'angolo è casuale, mentre distanza iniziale, velocità e intervallo sono costanti.

La circonferenza è centrata sull'origine del mondo. Lo spawner **non somma la propria posizione** al punto calcolato: spostarne il Transform nell'Editor non sposta l'arena. Questo è rilevante perché i Transform di alcuni oggetti di gestione sono salvati a coordinate molto lontane dal centro, senza influenzare queste formule.

`StartSpawning()` imposta il primo appuntamento a `Time.time + fSpawnInterval`; `StopSpawning()` disattiva le nuove generazioni. Il controllo avviene in `Update()`, senza coroutine dedicate allo spawn. Dopo ogni generazione viene fissato il successivo appuntamento a partire dal tempo corrente: non c'è recupero di più spawn nello stesso frame in caso di rallentamento.

### Avvicinamento al centro

Ogni `EnemyController` imposta il bersaglio a `Vector3.zero` e usa `Vector3.MoveTowards` con velocità **3 unità al secondo**, moltiplicata per `Time.deltaTime`. In questo modo lo spostamento dipende dal tempo trascorso e non direttamente dal numero di frame.

Il tempo teorico di arrivo è di circa **4 secondi** dal momento dello spawn, con una soglia di sconfitta leggermente precedente al centro. In assenza di input, il primo game over è quindi atteso intorno ai 6 secondi dall'avvio della sequenza: circa 2 secondi di attesa e 4 di tragitto. È una stima ricavata dai parametri, non una misura eseguita nell'Editor.

Non vengono impiegati NavMesh, ricerca del percorso, forze o evitamento degli ostacoli. Gli scheletri possono sovrapporsi e non sono orientati automaticamente verso il bersaglio: la loro istanza nasce con `Quaternion.identity` e il codice aggiorna solo la posizione.

## Punteggio e interfaccia

`GameController` conserva il punteggio in memoria e accetta incrementi solo quando la partita è attiva e non è già terminata. Il prefab Enemy assegna **10 punti per eliminazione**. Non ci sono moltiplicatori, combo, record persistenti o classifiche.

Il prefab `UI` usa **uGUI** con componenti `UnityEngine.UI.Text`, `Button`, `Image` e `CanvasScaler`. I testi sono quelli del prototipo:

- Titolo: `GAINS DEFENDER`.
- Pulsante: `Start`.
- Istruzione: `Defend the gains from the enemy by clicking them`.
- Contatore: `Score: …`.
- Fine partita: `GAME OVER`, seguito dal punteggio.

Il Canvas dell'interfaccia è in modalità **Screen Space - Overlay**, con scala relativa a una risoluzione di riferimento **1536 × 2040** e bilanciamento larghezza/altezza pari a 0,5. La composizione di partenza è quindi verticale; la cover orizzontale è un materiale di presentazione distinto dal layout del gioco. La resa dell'interfaccia su rapporti diversi va verificata nell'Editor.

Il pulsante è collegato a `GameController.StartGameSequence()` attraverso un evento serializzato. I riferimenti al controller e ai singoli elementi UI vengono risolti nelle istanze della scena: il solo prefab `GameManager` non contiene tutti i collegamenti necessari. L'`EventSystem` usa `InputSystemUIInputModule`.

La scomparsa delle istruzioni usa una coroutine con `WaitForSeconds`. Sono presenti messaggi di debug per intercettazione di un nemico e game over; non risultano effetti sonori o feedback animati specifici associati a queste azioni.

## Direzione visiva e rendering

L'elemento distintivo è l'accostamento fra **fotografia reale e oggetti 3D**. La fotografia `PXL_20260709_094909214.jpg`, di 6144 × 8160 pixel, mostra due contenitori di integratori impilati su un tavolo scuro, con arredi blu sullo sfondo. Il tema dei “gains” viene così comunicato tramite gli oggetti da proteggere.

Nella scena la fotografia è assegnata a una `RawImage` dentro un Canvas **Screen Space - Camera**, collegato alla camera principale, con distanza del piano pari a 100. La `RawImage` è estesa al Canvas. Il fondale è dunque un'immagine visualizzata dalla UI; non sono presenti acquisizione video dal vivo, tracking di superfici o integrazione AR Foundation. La prospettiva della camera 3D e la disposizione dei nemici servono a costruire visivamente la sovrapposizione.

La `Custom Camera` è prospettica, con inclinazione elevata rispetto al piano di gioco e tag `MainCamera`. La cartella contiene:

- `skeleton_model_110.fbx`, usato come figlio del prefab Enemy;
- `mat_skeleton.mat`, materiale chiaro con rimappatura nell'importatore del modello;
- `Mesh_afterpolish.fbx`, istanziato nella scena, con un componente disabilitato tramite override;
- la fotografia di sfondo.

Il prefab Enemy contiene un `BoxCollider` sulla radice, insieme al relativo script, ed è sul layer Enemy. La scala della radice è **3,5** sui tre assi. Separare il modello visivo dal collider e dalla logica facilita la sostituzione della mesh mantenendo l'interazione.

Il rendering usa **Universal Render Pipeline**. Sono presenti asset separati PC e Mobile: il primo ha render scale **1**, il secondo **0,8**. I livelli di qualità assegnano impostazioni differenti alle piattaforme. Questa predisposizione consente un compromesso fra qualità e costo grafico, ma non costituisce una verifica delle prestazioni su dispositivo. Il post-processing della camera principale è disattivato, anche se la cartella Settings contiene profili volume del progetto.

## Architettura

La logica di gameplay è suddivisa in quattro `MonoBehaviour`:

| Componente | Responsabilità | Collaborazioni |
| --- | --- | --- |
| `GameController` | Stato della partita, punteggio, schermate, istruzioni temporanee | Riferimenti UI e `EnemySpawner` |
| `EnemySpawner` | Abilitazione dello spawn, timer, posizione casuale, istanziazione | Prefab Enemy |
| `EnemyController` | Bersaglio, movimento, eliminazione, richiesta di game over | Cerca il `GameController` in `Start()` |
| `InputController` | Lettura mouse/touch e raycast sui nemici | `Camera.main` e `EnemyController` sul collider |

Il flusso principale è:

```text
Start → GameController → EnemySpawner → istanza Enemy
Mouse/touch → InputController → EnemyController.OnDefeated()
                                  → GameController.AddScore()
Nemico al centro → GameController.TriggerGameOver()
                 → StopSpawning() + schermata finale
```

Lo stato è rappresentato da due booleani privati, `bIsGameActive` e `bIsGameOver`, anziché da una macchina a stati esplicita. L'inizializzazione è raccolta in metodi `Init…()` richiamati da `Start()`. I campi configurabili usano prevalentemente `[SerializeField]`; il riferimento `eSpawner` è pubblico. I nomi delle variabili usano prefissi di tipo, per esempio `f` per float, `i` per interi e `g` per GameObject.

Non sono presenti singleton persistenti, salvataggi, servizi online, dependency injection o un sistema di eventi applicativo. Le chiamate dirette mantengono il prototipo contenuto, ma rendono importante la correttezza dei collegamenti nell'Inspector.

## Scelte progettuali e compromessi

Le motivazioni seguenti spiegano gli effetti delle soluzioni adottate, senza attribuire all'autore intenzioni non documentate.

| Scelta visibile nel progetto | Vantaggio tecnico o di design | Compromesso |
| --- | --- | --- |
| Un solo gesto per eliminare un nemico | Regola accessibile e condivisa fra mouse e touch | Profondità affidata soprattutto a rapidità e precisione |
| Nemici da un perimetro circolare verso un unico centro | Pressione da direzioni variabili e percorso breve da implementare | Arena vincolata all'origine; assenza di ostacoli e percorsi alternativi |
| Sconfitta al primo arrivo | Condizione di fine partita immediata e leggibile | Nessun margine d'errore o sistema di recupero |
| Raycast fisico filtrato per layer | Selezione coerente con camera e collider 3D | Il collider deve avere dimensioni adatte e lo script deve stare sull'oggetto colpito |
| `MoveTowards` con `deltaTime` | Movimento lineare semplice e dipendente dal tempo | Nessuna navigazione o reazione agli altri nemici |
| Quattro componenti e prefab distinti | Responsabilità comprensibili e parametri modificabili nell'Editor | Collegamenti manuali e riferimenti alla scena |
| Fotografia come sfondo | Identità immediata con pochi asset ambientali | Prospettiva, proporzioni e occlusioni devono essere curate separatamente |
| Parametri serializzati | Bilanciamento rapido senza modificare gli script | Nessuna progressione automatica durante la partita |
| `Instantiate` e `Destroy` | Ciclo di vita diretto per un prototipo con pochi oggetti | Aumentando il carico, un pool potrebbe ridurre allocazioni e lavoro di creazione |
| Ricerca del controller per ogni nemico | Collegamento automatico senza passarlo allo spawn | Dipendenza implicita dalla presenza di un controller nella scena |
| uGUI con Text legacy | Interfaccia essenziale e modifica rapida | Gestione di layout e testi da consolidare su più risoluzioni |

## Struttura della cartella

Il repository esterno contiene una seconda cartella omonima, che è la **radice del progetto Unity**:

```text
Gains_Defender/                     # repository
├── README.md                      # documentazione del progetto
├── Gains-Defender-cover.png        # cover promozionale
├── .gitattributes
├── .gitignore
└── Gains_Defender/                 # cartella da aprire con Unity Hub
    ├── Assets/
    │   ├── Scenes/SampleScene.unity
    │   ├── Scripts/
    │   │   ├── GameController.cs
    │   │   ├── EnemySpawner.cs
    │   │   ├── EnemyController.cs
    │   │   └── InputController.cs
    │   ├── Prefabs/
    │   │   ├── GameManager.prefab
    │   │   ├── EnemySpawner.prefab
    │   │   ├── Enemy.prefab
    │   │   └── UI.prefab
    │   ├── Mesh/                  # modelli, materiale e fotografia
    │   ├── Settings/              # renderer URP e profili volume
    │   ├── InputSystem_Actions.inputactions
    │   ├── Readme.asset
    │   └── TutorialInfo/          # documentazione iniziale del template
    ├── Packages/
    │   ├── manifest.json
    │   └── packages-lock.json
    ├── ProjectSettings/
    └── Gains_Defender.slnx
```

`Readme.asset` e gli script sotto `TutorialInfo` appartengono all'infrastruttura informativa del template Unity; non aggiungono regole al gameplay. I file `.meta` accompagnano gli asset e ne conservano le identità: vanno mantenuti insieme ai rispettivi file. Il `.gitignore` esclude cache Unity, cartelle temporanee, log, build e file generati dagli ambienti di sviluppo.

## Tecnologie e dipendenze

| Tecnologia | Versione presente nei file | Ruolo |
| --- | --- | --- |
| Unity Editor | **6000.3.5f2** | Editor del progetto |
| C# | Versione gestita da Unity | Script di gameplay |
| Universal RP | **17.3.0** | Rendering |
| Input System | **1.17.0** | Mouse, touchscreen e input UI |
| uGUI | **2.0.0** | Canvas, testi e pulsanti |
| Test Framework | **1.6.0** | Pacchetto installato; nessuna suite dedicata trovata |

Il manifest include anche AI Navigation 2.0.9, Timeline 1.8.10, Visual Scripting 1.9.9 e Multiplayer Center 1.0.1, oltre agli strumenti per IDE e ai moduli Unity. La loro presenza fra le dipendenze **non implica funzionalità di navigazione, timeline, scripting visuale o multiplayer**: i quattro script implementano il gameplay direttamente in C#.

L'asset `InputSystem_Actions.inputactions` conserva mappe e azioni generiche, incluse Move, Look e Jump. Questi nomi non indicano comandi di gameplay implementati: `InputController` interroga direttamente i dispositivi e non usa quelle azioni per muovere un personaggio.

## Apertura, avvio e build

1. Installare tramite Unity Hub l'Editor **6000.3.5f2**, preferibilmente la stessa versione registrata in `ProjectSettings/ProjectVersion.txt`.
2. Aggiungere il progetto selezionando la cartella **interna** `Gains_Defender`, quella che contiene `Assets`, `Packages` e `ProjectSettings`.
3. Attendere l'importazione degli asset e la risoluzione dei pacchetti.
4. Aprire `Assets/Scenes/SampleScene.unity`.
5. Attivare Play, premere Start e cliccare sui nemici.

La scena è già abilitata in `EditorBuildSettings.asset`. Per una build, selezionare la piattaforma nei Build Profiles, installare l'eventuale modulo richiesto e includere `SampleScene`. Non sono presenti eseguibili pronti nella cartella analizzata.

Il progetto usa il nuovo Input System (`activeInputHandler: 1`). La configurazione Android comprende backend IL2CPP, architettura ARM64 e SDK minimo 25; restano metadati del template, fra cui `DefaultCompany` e l'identificativo dell'applicazione. Questi sono dati di configurazione, non una certificazione di funzionamento mobile o di pubblicazione su uno store.

### Collegamenti da controllare in caso di problemi

- **Start non avvia nulla:** verificare l'evento OnClick e il suo riferimento al `GameController` dell'istanza di scena. Nel prefab UI isolato il target non è assegnato.
- **Non compaiono nemici:** controllare `eSpawner` e il prefab `gEnemy`; il primo spawn arriva dopo l'intervallo configurato.
- **I nemici non rispondono al clic:** verificare camera con tag `MainCamera`, layer Enemy, collider attivo, maschera dell'input e presenza dello script sullo stesso oggetto del collider.
- **Mancano titolo o contatore:** controllare i riferimenti UI assegnati nel GameManager della scena.
- **L'interfaccia appare tagliata:** provare il rapporto verticale di riferimento e verificare ancoraggi, dimensioni e scala dei testi.

## Parametri modificabili

I valori seguenti provengono dagli script e dai prefab, con i collegamenti specifici completati dalla scena.

| Componente | Campo | Valore | Effetto |
| --- | --- | --- | --- |
| `EnemySpawner` | `fSpawnInterval` | 2 s | Ritardo iniziale e intervallo fra nemici |
| `EnemySpawner` | `fSpawnRadius` | 12 | Distanza del perimetro dall'origine |
| `EnemySpawner` | `gEnemy` | Prefab Enemy | Oggetto istanziato |
| `EnemyController` | `fSpeed` | 3 | Velocità in unità al secondo |
| `EnemyController` | `iPointsValue` | 10 | Punti per eliminazione |
| `InputController` | `lEnemyLayer` | Enemy, layer 6 | Collider selezionabili |
| `InputController` | `fRayDistance` | 100 | Portata del raycast |
| `GameController` | `fInstructionDuration` | 5 s | Durata delle istruzioni |
| Prefab Enemy | Scala radice | 3,5 | Dimensioni del modello e del collider ereditate dal Transform |
| UI | Risoluzione di riferimento | 1536 × 2040 | Base di adattamento del layout |

La soglia di arrivo **0,1**, la posizione bersaglio **(0, 0, 0)** e il reset del punteggio a **0** sono definiti nel codice. Modificare il raggio o la velocità cambia il tempo disponibile per reagire; ridurre l'intervallo aumenta la frequenza delle minacce. Questi effetti possono essere regolati separatamente senza cambiare l'architettura.

## Stato attuale e limiti

Il materiale presente descrive un **prototipo locale per un giocatore**, con ciclo base implementato e margini di consolidamento.

### Gestione della fine partita e del riavvio

`TriggerGameOver()` ferma solo lo spawner. I nemici già presenti continuano ad aggiornarsi e ad avvicinarsi al centro. Il controller impedisce ulteriori incrementi di punteggio e gestisce una sola transizione a game over, ma non sospende il mondo.

`StartGameSequence()` azzera il punteggio e riabilita lo spawn senza distruggere i nemici rimasti, ricaricare la scena o ripristinare una collezione di oggetti. Un nemico della partita precedente può quindi raggiungere il centro e chiudere la successiva. Il pulsante di ripartenza esiste, ma il reset completo della sessione non è ancora implementato.

### Input e stato della partita

`InputController` non verifica se la partita è attiva e non esclude gli input sopra la UI tramite `EventSystem`. Può quindi elaborare raycast anche durante menu e game over. `AddScore()` protegge il punteggio, ma `OnDefeated()` distrugge comunque il nemico selezionato. La UI non costituisce automaticamente un blocco per questo raycast fisico.

### Istruzioni e coroutine

La coroutine delle istruzioni viene salvata in un campo, ma non viene fermata esplicitamente in caso di game over o nuova sequenza. In un riavvio rapido, una coroutine precedente può nascondere le istruzioni della nuova partita. Il game over non le nasconde immediatamente: possono rimanere visibili fino alla scadenza.

### Presentazione e contenuti

Non risultano implementati audio dedicato, animazioni di attacco o morte controllate dagli script, particelle, pausa, menu impostazioni, difficoltà crescente, nemici differenziati, salvataggio del record o multiplayer. Il movimento dei nemici è una traslazione; la presenza di modelli FBX non dimostra un sistema di animazione attivo.

La fotografia di sfondo non fornisce automaticamente profondità o occlusioni agli oggetti 3D. Non è documentata una calibrazione del rapporto fra immagine, camera e centro logico da difendere. La dimensione del BoxCollider e la sovrapposizione dei nemici possono influire sulla precisione percepita del clic.

Non sono state trovate suite di test specifiche, build distribuibili o misure delle prestazioni. La documentazione descrive il comportamento ricavabile dai file; funzionamento effettivo e resa sulle diverse piattaforme richiedono una verifica in Unity.

## Possibili sviluppi

Le seguenti sono proposte di evoluzione, non funzionalità già presenti:

1. **Reset completo della sessione:** rimuovere i nemici esistenti, fermare le coroutine precedenti e ripristinare uno stato iniziale coerente.
2. **Stati condivisi fra componenti:** abilitare input e movimento solo nelle fasi previste, includendo una pausa effettiva.
3. **Bersaglio configurabile:** sostituire l'origine fissa con un riferimento esplicito, mantenendo coerenti spawn, grafica e condizione di arrivo.
4. **Feedback di intercettazione:** suoni, effetti brevi e animazioni per rendere evidente l'esito del tocco.
5. **Progressione:** introdurre ondate, velocità variabili o più tipi di nemico dopo aver consolidato il bilanciamento base.
6. **Layout e prove su dispositivo:** verificare camera, UI e dimensione dei bersagli su diversi rapporti e touchscreen.
7. **Ottimizzazione e record:** valutare il pooling se aumenta il numero di nemici e aggiungere il salvataggio del miglior punteggio.

## Asset, crediti e licenza

Il progetto include la fotografia di integratori, due modelli FBX e un materiale per lo scheletro. Nella cartella analizzata non sono presenti attribuzioni complete o un file di licenza che chiarisca origine e condizioni di riuso di tutti gli asset. La loro presenza nel repository non basta a stabilire diritti di redistribuzione.

La documentazione e la cover sono materiali di presentazione aggiunti al repository. La cover riprende i soggetti del progetto con un'interpretazione illustrata: tavolo, integratori e scheletri. Per una distribuzione pubblica del gioco vanno completati i crediti, verificati i diritti degli asset e aggiornati i metadati applicativi del template.
