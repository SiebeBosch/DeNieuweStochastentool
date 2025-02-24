# De Nieuwe Stochastentool
De Nieuwe Stochastentool zoals oorspronkelijk ontwikkeld door Siebe Bosch (Hydroconsult). Deze applicatie is binnen TKI4 doorontwikkeld tot een volwaardige applicatie met support voor het modelinstrumentarium D-Hydro (Deltares). TKI-projecten zijn door het ministerie van EZ gesubsidiëerde innovatieprojecten. 
Zie: https://www.rvo.nl/subsidie-en-financieringswijzer/pps-toeslag-onderzoek-en-innovatie/aansluiten-bij-een-tki.
TKI4 behelst een project van Waterschap Drents Overijsselse Delta i.s.m. kennisinstituut Deltares en Hydroconsult.

Binnen dit TKI is De Nieuwe Stochastentool van Hydroconsult:
- Opensource gemaakt (deze GIT)
- Doorontwikkeld om ook volledige stochastenanalyses te kunnen uitvoeren en nabewerken met het programma D-Hydro.

Het programma is geschreven in VB.NET in Visual Studio 2022, in het .NET framework 4.8.
Daarnaast bevat het codeblokken HTML en javascript ten behoeve van de export naar de webviewer voor resultaten.

De broncode is gepubliceerd onder GPL 3.0. Zie het bijgevoegde bestand LICENSE voor de volledige licentietekst.

## Installatiebestanden
U vind links naar de installatiebestanden (Windows, 64 bit) rechts op deze pagina, onder het kopje 'releases'.

## Documentatie
De documentatie is in zijn geheel opgenomen in deze Github repository en wordt hier ook bijgehouden. U vindt hem hier: https://siebebosch.github.io/DeNieuweStochastentool/.

## Architectuur
De applicatie bestaat uit twee componenten:
- een GUI (gebruiksomgeving)
- een DLL (library)

## Dependencies
Belangrijkste dependency is de MapWinGIS library. Dit is een opensource GIS-component, oorspronkelijk van de University of Idaho. 
Broncodes en binaries van MapWinGIS zijn hier te vinden: https://github.com/MapWindow/MapWinGIS. De eenvoudigste manier om MapWinGIS toe te voegen aan De Nieuwe Stochastentool is door de library separaat te installeren (versie 5.3.0, 64 bit!). Daarna kan vanuit de solution explorer eenvoudig de MapWinGIS.ocx worden toegevoegd onder References.

Een andere relevante dependency is Gembox Spreadsheets. Dit is een externe component voor het exporteren van de stochastenresultaten naar Excel. Voor deze component is een betaalde licentie benodigd. Deze licentiecode vormt geen onderdeel van deze GIT en zal door mede-ontwikkelaars separaat moeten worden aangeschaft. https://www.gemboxsoftware.com/spreadsheet/pricelist

Verder zijn er nog diverse NuGET-packages geconfigureerd, maar dit zal zich vanzelf wijzen wanneer de broncode in Visual Studio wordt ingeladen. De binaries van deze packages zijn te groot om hier op te nemen, dus zullen separaat moeten worden geïnstalleerd.

## Compilen en builden setup
inbegrepen bij deze Git is een Inno setup script. Dit script kan, na compileren in Visual Studio, worden gebruikt om een installatiebestand voor De Nieuwe Stochastentool te vervaardigen. Download en installeer daartoe Inno Setup https://jrsoftware.org/ alsmede Inno Script Studio https://www.kymoto.org/products/inno-script-studio. Stel in Inno Script Studio het pad naar Inno Setup in. Open dan het .iss bestand en draai het script.

Den Haag, 9 januari 2025

Siebe Bosch

Hydroconsult

E-mail: siebe-at-hydroconsult-punt-nl

LinkedIn: https://www.linkedin.com/in/siebebosch/
