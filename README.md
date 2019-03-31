# dl24-starter

Repozytorium zawiera szkielet bota grającego w zadania z konkursu Deadline24 Lite.


## Wymagania

Narzędzia:

- .Net Core 2.2

Biblioteki:

- [Serilog](https://serilog.net/)
- [Json.NET](https://www.newtonsoft.com/json)

## Struktura

- *tools*
  - *generate_player.cmd* - skrypt do generacji szablonu nowego bota. Wykorzystuje do generacji projekt *Acme.FooBarPlayer*.
- *src* - katalog z kodami źródłowymi botów
  - *Acme.FooBarServer* - symulator prostego serwera gry.
  - *Acme.FooBarPlayer* - szablon prostego bota.
  - *Chupacabra.PlayerCore* - biblioteka pomocnicza dla botów.
- *players* - miejsce na skompilowane wersje botów. Każdy bot trafia do osobnego podkatalogu.
  - *\<nazwa\>_\<port\>* - katalog z botem *\<nazwa\>* łączącym się z serwerem na porcie *\<port\>*
- *Deploy\_\<nazwa\>\_\<port\>.cmd* - skrypt kopiujący binarki bota *\<nazwa\>* do folderu *players/\<nazwa\>\_\<port\>*


## Jak zacząć?

Gdy w trakcie konkursu DL24 poznamy już treści zadań i konfigurację serwerów testowych to możemy wygenerować szkielety botów. W tym celu dla każdego zadania uruchamiamy raz skrypt *tools\generate_player.cmd* podając:

- nazwę namespacu (najlepiej nazwę zespołu w CamelCase, np. *StinkingBishop*)
- nazwę zadania (również CamelCase, np. *Bonfire*)
- nazwę użytkownika do podłączenia do serwera gry
- hasło do podłączenia do serwera gry
- adres serwera gry (np. *universum.dl24*)
- porty oddzielone przecinkami dla poszczególnych światów gry (np. *20005,20006*)

Skrypt wygeneruje nam nowy projekt bota (np. o nazwie *StinkingBishop.BonfirePlayer*) oraz skrypty deployujące dla każdego z podanych światów.

Możemy teraz otworzyć nowo wygenerowany projekt w Visual Studio, skompilować go w trybie Debug, uruchomić skrypt deployujący i uruchomić bota z katalogu *players/\<nazwa\>\_\<port\>*.


## Co jest w środku?

Bot to aplikacja konsolowa. W konsoli widać informacje generowane przez kod bota. Po naciśnięciu dowolnego klawisza powinno pojawić się okienko z podglądem danych. To co ma się pojawić w tym okienku można ustawić za pomocą metody *Monitor.SetValue*.
Te same wartości powinny być widoczne w pliku *status.txt*.

Wygenerowany bot składa się z dwóch klas. Klasa *\<nazwa\>Engine* jest punktem wejścia do implementacji logiki bota. Klasa *\<nazwa\>Service* jest opakowaniem na protokół komunikacji z serwerem gry. W tej klasie powinny pojawić się metody odpowiadające 1-do-1 komendom przyjmowanym przez serwer gry.
